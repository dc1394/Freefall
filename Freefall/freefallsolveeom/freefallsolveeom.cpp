/*! \file freefallsolveeom.cpp
    \brief 空気抵抗のある自由落下系に対して運動方程式を解くクラスの実装

    Copyright © 2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "freefallsolveeom.h"
#include <cmath>                                // for std::cos, std::floor, std::log10
#include <cstdio>                               // for std::fclose, std::fflush, std::fopen, std::fwrite
#include <optional>                             // for std::make_optional
#include <queue>                                // for std::queue
#include <string>                               // for std::to_string
#include <tuple>                                // for std::get, std::make_tuple
#include <utility>                              // for std::make_pair  
#include <boost/assert.hpp>                     // for BOOST_ASSERT
#include <boost/math/constants/constants.hpp>   // for boost::math::constants::pi
#include <boost/math/tools/minima.hpp>          // for boost::math::tools::brent_find_minima
#include <boost/math/tools/roots.hpp>           // for boost::math::tools::bisect

namespace freefallsolveeom {
    // #region コンストラクタ・デストラクタ

    FreefallSolveEom::FreefallSolveEom(double dt, double tintervalgraphplot, double eps, double m, double r, double h0, double v0, Ode_Solver_type ode_solver_type) :
        acc_(gsl_interp_accel_alloc(), gsl_interp_accel_deleter),
        dt_(dt),
        eps_(eps),
        fp_(nullptr, std::fclose),
        h0_(h0),
        imax_(static_cast<std::int32_t>(tintervalgraphplot / dt)),
        l2divm2northlatitude45_(sqr(sqr(FreefallSolveEom::R0 + h0) * 2.0 * pi<double>() / (24.0 * 60.0 * 60.0)) * std::cos(pi<double>() * 0.25)),
        m_(m),
        ode_solver_type_(ode_solver_type),
        r_(r),
        spherevolume_(4.0 / 3.0 * boost::math::constants::pi<double>() * r * r * r),
        spline_pressure_(nullptr, gsl_spline_deleter),
        spline_temperature_(nullptr, gsl_spline_deleter),
        tintervalgraphplot_(tintervalgraphplot),
        tintervaloutputcsv_(std::nullopt),
        v0_(v0),
		x_({ R0 + h0, v0 })
    {
        initialize();
    }

    FreefallSolveEom::FreefallSolveEom(double dt, double tintervalgraphplot, double tintervaloutputcsv, std::string const & csvfilename, double eps, double m, double r, double h0, double v0, Ode_Solver_type ode_solver_type) :
        acc_(gsl_interp_accel_alloc(), gsl_interp_accel_deleter),
        dt_(dt),
        eps_(eps),
        fp_(std::unique_ptr< FILE, decltype(&std::fclose) >(std::fopen(csvfilename.c_str(), "w"), std::fclose)),
        h0_(h0),
        imax_(static_cast<std::int32_t>(tintervalgraphplot / dt)),
        l2divm2northlatitude45_(sqr(sqr(FreefallSolveEom::R0 + h0) * 2.0 * pi<double>() / (24.0 * 60.0 * 60.0)) * std::cos(pi<double>() * 0.25)),
        m_(m),
        ode_solver_type_(ode_solver_type),
        outputtocsvdigits_(std::to_string(static_cast<std::int32_t>(std::ceil(std::log10(1.0 / tintervaloutputcsv))))),
        r_(r),
        spherevolume_(4.0 / 3.0 * boost::math::constants::pi<double>() * r * r * r),
        spline_pressure_(nullptr, gsl_spline_deleter),
        spline_temperature_(nullptr, gsl_spline_deleter),
        tintervalgraphplot_(tintervalgraphplot),
        tintervaloutputcsv_(std::make_optional(tintervaloutputcsv)),
        v0_(v0),
        x_({ R0 + h0, v0 })
    {
        initialize();
    }

    // #endregion コンストラクタ・デストラクタ

    // #region publicメンバ関数

    std::tuple< double, double, double, FreefallSolveEom::hmaxtype, FreefallSolveEom::vmaxtype, FreefallSolveEom::tandvtype, FreefallSolveEom::tandvandbooltype > FreefallSolveEom::operator()()
    {
        FreefallSolveEom::state_type result{};
        switch (ode_solver_type_) {
        case Ode_Solver_type::ADAMS_BASHFORTH_MOULTON:
            result = solveeom_run(adams_bashforth_moulton< 2, FreefallSolveEom::state_type >());
            break;

        case Ode_Solver_type::BULIRSCH_STOER:
            result = solveeom_run(bulirsch_stoer< FreefallSolveEom::state_type >(eps_, eps_));
            break;

        case Ode_Solver_type::CONTROLLED_RUNGE_KUTTA:
            result = solveeom_run(make_controlled(eps_, eps_, error_stepper_type()));
            break;

        default:
            BOOST_ASSERT(!"Ode_Solver_typeがあり得ない値になっている！");
            break;
        }

        auto const t = iscalculationfinished_ ? tend_ : t_;
        auto const h = result[0] - FreefallSolveEom::R0;
        auto const v = result[1];

        FreefallSolveEom::hmaxtype stateofhmax = std::nullopt;
        if (hmaxoftandh_)
        {
            stateofhmax = hmaxoftandh_;
        }
        else if (v0_ <= 0.0)
        {
            stateofhmax = std::make_optional(std::make_pair(0.0, h0_));
        }
        
        FreefallSolveEom::vmaxtype stateofvmax = std::nullopt;
        if (vmaxoftandhandv_ && std::fabs(std::get<2>(*vmaxoftandhandv_)) >= std::fabs(v0_))
        {
            stateofvmax = vmaxoftandhandv_;
        }
        else if (vmaxoftandhandv_ || (iscalculationfinished_ && std::fabs(v) < std::fabs(v0_)))
        {
            stateofvmax = std::make_optional(std::make_tuple(0.0, h0_, v0_));    
        }
        else if (iscalculationfinished_ && std::fabs(v) >= std::fabs(v0_))
        {
            stateofvmax = std::make_optional(std::make_tuple(t, h, v));
        }

        return std::make_tuple(t, h, v, stateofhmax, stateofvmax, stateescapeofkarmanline_, stateescapeofexosphere_);
    }

    // #endregion publicメンバ関数

    // #region privateメンバ関数

	std::function<void(FreefallSolveEom::state_type const &, FreefallSolveEom::state_type &, double)> FreefallSolveEom::getEOM() const
    {
        auto const eom = [this](FreefallSolveEom::state_type const & x, FreefallSolveEom::state_type & dxdt, double const)
        {
            // dx/dt = v
            dxdt[0] = x[1];

            // 球に働く引力
            auto const g = 
                // 球に働く重力
                FreefallSolveEom::G * FreefallSolveEom::M / sqr(x[0]) -
                // 球に働く遠心力
                l2divm2northlatitude45_ / (x_[0] * x_[0] * x_[0]);

            // 空気の密度
            auto const rho = get_rho(x[0]);

            // 球に働く力
            auto const f1 = -g +
                // 球に働く浮力
                rho * spherevolume_ * g;

            // 空気の粘性係数
            auto const myu = get_myu(x[0]);

            // レイノルズ数
            auto const Re = 2.0 * r_ * rho * std::fabs(x[1]) / myu;

            // 粘性抵抗
            auto const F = 6.0 * pi<double>() * myu * r_ * x[1];

            // 重力と遠心力のみが働く
            if (Re < FreefallSolveEom::ZERODECISION)
            {
                dxdt[1] = -g;

                return;
            }
            
            // 粘性抵抗のみが働く
            if (Re < FreefallSolveEom::RETHRESHOLD) {
                dxdt[1] = f1 - F / m_;

                return;
            }

            auto const FD = 0.5 * rho * pi<double>() * sqr(r_) * std::fabs(x[1]) * x[1];

            // Drag coefficient
            double CD;

            // N.-S. Cheng, Comparison of formulas for drag coefficient and settling velocity of
            // spherical particles, Powder Technology 189 (2009) 395–398.
            // 2.0*10^-3 < Re < 2.0*10^5
            if (Re < 2.0E+5)
            {
                CD = 24.0 / Re * std::pow(1.0 + 0.27 * Re, 0.43) + 0.47 * (1.0 - std::exp(-0.04 * std::pow(Re, 0.38)));
            }
            // 2.0*10^5 <= Re < 10^6
            else if (Re < 1.0E+6)
            {
                // Almedeij J. Drag coefficient of flow around a sphere: Matching asymptotically the wide
                // trend. std::powder Technology. (2008);doi:10.1016/j.std::powtec.2007.12.006.
                auto const phi1 = std::pow(24.0 / Re, 10) + std::pow(21.0 * std::pow(Re, -0.67), 10) +
                    std::pow(4.0 * std::pow(Re, -0.33), 10) + std::pow(0.4, 10);
                auto const phi2 = 1.0 / (1.0 / std::pow(0.148 * std::pow(Re, 0.11), 10) + 1.0 / std::pow(0.5, 10));
                auto const phi3 = std::pow((1.57E+8) * std::pow(Re, -1.625), 10);
                auto const phi4 = 1.0 / (1.0 / std::pow((6.0E-17) * std::pow(Re, 2.63), 10) + 1.0 / std::pow(0.2, 10));

                CD = std::pow((1.0 / (1.0 / (phi1 + phi2) + 1.0 / phi3) + phi4), 0.1);
            }
            // 10^6 <= Re
            else
            {
                // Clift R, Grace JR, Weber ME. Bubbles, drops, and particles. New York: Academic; 1978
                CD = 0.19 - 8.0E+4 / Re;
            }

            // 慣性抵抗÷(m)
            auto const f2 = FD * CD / m_;

            dxdt[1] = f1 - F / m_ - f2;
        };

        return eom;
    }

    double FreefallSolveEom::get_P(double r) const
    {
        // ジオポテンシャル高度を取得
        auto const H = getGeopotentialFromAltitude(r);

        // 「標準大気 ー 各高度における空気の温度・圧力・密度・音速・粘性係数・動粘性係数の計算式」
        // https://pigeon-poppo.com/standard-atmosphere/ より
        if (H <= 11000.0)
        {
            return 101325.0 * std::pow(288.15 / FreefallSolveEom::get_T(r), -5.256);
        }
        else if (H <= 20000.0)
        {
            return 22632.064 * std::exp(-0.1577 * (FreefallSolveEom::MTOKM * H - 11.0));
        }
        else if (H <= 32000.0)
        {
            return 5474.889 * std::pow(216.65 / FreefallSolveEom::get_T(r), 34.163);
        }
        else if (H <= 47000.0)
        {
            return 868.019 * std::pow(228.65 / FreefallSolveEom::get_T(r), 12.201);
        }
        else if (H <= 51000.0)
        {
            return 110.906 * std::exp(-0.1262 * (FreefallSolveEom::MTOKM * H - 47.0));
        }
        else if (H <= 71000.0)
        {
            return 66.939 * std::pow(270.65 / FreefallSolveEom::get_T(r), -12.201);
        }
        else if (H <= 84852.0)
        {
            return 3.956 * std::pow(214.65 / FreefallSolveEom::get_T(r), -17.082);
        }
        else if (r - FreefallSolveEom::R0 <= 1000000.0)
        {   
            return gsl_spline_eval(spline_pressure_.get(), r - FreefallSolveEom::R0, acc_.get());
        }
        else
        {
            return 0.0;
        }
    }

    double FreefallSolveEom::get_T(double r) const
    {
        // ジオポテンシャル高度を取得
        auto const H = getGeopotentialFromAltitude(r);
        
        // 「標準大気 ー 各高度における空気の温度・圧力・密度・音速・粘性係数・動粘性係数の計算式」
        // https://pigeon-poppo.com/standard-atmosphere/ より
        if (H <= 11000.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE + 15.0 - 6.5 * FreefallSolveEom::MTOKM * H;
        }
        else if (H <= 20000.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE - 56.5;
        }
        else if (H <= 32000.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE - 76.5 + FreefallSolveEom::MTOKM * H;
        }
        else if (H <= 47000.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE - 134.1 + 2.8 * FreefallSolveEom::MTOKM * H;
        }
        else if (H <= 51000.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE - 2.5;
        }
        else if (H <= 71000.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE + 140.3 - 2.8 * FreefallSolveEom::MTOKM * H;
        }
        else if (H <= 84852.0)
        {
            return FreefallSolveEom::CELSIUSTOABSOLUTETEMPERATURE + 83.5 - 2.0 * FreefallSolveEom::MTOKM * H;
        }
        else if (r - FreefallSolveEom::R0 <= 1000000.0)
        {
            return gsl_spline_eval(spline_temperature_.get(), r - FreefallSolveEom::R0, acc_.get());
        }
        else
        {
            return 1000.0;
        }
    }

    void FreefallSolveEom::initialize()
    {
        // 高度80km以上での気圧と温度のデータ
        // 国立天文台編『理科年表』丸善出版（2012）p.331より引用
        p_data_ = { 1.0524,    0.37338,   0.18359,   0.15381,   3.2011E-2, 7.1042E-3,
                    2.5382E-3, 1.2505E-3, 7.2028E-4, 3.0395E-4, 1.5271E-4, 8.4736E-5,
                    2.4767E-5, 8.7704E-6, 3.4498E-6, 1.4518E-6, 6.4468E-7, 3.0236E-7,
                    1.5137E-7, 8.2130E-8, 4.8865E-8, 3.1908E-8, 2.2599E-8, 1.7036E-8,
                    1.3415E-8, 1.0873E-8, 7.5138E-9 };

        t_data_ = { 198.639, 186.87, 186.87, 186.87, 195.08, 240.00, 360.00, 469.27,
                    559.63,  696.29, 790.07, 854.56, 941.33, 976.01, 990.06, 995.83,
                    998.22,  999.24, 999.67, 999.85, 999.93, 999.97, 999.99, 999.99,
                    1000.0,  1000.0, 1000.0 };

        z_mesh_ = { 80000.0,  86000.0,  90000.0,  91000.0,  100000.0, 110000.0,
                    120000.0, 130000.0, 140000.0, 160000.0, 180000.0, 200000.0,
                    250000.0, 300000.0, 350000.0, 400000.0, 450000.0, 500000.0,
                    550000.0, 600000.0, 650000.0, 700000.0, 750000.0, 800000.0,
                    850000.0, 900000.0, 1000000.0 };

        BOOST_ASSERT(p_data_.size() == z_mesh_.size());
        BOOST_ASSERT(t_data_.size() == z_mesh_.size());

        spline_pressure_.reset(gsl_spline_alloc(gsl_interp_cspline, z_mesh_.size()));
        spline_temperature_.reset(gsl_spline_alloc(gsl_interp_cspline, z_mesh_.size()));

        gsl_spline_init(spline_pressure_.get(), z_mesh_.data(), p_data_.data(), z_mesh_.size());
        gsl_spline_init(spline_temperature_.get(), z_mesh_.data(), t_data_.data(), z_mesh_.size());
    }
    
    template <typename Stepper>
    FreefallSolveEom::state_type FreefallSolveEom::solveeom_run(Stepper const & stepper)
    {
        using namespace boost::math::tools;

        if (isfirststep_)
        {
            if (fp_)
            {
                outputresulttocsv(0.0, x_);
            }

            // 0秒目ですでにカーマン・ラインを突破しているかどうか
            if (x_[0] >= FreefallSolveEom::KARMANLINE)
            {
                stateescapeofkarmanline_ = std::make_optional(std::make_pair(0.0, v0_));
            }

            // 0秒目ですでに外気圏を脱出しているかどうか
            if (x_[0] >= FreefallSolveEom::ALTITUDEOFEXOSPHERE)
            {
                // 外気圏を脱出した際に速度が第二宇宙速度以上だったかどうか
                if (x_[1] >= FreefallSolveEom::SECONDESCAPEVELOCITYOFEXOSPHERE)
                {
                    stateescapeofexosphere_ = std::make_optional(std::make_tuple(0.0, v0_, true));

                    // 計算打ち切り
                    iscalculationfinished_ = true;
                    tend_ = 0.0;
                }
                else
                {
                    stateescapeofexosphere_ = std::make_optional(std::make_tuple(0.0, v0_, false));
                }
            }

            isfirststep_ = false;
            return x_;
        }

        std::queue<FreefallSolveEom::state_type> history{};

        for (auto i = 1; i <= imax_; i++) {
            auto const ttmp = static_cast<double>(i) * dt_;

            if (history.size() < 2)
            {
                history.push(x_);
            }
            else
            {
                history.pop();
                history.push(x_);
            }

            integrate_eom(stepper, dt_, x_);

            if (vmaxoftandhandv_ && x_[1] < 0.0)
            {
                BOOST_ASSERT(std::get<2>(*vmaxoftandhandv_) < x_[1]);
            }

            auto const statebefore = history.back();

            // カーマン・ラインを突破した際の時間を探索
            if (!stateescapeofkarmanline_ && x_[0] >= FreefallSolveEom::KARMANLINE)
            {
                auto maxit = MAXITER;
                auto res = bisect(
                    [&stepper, &statebefore, this](double t)
                {
                    auto x = statebefore;
                    integrate_eom(stepper, t, x);
                    return x[0] - FreefallSolveEom::KARMANLINE;
                },
                    0.0,
                    dt_,
                    eps_tolerance<double>(FreefallSolveEom::DIGITS),
                    maxit);

                auto const tescapeofkarmanline = (res.first + res.second) * 0.5;
                auto xtmp(statebefore);
                integrate_eom(stepper, tescapeofkarmanline, xtmp);

                stateescapeofkarmanline_ = std::make_optional(std::make_pair(t_ + static_cast<double>(i - 1) * dt_ + tescapeofkarmanline, xtmp[1]));
            }

            // 外気圏を脱出した際の時間を探索
            if (!stateescapeofexosphere_ && x_[0] >= FreefallSolveEom::ALTITUDEOFEXOSPHERE)
            {
                auto maxit = MAXITER;
                auto res = bisect(
                    [&stepper, &statebefore, this](double t)
                {
                    auto x = statebefore;
                    integrate_eom(stepper, t, x);
                    return x[0] - FreefallSolveEom::ALTITUDEOFEXOSPHERE;
                },
                    0.0,
                    dt_,
                    eps_tolerance<double>(FreefallSolveEom::DIGITS),
                    maxit);

                auto const tescapeofexosphere = (res.first + res.second) * 0.5;
                auto xtmp(statebefore);
                integrate_eom(stepper, tescapeofexosphere, xtmp);

                // 外気圏を脱出した際に速度が第二宇宙速度以上だったかどうか
                if (xtmp[1] >= FreefallSolveEom::SECONDESCAPEVELOCITYOFEXOSPHERE)
                {
                    stateescapeofexosphere_ = std::make_optional(std::make_tuple(t_ + static_cast<double>(i - 1) * dt_ + tescapeofexosphere, xtmp[1], true));

                    // 計算打ち切り
                    iscalculationfinished_ = true;
                    tend_ = t_ + static_cast<double>(i - 1) * dt_ + tescapeofexosphere;

                    return xtmp;
                }
                else
                {
                    stateescapeofexosphere_ = std::make_optional(std::make_tuple(t_ + static_cast<double>(i - 1) * dt_ + tescapeofexosphere, xtmp[1], false));
                }
            }
            
            // 地面に衝突する時の速度とその際の時間を探索
            if (x_[0] < FreefallSolveEom::R0)
            {
                auto maxit = MAXITER;
                auto res = bisect(
                    [&stepper, &statebefore, this](double t)
                {
                    auto x = statebefore;
                    integrate_eom(stepper, t, x);
                    return x[0] - FreefallSolveEom::R0;
                },
                    0.0,
                    dt_,
                    eps_tolerance<double>(FreefallSolveEom::DIGITS),
                    maxit);

                auto const tendtmp = (res.first + res.second) * 0.5;
                auto xtmp(statebefore);
                integrate_eom(stepper, tendtmp, xtmp);

                tend_ = t_ + static_cast<double>(i - 1) * dt_ + tendtmp;
                                
                if (fp_)
                {
                    outputresulttocsv(tend_, xtmp);
                    std::fflush(fp_.get());
                    fp_.reset();
                }
                
                iscalculationfinished_ = true;

                return xtmp;
            }

            // 最高到達高度とその際の時間の探索
            if (statebefore[1] * x_[1] <= 0.0)
            {
                auto maxit = MAXITER;
                auto const res = bisect(
                    [&stepper, &statebefore, this](double t)
                {
                    auto x = statebefore;
                    integrate_eom(stepper, t, x);
                    return x[1];
                },
                    0.0,
                    dt_,
                    eps_tolerance<double>(FreefallSolveEom::DIGITS),
                    maxit);

                auto const thmaxtmp = (res.first + res.second) * 0.5;
                auto xtmp(statebefore);
                integrate_eom(stepper, thmaxtmp, xtmp);

                hmaxoftandh_ = std::make_optional(std::make_pair(t_ + static_cast<double>(i - 1) * dt_ + thmaxtmp, xtmp[0] - FreefallSolveEom::R0));
            }

            // 最高速度とその際の時間と高度の探索
            if (!vmaxoftandhandv_ && x_[1] < 0.0 && x_[1] > statebefore[1])
            {
                auto maxit = MAXITER;
                auto state2before = history.front();
                auto const res = brent_find_minima(
                    [&stepper, &state2before, this](double t)
                {
                    auto x = state2before;
                    integrate_eom(stepper, t, x);
                    return x[1];
                },
                    0.0,
                    2.0 * dt_,
                    FreefallSolveEom::DIGITS,
                    maxit);

                auto xtmp(state2before);
                integrate_eom(stepper, res.first, xtmp);

                BOOST_ASSERT(xtmp[1] < state2before[1]);
                BOOST_ASSERT(xtmp[1] < statebefore[1]);
                BOOST_ASSERT(xtmp[1] < x_[1]);

                vmaxoftandhandv_ = std::make_optional(std::make_tuple(t_ + static_cast<double>(i - 2) * dt_ + res.first, xtmp[0] - FreefallSolveEom::R0, xtmp[1]));
            }

            if (tintervaloutputcsv_ &&
                (std::fabs(ttmp / *tintervaloutputcsv_ - std::floor(ttmp / *tintervaloutputcsv_)) <= FreefallSolveEom::ZERODECISIONTOCSV || std::fabs(ttmp / *tintervaloutputcsv_ - std::ceil(ttmp / *tintervaloutputcsv_)) <= FreefallSolveEom::ZERODECISIONTOCSV))
            {
                outputresulttocsv(t_ + ttmp, x_);
            }
        }

        t_ += tintervalgraphplot_;

        return x_;
    }

    // #endregion privateメンバ関数

    // #region templateメンバ関数の実体化

    template FreefallSolveEom::state_type FreefallSolveEom::solveeom_run<adams_bashforth_moulton< 2, FreefallSolveEom::state_type > >(adams_bashforth_moulton< 2, state_type > const & stepper);
    template FreefallSolveEom::state_type FreefallSolveEom::solveeom_run<bulirsch_stoer < FreefallSolveEom::state_type > >(bulirsch_stoer < state_type > const & stepper);
    template FreefallSolveEom::state_type FreefallSolveEom::solveeom_run<FreefallSolveEom::error_stepper_type>(error_stepper_type const & stepper);

    // #endregion templateメンバ関数の実体化

    // #region staticメンバ変数

    const double FreefallSolveEom::SECONDESCAPEVELOCITYOFEXOSPHERE = std::sqrt(2.0 * FreefallSolveEom::G * FreefallSolveEom::M / FreefallSolveEom::ALTITUDEOFEXOSPHERE);

    // #endregion staticメンバ変数
}
