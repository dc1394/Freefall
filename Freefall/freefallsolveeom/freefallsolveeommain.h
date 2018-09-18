/*! \file freefallsolveeommain.h
    \brief 空気抵抗のある自由落下系に対して運動方程式を解く関数群の宣言

    Copyright © 2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#ifndef _FREEFALLSOLVEEOMMAIN_H_
#define _FREEFALLSOLVEEOMMAIN_H_

#pragma once

#ifdef __cplusplus
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#include "freefallsolveeom.h"
#include <cstdint>              // for std::int32_t    
#include <optional>		        // for std::optional

extern "C" {
    //! A global variable.
    /*!
        SolveEoMクラスのオブジェクトへのポインタ
    */
    static std::optional<freefallsolveeom::FreefallSolveEom> pse;

    //! A global function.
    /*!
        空気抵抗のある自由落下系に対して運動方程式を解いた計算結果を取得する
        \param thmax 最高到達高度の時の時間（秒）へのポインタ
        \param hmax 最高到達高度（m）へのポインタ
        \param tvmax 最高速度の際の時間（秒）へのポインタ
        \param vmax 最高速度（m/s）へのポインタ
    */
    DLLEXPORT void __stdcall getresult(double * thmax, double * hmax, double * tvmax, double * vmax);

    //! A global function.
    /*!
        空気抵抗のある自由落下系に対して運動方程式を解くクラスのコンストラクタ（CSVファイルに結果を出力しない）を呼び出す
        \param dt 常微分方程式の数値解法の時間刻み（秒）
        \param tintervalgraphplot グラフプロット用の時間間隔（秒）
        \param eps 常微分方程式の数値解法の許容誤差
        \param m 球の質量（kg）
        \param r 球の半径（m）
        \param h0 初期高度（m）
        \param v0 初期速度（m/s）
        \param ode_solver_type 常微分方程式の数値解法
    */
    DLLEXPORT void __stdcall init(double dt, double tintervalgraphplot, double eps, double m, double r, double h0, double v0, std::int32_t ode_solver_type);

    //! A global function.
    /*!
        空気抵抗のある自由落下系に対して運動方程式を解くクラスのコンストラクタ（CSVファイルに結果を出力する）を呼び出す
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
    DLLEXPORT void __stdcall initofcsvoutput(double dt, double tintervalgraphplot, double tintervaloutputcsv, char const * csvfilename, double eps, double m, double r, double h0, double v0, std::int32_t ode_solver_type);

    //! A global function.
    /*!
        計算が終了したかどうかを調べる
        \return 計算が終了したかどうか
    */
    DLLEXPORT std::int32_t __stdcall iscalculationfinished();

    //! A global function.
    /*!
        空気抵抗のある自由落下系における運動方程式を与えられた時間だけ解き、状態を求める
        \param t 経過時間へのポインタ（返り値として使用）
        \param h 高度へのポインタ（返り値として使用）
        \param v 速度へのポインタ（返り値として使用）
        \param ishmax 最高到達高度が発見できたかどうかへのポインタ（返り値として使用）
        \param thmax 最高到達高度の際の時間（秒）へのポインタ（返り値として使用）
        \param hmax 最高到達高度の時の高度（m）へのポインタ（返り値として使用）
        \param isvmax 最高速度が発見できたかどうか（返り値として使用）
        \param tvmax 最高速度の際の時間（秒）へのポインタ（返り値として使用）
        \param hvmax 最高速度の際の高度（m）へのポインタ（返り値として使用）
        \param vmax 最高速度（m/s）へのポインタ（返り値として使用）
        \param iskarmanline カーマン・ラインを突破したかどうかへのポインタ（返り値として使用）
        \param tkarmanline カーマン・ラインを突破した際の時間（秒）へのポインタ（返り値として使用）
        \param vkarmanline カーマン・ラインを突破した際の速度（m/s）へのポインタ（返り値として使用）
        \param isexosphere 外気圏を脱出したかどうかへのポインタ（返り値として使用）
        \param texosphere 外気圏を脱出した際の時間（秒）へのポインタ（返り値として使用）
        \param vexosphere 外気圏を脱出した際の速度（m/s）へのポインタ（返り値として使用）
        \param issecondescape 外気圏を脱出した際に速度が第二宇宙速度以上だったかどうかへのポインタ（返り値として使用）
    */
    DLLEXPORT void __stdcall nextstep(double * t, double * h, double * v, bool * ishmax, double * thmax, double * hmax, bool * isvmax, double * tvmax, double * hvmax, double * vmax, bool * iskarmanline, double * tkarmanline, double * vkarmanline, bool * isexosphere, double * texosphere, double * vexosphere, bool * issecondescape);
}

#endif  // _FREEFALLSOLVEEOMMAIN_H_
