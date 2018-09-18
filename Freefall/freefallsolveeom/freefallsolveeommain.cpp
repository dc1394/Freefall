/*! \file freefallsolveeommain.cpp
    \brief 空気抵抗のある自由落下系に対して運動方程式を解く関数群の実装

    Copyright © 2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "freefallsolveeommain.h"
#include <tuple>                    // for std::get

extern "C" {
    void __stdcall init(double dt, double tintervalgraphplot, double eps, double m, double r, double h0, double v0, std::int32_t ode_solver_type)
    {
        pse.emplace(dt, tintervalgraphplot, eps, m, r, h0, v0, static_cast<freefallsolveeom::FreefallSolveEom::Ode_Solver_type>(ode_solver_type));
    }

    void __stdcall initofcsvoutput(double dt, double tintervalgraphplot, double tintervaloutputcsv, char const * csvfilename, double eps, double m, double r, double h0, double v0, std::int32_t ode_solver_type)
    {
        pse.emplace(dt, tintervalgraphplot, tintervaloutputcsv, csvfilename, eps, m, r, h0, v0, static_cast<freefallsolveeom::FreefallSolveEom::Ode_Solver_type>(ode_solver_type));
    }
    
    std::int32_t __stdcall iscalculationfinished()
    {
        return pse->isCalculationFinished() ? 1 : 0;
    }
    
    void __stdcall nextstep(double * t, double * h, double * v, bool * ishmax, double * thmax, double * hmax, bool * isvmax, double * tvmax, double * hvmax, double * vmax, bool * iskarmanline, double * tkarmanline, double * vkarmanline, bool * isexosphere, double * texosphere, double * vexosphere, bool * issecondescape)
    {
        auto const [tres, hres, vres, stateofhmax, stateofvmax, stateofkamanline, stateofexosphere] = (*pse)();

        *t = tres;
        *h = hres;
        *v = vres;

        if (stateofhmax)
        {
            *ishmax = true;
            *thmax = std::get<0>(*stateofhmax);
            *hmax = std::get<1>(*stateofhmax);
        }
        else
        {
            *ishmax = false;
        }

        if (stateofvmax)
        {
            *isvmax = true;
            *tvmax = std::get<0>(*stateofvmax);
            *hvmax = std::get<1>(*stateofvmax);
            *vmax = std::get<2>(*stateofvmax);
        }
        else
        {
            *isvmax = false;
        }

        if (stateofkamanline)
        {
            *iskarmanline = true;
            *tkarmanline = std::get<0>(*stateofkamanline);
            *vkarmanline = std::get<1>(*stateofkamanline);
        }
        else
        {
            *iskarmanline = false;
        }

        if (stateofexosphere)
        {
            *isexosphere = true;
            *texosphere = std::get<0>(*stateofexosphere);
            *vexosphere = std::get<1>(*stateofexosphere);
            *issecondescape = std::get<2>(*stateofexosphere);
        }
        else
        {
            *isexosphere = false;
            *issecondescape = false;
        }
    }
}
