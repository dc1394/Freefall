//-----------------------------------------------------------------------
// <copyright file="FreefallSolveEom.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.Linq;
    using MyLogic;
    using OxyPlot;
    using OxyPlot.Series;

    /// <summary>
    /// 空気抵抗のある自由落下系に対して運動方程式を1ステップ解くクラス
    /// </summary>
    internal class FreefallSolveEom
    {
        #region フィールド

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸の単位をkmにするかどうかの閾値
        /// </summary>
        private static readonly Double 速度km_sグラフ閾値 = 50000.0;

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸の単位をkmにするかどうかの閾値
        /// </summary>
        private static readonly Double 高度kmグラフ閾値 = 50000.0;

        /// <summary>
        /// 経過時間－高度グラフのプロット用のLineSeries
        /// </summary>
        private readonly LineSeries altitudeLineSeries;

        /// <summary>
        /// 経過時間－速度グラフのプロット用のLineSeries
        /// </summary>
        private readonly LineSeries velocityLineSeries;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// 空気抵抗のある自由落下系に対して運動方程式を1ステップ解くクラスのコンストラクタ
        /// </summary>
        /// <param name="sd">保存された設定情報のオブジェクト</param>
        /// <param name="altitudeLineSeries">経過時間－高度グラフのプロット用のLineSeries</param>
        /// <param name="velocityLineSeries">経過時間－速度グラフのプロット用のLineSeries</param>
        /// <param name="dt">常微分方程式の数値解法の時間刻み（秒）</param>
        /// <param name="tintervalgraphplot">グラフプロット用の時間間隔（秒）</param>
        /// <param name="eps">常微分方程式の数値解法の許容誤差</param>
        /// <param name="m">球の質量（kg）</param>
        /// <param name="r">球の半径（m）</param>
        /// <param name="h0">初期高度（m）</param>
        /// <param name="v0">初期速度（m/s）</param>
        /// <param name="odesolver">常微分方程式の数値解法</param>
        internal FreefallSolveEom(SaveDataManage.SaveData sd, LineSeries altitudeLineSeries, LineSeries velocityLineSeries, Double dt, Double tintervalgraphplot, Double eps, Double m, Double r, Double h0, Double v0, Int32 odesolver)
        {
            this.altitudeLineSeries = altitudeLineSeries;

            this.velocityLineSeries = velocityLineSeries;

            if (sd.IsOutputToCsvFile)
            {
                Double.TryParse(sd.IntervalOfOutputToCsvFile, out Double tintervaloutputcsv);
                UnsafeNativeMethods.FreefallInit(dt, tintervalgraphplot, tintervaloutputcsv, sd.CsvFileNameFullPath, eps, m, r, h0, v0, odesolver);
            }
            else
            {
                UnsafeNativeMethods.FreefallInit(dt, tintervalgraphplot, eps, m, r, h0, v0, odesolver);
            }
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸の単位をkmにするかどうか
        /// </summary>
        internal static Boolean Is高度単位km { get; set; }

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸の単位をkmにするかどうか
        /// </summary>
        internal static Boolean Is速度単位km_s { get; set; }

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸の単位をkmに変更したかどうかのフラグ
        /// </summary>
        internal static Boolean 高度単位km変更フラグ { get; set; }

        /// <summary>
        /// 経過時間－速度の関係のグラフの縦軸の単位をkm/sに変更したかどうかのフラグ
        /// </summary>
        internal static Boolean 速度単位km_s変更フラグ { get; set; }

        /// <summary>
        /// 物体が地面に衝突したときの時間と速度
        /// </summary>
        internal DataPoint 地面衝突時間And速度 { get; private set; }

        #endregion プロパティ

        #region メソッド

        /// <summary>
        /// 空気抵抗のある自由落下系における運動方程式を与えられた時間だけ解き、状態を求める
        /// </summary>
        /// <returns>系の状態が詰まったSystem.ValueTuple</returns>
        public (Double t, Double h, Double v, (Double, Double)?, (Double, Double, Double)?, (Double, Double)?, (Double, Double, Boolean)?) FreefallSolveEomNextStep()
        {
            var (t, h, v, hmaxstate, vmaxstate, stateofkarmanline, stateofexosphere) = UnsafeNativeMethods.FreefallSolveEomNextStep();
            
            if (!FreefallSolveEom.高度単位km変更フラグ && h >= FreefallSolveEom.高度kmグラフ閾値)
            {
                FreefallSolveEom.Is高度単位km = true;
                FreefallSolveEom.高度単位km変更フラグ = true;

                lock (this.altitudeLineSeries)
                {
                    var altitudeDataPoints = this.altitudeLineSeries.Points
                                                 .Select(item => new DataPoint(item.X, item.Y / 1000.0))
                                                 .ToList();
                    this.altitudeLineSeries.Points.Clear();
                    altitudeDataPoints.ForEach(item => this.altitudeLineSeries.Points.Add(item));
                }
            }
            
            if (!FreefallSolveEom.速度単位km_s変更フラグ && Math.Abs(v) >= FreefallSolveEom.速度km_sグラフ閾値)
            {
                FreefallSolveEom.Is速度単位km_s = true;
                FreefallSolveEom.速度単位km_s変更フラグ = true;

                lock (this.velocityLineSeries)
                {
                    var velocityDataPoints = this.velocityLineSeries.Points
                                                 .Select(item => new DataPoint(item.X, item.Y / 1000.0))
                                                 .ToList();
                    this.velocityLineSeries.Points.Clear();
                    velocityDataPoints.ForEach(item => this.velocityLineSeries.Points.Add(item));
                }
            }
            
            // LineSeriesに結果を格納
            lock (this.altitudeLineSeries)
            {
                if (FreefallSolveEom.Is高度単位km)
                {
                    h /= 1000.0;
                }
            
                this.altitudeLineSeries.Points.Add(new DataPoint(t, h));
            }
            
            // LineSeriesに結果を格納
            lock (this.velocityLineSeries)
            {
                if (FreefallSolveEom.Is速度単位km_s)
                {
                    v /= 1000.0;
                }
            
                this.velocityLineSeries.Points.Add(new DataPoint(t, v));
            }
                        
            // 物体が地面に衝突したときの時間と速度を記録
            lock (this.velocityLineSeries)
            {
                this.地面衝突時間And速度 = this.velocityLineSeries.Points.Last();
            }

            if (FreefallSolveEom.Is速度単位km_s)
            {
                // 速度の単位をm/sに戻す
                this.地面衝突時間And速度 = new DataPoint(this.地面衝突時間And速度.X, this.地面衝突時間And速度.Y * 1000.0);
            }

            return (t, h, v, hmaxstate, vmaxstate, stateofkarmanline, stateofexosphere);
        }

        #endregion メソッド
    }
}
