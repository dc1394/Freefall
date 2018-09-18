//-----------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.Runtime.InteropServices;
    
    /// <summary>
    /// DLLから関数を呼び出すクラス
    /// </summary>
    internal static class UnsafeNativeMethods
    {
        #region メソッド

        /// <summary>
        /// 空気抵抗のある自由落下系における運動方程式を与えられた時間だけ解き、状態を求める
        /// </summary>
        /// <returns>系の状態が詰まったSystem.ValueTuple</returns>
        internal static unsafe (Double , Double , Double , (Double, Double)?, (Double, Double, Double)?, (Double, Double)?, (Double, Double, Boolean)?) FreefallSolveEomNextStep()
        {
            Double t, h, v, thmax, hmax, tvmax, hvmax, vmax, tkarmanline, vkarmanline, texosphere, vexosphere;
            Boolean ishmax, isvmax, iskarmanline, isexosphere, issecondescape;
            UnsafeNativeMethods.NextStep(&t, &h, &v, &ishmax, &thmax, &hmax, &isvmax, &tvmax, &hvmax, &vmax, &iskarmanline, &tkarmanline, &vkarmanline, &isexosphere, &texosphere, &vexosphere, &issecondescape);

            (Double, Double)? stateofhmax = null;
            if (ishmax)
            {
                stateofhmax = (thmax, hmax);
            }

            (Double, Double, Double)? stateofvmax = null;
            if (isvmax)
            {
                stateofvmax = (tvmax, hvmax, vmax);
            }

            (Double, Double)? stateofkarmnline = null;
            if (iskarmanline)
            {
                stateofkarmnline = (tkarmanline, vkarmanline);
            }

            (Double, Double, Boolean)? stateofexosphere = null;
            if (isexosphere)
            {
                stateofexosphere = (texosphere, vexosphere, issecondescape);
            }

            return (t, h, v, stateofhmax, stateofvmax, stateofkarmnline, stateofexosphere);
        }

        /// <summary>
        /// 空気抵抗のある自由落下系に対して運動方程式を解くクラスのコンストラクタ（CSVファイルに結果を出力しない）を呼び出す
        /// </summary>
        /// <param name="dt">常微分方程式の数値解法の時間刻み（秒）</param>
        /// <param name="tintervalgraphplot">グラフプロット用の時間間隔（秒）</param>
        /// <param name="eps">常微分方程式の数値解法の許容誤差</param>
        /// <param name="m">球の質量（kg）</param>
        /// <param name="r">球の半径（m）</param>
        /// <param name="h0">初期高度（m）</param>
        /// <param name="v0">初期速度（m/s）</param>
        /// <param name="odeSolverType">常微分方程式の数値解法</param>
        [DllImport("freefallsolveeom", EntryPoint = "init")]
        internal static extern void FreefallInit(Double dt, Double tintervalgraphplot, Double eps, Double m, Double r, Double h0, Double v0, Int32 odeSolverType);

        /// <summary>
        /// 空気抵抗のある自由落下系に対して運動方程式を解くクラスのコンストラクタ（CSVファイルに結果を出力する）を呼び出す
        /// </summary>
        /// <param name="dt">常微分方程式の数値解法の時間刻み（秒）</param>
        /// <param name="tintervalgraphplot">グラフプロット用の時間間隔（秒）</param>
        /// <param name="tintervaloutputcsv">CSVファイル出力用の時間間隔（秒）</param>
        /// <param name="csvfilename">出力するCSVファイルのファイル名</param>
        /// <param name="eps">常微分方程式の数値解法の許容誤差</param>
        /// <param name="m">球の質量（kg）</param>
        /// <param name="r">球の半径（m）</param>
        /// <param name="h0">初期高度（m）</param>
        /// <param name="v0">初期速度（m/s）</param>
        /// <param name="odeSolverType">常微分方程式の数値解法</param>
        [DllImport("freefallsolveeom", EntryPoint = "initofcsvoutput", CharSet = CharSet.Ansi)]
        internal static extern void FreefallInit(Double dt, Double tintervalgraphplot, Double tintervaloutputcsv, String csvfilename, Double eps, Double m, Double r, Double h0, Double v0, Int32 odeSolverType);

        /// <summary>
        /// 計算が終了したかどうかを調べる
        /// </summary>
        /// <returns>計算が終了していたら1、していなかったら0</returns>
        [DllImport("freefallsolveeom", EntryPoint = "iscalculationfinished")]
        internal static extern Int32 IsCalculationFinished();

        /// <summary>
        /// 空気抵抗のある自由落下系における運動方程式を与えられた時間だけ解き、状態を求める
        /// </summary>
        /// <param name="t">経過時間へのポインタ（返り値として使用）</param>
        /// <param name="h">高度へのポインタ（返り値として使用）</param>
        /// <param name="v">速度へのポインタ（返り値として使用）</param>
        /// <param name="ishmax">最高到達高度が発見できたかどうかへのポインタ（返り値として使用）</param>
        /// <param name="thmax">最高到達高度の際の時間（秒）へのポインタ（返り値として使用）</param>
        /// <param name="hmax">最高到達高度の時の高度（m）へのポインタ（返り値として使用）</param>
        /// <param name="isvmax">最高速度が発見できたかどうか（返り値として使用）</param>
        /// <param name="tvmax">最高速度の際の時間（秒）へのポインタ（返り値として使用）</param>
        /// <param name="hvmax">最高速度の際の高度（m）へのポインタ（返り値として使用）</param>
        /// <param name="vmax">最高速度（m/s）へのポインタ（返り値として使用）</param>
        /// <param name="iskarmanline"> カーマン・ラインを突破したかどうかへのポインタ（返り値として使用）</param>
        /// <param name="tkarmanline">カーマン・ラインを突破した際の時間（秒）へのポインタ（返り値として使用）</param>
        /// <param name="vkarmanline">カーマン・ラインを突破した際の速度（m/s）へのポインタ（返り値として使用）</param>
        /// <param name="isexosphere">外気圏を脱出したかどうかへのポインタ（返り値として使用）</param>
        /// <param name="texosphere">外気圏を脱出した際の時間（秒）へのポインタ（返り値として使用）</param>
        /// <param name="vexosphere">外気圏を脱出した際の速度（m/s）へのポインタ（返り値として使用）</param>
        /// <param name="issecondescape">外気圏を脱出した際に速度が第二宇宙速度以上だったかどうかへのポインタ（返り値として使用）</param>
        [DllImport("freefallsolveeom", EntryPoint = "nextstep")]
        private static extern unsafe void NextStep(Double * t, Double * h, Double * v, Boolean * ishmax, Double * thmax, Double * hmax, Boolean * isvmax, Double * tvmax, Double * hvmax, Double * vmax, Boolean * iskarmanline, Double * tkarmanline, Double * vkarmanline, Boolean * isexosphere, Double * texosphere, Double * vexosphere, Boolean * issecondescape);

        #endregion メソッド
    }
}
