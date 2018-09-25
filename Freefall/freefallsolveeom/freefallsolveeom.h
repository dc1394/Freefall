/*! \file freefallsolveeom.h
    \brief 空気抵抗のある自由落下系に対して運動方程式を解くクラスの宣言

    Copyright © 2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/

#ifndef _FREEFALLSOLVEEOM_H_
#define _FREEFALLSOLVEEOM_H_

#pragma once

#include "utility/deleter.h"
#include <array>                        // for std::array
#include <cstdint>                      // for std::int32_t
#include <cstdio>                       // for std::fclose
#include <functional>                   // for std::function
#include <memory>                       // for std::unique_ptr
#include <optional>                     // for std::optional
#include <tuple>                        // for std::tuple
#include <utility>                      // for std::pair
#include <boost/cstdint.hpp>            // for boost::uintmax_t
#include <boost/numeric/odeint.hpp>     // for boost::numeric::odeint

namespace freefallsolveeom {
    using namespace boost::math::constants;
    using namespace boost::numeric::odeint;

    //! A class.
    /*!
        空気抵抗のある自由落下系に対して運動方程式を解くクラスの宣言
    */
    class FreefallSolveEom final {
    public:
        // #region 列挙型

        //!  A enumerated type
        /*!
            微分方程式の解法の種類を表す列挙型
        */
        enum class Ode_Solver_type : std::int32_t {
            // Adams Bashforth Moulton法
            ADAMS_BASHFORTH_MOULTON,
            // Bulirsch-Stoer法
            BULIRSCH_STOER,
            // コントロールされたRunge-Kutta法
            CONTROLLED_RUNGE_KUTTA
        };

        // #endregion 列挙型

        // #region 型エイリアス

        //! A typedef.
        /*!
            最高到達高度の際の時間と高度のstd::pairのstd::optionalの型
        */
        using hmaxtype = std::optional< std::pair<double, double> >;

        //! A typedef.
        /*!
            カーマン・ラインを突破した際の時間と速度のstd::pairのstd::optionalの型
        */
        using tandvtype = std::optional< std::pair<double, double> >;

        //! A typedef.
        /*!
            外気圏を脱出した際の時間と速度とその時に第二宇宙速度を超えていたかどうかのstd::tupleのstd::optionalの型
        */
        using tandvandbooltype = std::optional< std::tuple<double, double, bool> >;

        //! A typedef.
        /*!
            最高速度の際の時間と速度と高度のstd::tupleのstd::optionalの型
        */
        using vmaxtype = std::optional< std::tuple<double, double, double> >;

    private:
        //! A typedef.
        /*!
            2階常微分方程式の状態の型
        */
        using state_type = std::array<double, 2>;

        //! A typedef.
        /*!
            Runge-Kutta法の誤差のコントロールの型
        */
        using error_stepper_type = runge_kutta_dopri5< state_type >;
        
        // #endregion 型エイリアス

        // #region コンストラクタ・デストラクタ

    public:
        //! A constructor.
        /*!
            CSVファイルに計算結果を出力しない場合のコンストラクタ
            \param dt 常微分方程式の数値解法の時間刻み（秒）
            \param tintervalgraphplot グラフプロット用の時間間隔（秒）
            \param eps 常微分方程式の数値解法の許容誤差
            \param m 球の質量（kg）
            \param r 球の半径（m）
            \param h0 初期高度（m）
            \param v0 初期速度（m/s）
            \param ode_solver_type 常微分方程式の数値解法
        */
        FreefallSolveEom(double dt, double tintervalgraphplot, double eps, double m, double r, double h0, double v0, FreefallSolveEom::Ode_Solver_type ode_solver_type);

        //! A constructor.
        /*!
            CSVファイルに計算結果を出力する場合のコンストラクタ
            \param dt 常微分方程式の数値解法の時間刻み（秒）
            \param tintervalgraphplot グラフプロット用の時間間隔（秒）
            \param tintervaloutputcsv CSVファイル出力用の時間間隔（秒）
            \param csvfilename 出力するCSVファイルのファイル名
            \param eps 常微分方程式の数値解法の許容誤差
            \param m 球の質量（kg）
            \param r 球の半径（m）
            \param h0 初期高度（m）
            \param v0 初期速度（m/s）
            \param ode_solver_type 常微分方程式の数値解法
        */
        FreefallSolveEom(double dt, double tintervalgraphplot, double tintervaloutputcsv, std::string const & csvfilename, double eps, double m, double r, double h0, double v0, FreefallSolveEom::Ode_Solver_type ode_solver_type);

        //! A destructor.
        /*!
            デフォルトデストラクタ
        */
        ~FreefallSolveEom() = default;

        // #endregion コンストラクタ・デストラクタ

        // #region publicメンバ関数

        //! A public member function (const).
        /*!
            計算が終了したかどうかを返す
            \return 計算が終了したかどうか
        */
        bool isCalculationFinished() const
        {
            return iscalculationfinished_;
        }

        //! A public member function.
        /*!
            運動方程式を、tintervalgraphplot秒ぶん積分する
            \return 経過時間、高度、速度、最高到達高度の時の状態、最高速度の際の状態、カーマンラインを脱出した際の状態、外気圏を脱出した際の状態
        */
        std::tuple< double, double, double, FreefallSolveEom::hmaxtype, FreefallSolveEom::vmaxtype, FreefallSolveEom::tandvtype, FreefallSolveEom::tandvandbooltype > operator()();

        // #endregion publicメンバ関数

    private:
        // #region private staticメンバ関数

        //! A static private member function.
        /*!
            地球中心からの距離r（m）からジオポテンシャル高度H（m）を取得する
            \param r 地球中心からの距離（m）
            \return ジオポテンシャル高度（m）
        */
        static double getGeopotentialFromAltitude(double r)
        {
            // 「標準大気 ー 各高度における空気の温度・圧力・密度・音速・粘性係数・動粘性係数の計算式」
            // https://pigeon-poppo.com/standard-atmosphere/ より
            return FreefallSolveEom::R0 * (r - FreefallSolveEom::R0) / r;
        }

        // #region private staticメンバ関数

        // #region privateメンバ関数

        //! A private member function (const).
        /*!
            空気の粘性係数（N･s/m²)を返す
            \param r 地球中心からの距離r（m）
            \return 空気の粘性係数（N･s/m²)
        */
        double get_myu(double r) const
        {
            // 「標準大気 ー 各高度における空気の温度・圧力・密度・音速・粘性係数・動粘性係数の計算式」
            // https://pigeon-poppo.com/standard-atmosphere/ より

            // サザーランドの定数
            auto constexpr S = 110.4;

            // 空気の絶対温度（K)
            auto const T = get_T(r);
            return 1.458E-6 * std::pow(T, 1.5) / (T + S);
        }

        //! A private member function (const).
        /*!
            空気の圧力（Pa)を返す
            \param r 地球中心からの距離r（m）
            \return 空気の圧力（Pa）
        */
        double get_P(double r) const;

        //! A private member function (const).
        /*!
            空気の密度（kg/m³)を返す
            \param r 地球中心からの距離r（m）
            \return 空気の密度（kg/m³)
        */
        double get_rho(double r) const
        {
            // 「標準大気 ー 各高度における空気の温度・圧力・密度・音速・粘性係数・動粘性係数の計算式」
            // https://pigeon-poppo.com/standard-atmosphere/ より
            return 0.00348368 * get_P(r) / get_T(r);
        }

        //! A private member function (const).
        /*!
            空気の絶対温度（K)を返す
            \param r 地球中心からの距離r（m）
            \return 空気の絶対温度（K）
        */
        double get_T(double r) const;

        //! A private member function.
        /*!
            メンバ変数を初期化する（コンストラクタの中身）
        */
        void initialize();

        template <typename Stepper>
        //! A private member function.
        /*!
            運動方程式を時刻tまで積分する
            \param stepper 数値積分のステッパー
            \param t 時刻
            \param x 位置と速度が格納されたstd::array
        */
        void integrate_eom(Stepper const & stepper, double t, state_type & x);

        //! A private member function.
        /*!
            計算結果をcsvファイルに出力する
            \param t 経過時間（秒）
            \param x 常微分方程式の状態（現在の地球中心からの距離と速度）
        */
        void outputresulttocsv(double t, state_type const & x) const
        {
            std::fprintf(fp_.get(), ("%." + outputtocsvdigits_ + "f, %.2f, %.2f\n").c_str(), t, x[0] - FreefallSolveEom::R0, x[1]);
        }

        template <typename Stepper>
        //! A private member function.
        /*!
            空気抵抗のある自由落下系における運動方程式をdtgraphplot秒ぶんだけ解く
            \param stepper 数値積分のステッパー
            \return 位置と速度が格納されたstate_type
        */
        FreefallSolveEom::state_type solveeom_run(Stepper const & stepper);

        // #endregion privateメンバ関数

        // #region staticメンバ変数

        //! A private static member variable (constant expression).
        /*!
            摂氏から絶対温度に変換するときの定数
        */
        static auto constexpr CELSIUSTOABSOLUTETEMPERATURE = 273.15;

        //! A private static member variable (constant expression).
        /*!
            方程式の根や最小値を見つけるときの精度のビット
        */
        static auto constexpr DIGITS = std::numeric_limits<double>::digits;

        //! A private static member variable (constant expression).
        /*!
            万有引力定数（m³kg¯¹s¯²）
        */
        static auto constexpr G = 6.6740831E-11;
                
        //! A private static member variable (constant expression).
        /*!
            北緯45度での地球の半径（m）
        */
        static auto constexpr R0 = 6356766.0;

        //! A private static member variable (constant expression).
        /*!
            外気圏の高さ（高度10000km）の、地球中心からの距離
        */
        static auto constexpr ALTITUDEOFEXOSPHERE = 10000000.0 + FreefallSolveEom::R0;

        //! A private static member variable (constant expression).
        /*!
            カーマン・ライン（高度100km）の、地球中心からの距離
        */
        static auto constexpr KARMANLINE = 100000.0 + FreefallSolveEom::R0;

        //! A private static member variable (constant expression).
        /*!
            レイノルズ数の閾値
        */
        static auto constexpr RETHRESHOLD = 2.0E-3;

        //! A private static member variable (constant expression).
        /*!
            地球の質量（kg）
        */
        static auto constexpr M = 5.97243E+24;

        //! A private static member variable (constant expression).
        /*!
            方程式の根や最小値を見つけるときの繰り返しの最高値
        */
        static boost::uintmax_t constexpr MAXITER = 1000;

        //! A private static member variable (constant expression).
        /*!
            メートルをキロメートルに変換するときの定数
        */
        static auto constexpr MTOKM = 0.001;
                
        //! A private static member variable (constant expression).
        /*!
            0判定用の定数
        */
        static auto constexpr ZERODECISION = 1.0E-14;

        //! A private static member variable (constant expression).
        /*!
            CSVファイル出力用の0判定用の定数
        */
        static auto constexpr ZERODECISIONTOCSV = 1.0E-11;

        //! A private static member variable (constant).
        /*!
            外気圏最上部の高度での第二宇宙速度
        */
        static const double SECONDESCAPEVELOCITYOFEXOSPHERE;
        
        // #endregion staticメンバ変数

        // #region メンバ変数

        //! A private member variable.
        /*!
            gsl_interp_accelへのスマートポインタ
        */
        std::unique_ptr<gsl_interp_accel, decltype(gsl_interp_accel_deleter)> const acc_;

        //! A private member variable.
        /*!
            時間のカウント
        */
        double cnt_ = 0;

        //! A private member variable (constant).
        /*!
            常微分方程式の数値解法の時間刻み（秒）
        */
        double const dt_;

        //! A private member variable (constant).
        /*!
            常微分方程式の数値解法の許容誤差
        */
        double const eps_;

        //! A private member variable.
        /*!
            CSVファイル出力用のファイルポインタのスマートポインタ
        */
        std::unique_ptr< FILE, decltype(&std::fclose) > fp_;

        //! A private member variable (constant).
        /*!
            初期高度（m）
        */
        double const h0_;

        //! A private member variable.
        /*!
            最高到達高度のときの時間（秒）と高度（m）のstd::pairのstd::optional
        */
        FreefallSolveEom::hmaxtype hmaxoftandh_ = std::nullopt;

        //! A private member variable (constant).
        /*!
            積分時のカウンタの最大値
        */
        std::int32_t const imax_;

        //! A private member variable.
        /*!
            計算が終わったかどうか
        */
        bool iscalculationfinished_ = false;

        //! A private member variable.
        /*!
            最初のステップかどうか
        */
        bool isfirststep_ = true;
        
        //! A private member variable (constant).
        /*!
            物体の角運動量Lの2乗を質量mの2乗で割り、北緯45度地点に修正した定数（m⁴s¯²）
        */
        double const l2divm2northlatitude45_;
        
        //! A private member variable (constant).
        /*!
            球の質量（kg）
        */
        double const m_;
        
        //! A private member variable (constant).
        /*!
            常微分方程式の数値解法
        */
        FreefallSolveEom::Ode_Solver_type const ode_solver_type_;

        //! A private member variable (constant).
        /*!
            CSVファイルに記録するときの、小数点以下の桁数
        */
        std::string outputtocsvdigits_;

        //! A private member variable.
        /*!
            圧力Pのデータが格納された可変長配列
        */
        std::vector<double> p_data_;

        //! A private member variable (constant).
        /*!
			投げる球の半径（m）
        */
        double const r_;

        //! A private member variable (constant).
        /*!
            球の体積（m³）
        */
        double const spherevolume_;

        //! A private member variable.
        /*!
            gsl_interp_typeへのスマートポインタ
        */
        std::unique_ptr<gsl_spline, decltype(gsl_spline_deleter)> spline_pressure_;

        //! A private member variable.
        /*!
            gsl_interp_typeへのスマートポインタ
        */
        std::unique_ptr<gsl_spline, decltype(gsl_spline_deleter)> spline_temperature_;

        //! A private member variable.
        /*!
            外気圏を脱出した際の時間（秒）と速度（m/s）とその時に第二宇宙速度を超えていたかどうかのstd::tupleのstd::optional
        */
        FreefallSolveEom::tandvandbooltype stateescapeofexosphere_ = std::nullopt;

        //! A private member variable.
        /*!
            カーマン・ラインを突破した際の時間（秒）と速度（m/s）のstd::pairのstd::optional
        */
        FreefallSolveEom::tandvtype stateescapeofkarmanline_ = std::nullopt;

        //! A private member variable.
        /*!
            経過時間（秒）
        */
        double t_ = 0.0;

        //! A private member variable.
        /*!
            温度Tのデータが格納された可変長配列
        */
        std::vector<double> t_data_;
                
        //! A private member variable.
        /*!
            地上落下時の時間（秒）
        */
        double tend_ = 0.0;
               
        //! A private member variable (constant).
        /*!
            グラフプロット用の時間間隔（秒）
        */
        double const tintervalgraphplot_;

        //! A private member variable (constant).
        /*!
            CSVファイル出力用の時間間隔（秒）
        */
        std::optional<double> const tintervaloutputcsv_;

        //! A private member variable.
        /*!
            tintervalgraphplot_より、tintervaloutputcsvの方が大きいかどうか
        */
        std::optional<bool> const islargertintervaloutputcsv_;

        //! A private member variable (constant).
        /*!
            初期速度（m/s）
        */
        double const v0_;

        //! A private member variable.
        /*!
            最高速度のときの時間（秒）と高度（m）と速度（m/s）とのstd::pairのstd::optional
        */
        FreefallSolveEom::vmaxtype vmaxoftandhandv_ = std::nullopt;
        
        //! A private member variable.
        /*!
            微分方程式の現在の状態
        */
        FreefallSolveEom::state_type x_;

        //! A private member variable.
        /*!
            高度zのメッシュが格納された可変長配列
        */
        std::vector<double> z_mesh_;

        // #endregion メンバ変数

        // #region 禁止されたコンストラクタ・メンバ関数

    public:
        //! A public constructor (deleted).
        /*!
            デフォルトコンストラクタ（禁止）
        */
        FreefallSolveEom() = delete;

        //! A public copy constructor (deleted).
        /*!
            コピーコンストラクタ（禁止）
        */
        FreefallSolveEom(FreefallSolveEom const &) = delete;

        //! A public member function (deleted).
        /*!
            operator=()の宣言（禁止）
            \return コピー元のオブジェクト
        */
        FreefallSolveEom & operator=(FreefallSolveEom const &) = delete;

        // #endregion 禁止されたコンストラクタ・メンバ関数
    };
    
    // #region template関数の実装

    //! A function.
    /*!
        値を二乗する関数
        \param x 値
        \return xを二乗した値
    */
    template <typename T>
    T sqr(T x)
    {
        return x * x;
    }

    template <typename Stepper>
    //! A private member function.
    /*!
        運動方程式を時刻tまで積分する
        \param stepper 数値積分のステッパー
        \param t 時刻
        \param x 位置と速度が格納されたstd::array
    */
    void FreefallSolveEom::integrate_eom(Stepper const & stepper, double t, FreefallSolveEom::state_type & x)
    {
        integrate_adaptive(
            stepper,
            [this](FreefallSolveEom::state_type const & x, FreefallSolveEom::state_type & dxdt, double const)
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
            },
            x,
            0.0,
            t,
            dt_);
    }

    // #endregion template関数の実装
}

#endif  // _FREEFALLSOLVEEOM_H_
