//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using MyLogic;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        #region フィールド

        /// <summary>
        /// 「Freefall - キャンセル待機中」の固定文字列
        /// </summary>
        private static readonly String Freefallキャンセル待機中 = "Freefall - キャンセル待機中";

        /// <summary>
        /// 「Freefall - 実行待機中」の固定文字列
        /// </summary>
        private static readonly String Freefall実行待機中 = "Freefall - 実行待機中";

        /// <summary>
        /// 「Freefall - 計算中」の固定文字列
        /// </summary>
        private static readonly String Freefall計算中 = "Freefall - 計算中";
        
        /// <summary>
        /// 「から投げる」の固定文字列
        /// </summary>
        private static readonly String から投げるテキスト = "から投げる";

        /// <summary>
        /// 「から落とす」の固定文字列
        /// </summary>
        private static readonly String から落とすテキスト = "から落とす";

        /// <summary>
        /// 「計算所要時間：」の固定文字列
        /// </summary>
        private static readonly String 計算所要時間テキスト = "計算所要時間：";

        /// <summary>
        /// 「最高速度：」の固定文字列
        /// </summary>
        private static readonly String 最高速度テキスト = "最高速度：";

        /// <summary>
        /// 「最高到達高度：」の固定文字列
        /// </summary>
        private static readonly String 最高到達高度テキスト = "最高到達高度：";

        /// <summary>
        /// 最高速度の単位をkm/sにするかどうかの閾値
        /// </summary>
        private static readonly Double 速度km_s閾値 = 10000.0;

        /// <summary>
        /// 最高到達高度の単位をkmにするかどうかの閾値
        /// </summary>
        private static readonly Double 高度km閾値 = 10000.0;

        /// <summary>
        /// タイマーの時間間隔
        /// </summary>
        private static readonly Int32 TimerInterval = 50;
        
        /// <summary>
        /// 時間計測用のストップウォッチオブジェクト
        /// </summary>
        private readonly Stopwatch sw = new Stopwatch();

        /// <summary>
        /// 経過時間－高度グラフのプロット用のLineSeries
        /// </summary>
        private readonly LineSeries altitudeLineSeries = new LineSeries() { Color = OxyColors.Red };

        /// <summary>
        /// 処理をキャンセルする場合のトークン
        /// </summary>
        private CancellationTokenSource cts;

        /// <summary>
        /// タイマーオブジェクト
        /// </summary>
        private DispatcherTimer dispatcherTimer;

        /// <summary>
        /// 空気抵抗のある自由落下系に対して運動方程式を1ステップ解くクラスのオブジェクト
        /// </summary>
        private FreefallSolveEom ffse;

        /// <summary>
        /// 高度（m or km）
        /// </summary>
        private Double h;

        /// <summary>
        /// 処理が正常終了したかキャンセルで打ち切られたかを示すフラグ
        /// </summary>
        private Boolean isProgressCanceled;

        /// <summary>
        /// 対応するView
        /// </summary>
        private MainWindowViewModel mwvm;
        
        /// <summary>
        /// 現在の動作状態
        /// </summary>
        private OperationState os = OperationState.処理開始待機中;

        /// <summary>
        /// 設定情報保存クラスのオブジェクト
        /// </summary>
        private readonly SaveDataManage.SaveDataManage sdm = new SaveDataManage.SaveDataManage();

        /// <summary>
        /// 外気圏を脱出した際の時間（秒）と速度とその時に第二宇宙速度を超えていたかどうか
        /// </summary>
        private (Double texosphere, Double vexosphere, Boolean issecondescape)? stateofexosphere;

        /// <summary>
        /// 最高到達高度（m）とその際の時間（秒）
        /// </summary>
        private (Double, Double)? stateofhmax;

        /// <summary>
        /// カーマン・ラインを突破した際の時間（秒）と速度
        /// </summary>
        private (Double, Double)? stateofkarmanline;
        
        /// <summary>
        /// 最高速度のときの時間（秒）と高度（m）と速度（m/s）
        /// </summary>
        private (Double, Double, Double)? stateofvmax;

        /// <summary>
        /// 経過時間
        /// </summary>
        private Double t;

        /// <summary>
        /// テキストボックスの自動変更フラグ
        /// </summary>
        private Boolean textBox自動変更;

        /// <summary>
        /// 速度（m/s or km/s）
        /// </summary>
        private Double v;

        /// <summary>
        /// 経過時間－速度グラフのプロット用の LineSeries
        /// </summary>
        private readonly LineSeries velocityLineSeries = new LineSeries();

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸の単位をkmに変更したかどうかのフラグ
        /// </summary>
        private Boolean 経過時間高度縦軸単位km変更フラグ;

        /// <summary>
        /// 経過時間－速度の関係のグラフの縦軸の単位をkm/sに変更したかどうかのフラグ
        /// </summary>
        private Boolean 経過時間速度縦軸単位km_s変更フラグ;
        
        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion 構築

        #region 列挙型

        /// <summary>
        /// 「計算・キャンセル」ボタンのContentの列挙型
        /// </summary>
        private enum ButtonState
        {
            /// <summary>
            /// 計算待ち状態のとき
            /// </summary>
            計算開始,

            /// <summary>
            /// 計算中のとき
            /// </summary>
            キャンセル
        }

        /// <summary>
        /// 現在の動作状態を表す列挙型
        /// </summary>
        private enum OperationState
        {
            /// <summary>
            /// 待機中
            /// </summary>
            処理開始待機中,

            /// <summary>
            /// 処理中
            /// </summary>
            処理中
        }

        #endregion 列挙型

        #region メソッド

        /// <summary>
        /// 処理をキャンセルする
        /// </summary>
        private void Cancel()
        {
            // 「キャンセル」ボタンを無効にする
            this.計算開始キャンセルButton.IsEnabled = false;

            // ウィンドウタイトル変更
            this.Title = Freefallキャンセル待機中;

            // FileIOの処理をキャンセルする
            this.cts.Cancel();
        }

        /// <summary>
        /// 「CalculateResultText」TextBlockのテキストを生成する
        /// </summary>
        /// <returns>「CalculateResultText」TextBlockのテキスト</returns>
        private String CalculateResultText生成()
        {
            String resultstr = String.Empty;
            if (this.stateofkarmanline != null)
            {
                var (tkarmanline, vkarmanline) = this.stateofkarmanline.Value;
                if (vkarmanline >= MainWindow.速度km_s閾値)
                {
                     resultstr += $"{tkarmanline:F1}秒後に{vkarmanline / 1000.0:F1}km/sでカーマン・ライン（高度100km）突破{Environment.NewLine}";
                }
                else
                {
                    resultstr += $"{tkarmanline:F1}秒後に{vkarmanline:F1}m/sでカーマン・ライン（高度100km）突破{Environment.NewLine}";
                }
            }
            else
            {
                resultstr += String.Empty;
            }

            if (this.stateofexosphere != null)
            {
                var (texosphere, vexosphere, dummy) = this.stateofexosphere.Value;
                if (vexosphere >= MainWindow.速度km_s閾値)
                {
                    resultstr += $"{texosphere:F1}秒後に{vexosphere / 1000.0:F1}km/sで外気圏（～高度10000km）脱出{Environment.NewLine}";
                }
                else
                {
                    resultstr += $"{texosphere:F1}秒後に{vexosphere:F1}m/sで外気圏（～高度10000km）脱出{Environment.NewLine}";
                }
            }
            else
            {
                resultstr += String.Empty;
            }

            return resultstr;
        }

        /// <summary>
        /// 計算等の終了処理を行う
        /// それに伴い、UI要素等を変更する
        /// </summary>
        private void DoEnd()
        {
            // タイマー停止
            this.dispatcherTimer.Stop();

            if (this.isProgressCanceled)
            {
                // 「キャンセル」ボタンを有効にする
                this.計算開始キャンセルButton.IsEnabled = true;

                // キャンセルフラグを元に戻す
                this.isProgressCanceled = false;
            }
            else
            {
                // ストップウォッチ停止
                this.sw.Stop();

                // 処理時間の表示
                this.CalculationRequiredTimeText.Text = $"{MainWindow.計算所要時間テキスト}{this.sw.Elapsed.TotalSeconds:F2}秒";

                // 計算結果をUIに表示
                this.Run計算結果UI表示();
            }
            
            // ストップウォッチリセット
            this.sw.Reset();
            
            // PlotViewの操作を可能にする
            this.TvsAltitudePlotView.IsEnabled = true;
            this.TvsVelocityPlotView.IsEnabled = true;

            // ボタンのテキストを「計算開始」にする
            this.計算開始キャンセルButton.Content = MainWindow.ButtonState.計算開始;

            // ウィンドウタイトル変更
            this.Title = MainWindow.Freefall実行待機中;

            // 現在の動作状態を「処理開始待機中」にする
            this.os = OperationState.処理開始待機中;
        }

        /// <summary>
        /// 常微分方程式の数値解法の時間刻みΔt、グラフプロット用の時間間隔、
        /// 常微分方程式の数値解法の許容誤差、常微分方程式の数値解法を取得する
        /// </summary>
        /// <returns>常微分方程式の数値解法の時間刻みΔt、グラフプロット用の時間間隔、常微分方程式の数値解法の許容誤差、常微分方程式の数値解法が格納されたTuple</returns>
        private (Double dt, Double tintervalgraphplot, Double eps, Int32 odesolver) Get各種条件()
        {
            var sd = this.sdm.SaveData;

            Double.TryParse(sd.DeltatOfOdeSolver, out Double dt);
            Double.TryParse(this.mwvm.IntervalOfGraphPlot, out Double tintervalgraphplot);
            Double.TryParse(sd.EpsOfSolveOde, out Double eps);
            
            return (dt, tintervalgraphplot, eps, (Int32)sd.OdeSolver);
        }

        /// <summary>
        /// 経過時間－高度の関係のグラフと、経過時間－速度の関係のグラフをプロットする
        /// </summary>
        /// <param name="uselock">プロットの際にロックをかけるかどうか</param>
        private void GraphPlot(Boolean uselock)
        {
            // TvsAltitudePlotModelにLineSeriesを削除
            this.mwvm.TvsAltitudePlotModel.Series.Remove(this.altitudeLineSeries);
            
            // TvsVelocityPlotModelにLineSeriesを削除
            this.mwvm.TvsVelocityPlotModel.Series.Remove(this.velocityLineSeries);

            // TvsAltitudePlotModelのLineSeriesの設定
            this.mwvm.TvsAltitudePlotModel.Series.Add(this.altitudeLineSeries);

            // TvsVelocityPlotModelのLineSeriesの設定
            this.mwvm.TvsVelocityPlotModel.Series.Add(this.velocityLineSeries);

            if (uselock)
            {
                lock (this.altitudeLineSeries)
                {
                    // 経過時間－高度の関係のグラフをプロット
                    this.mwvm.TvsAltitudePlotModel.InvalidatePlot(true);
                }

                lock (this.velocityLineSeries)
                {
                    // 経過時間－速度の関係のグラフをプロット
                    this.mwvm.TvsVelocityPlotModel.InvalidatePlot(true);
                }
            }
            else
            {
                // 経過時間－高度の関係のグラフをプロット
                this.mwvm.TvsAltitudePlotModel.InvalidatePlot(true);

                // 経過時間－速度の関係のグラフをプロット
                this.mwvm.TvsVelocityPlotModel.InvalidatePlot(true);
            }
        }

        /// <summary>
        /// 「MaximumVelocityText」TextBlockのテキストを生成し、表示する
        /// </summary>
        private void MaximumVelocityText生成()
        {
            if (this.stateofvmax != null)
            {
                var (tvmax, hvmax, vmax) = this.stateofvmax.Value;
                if (Math.Abs(hvmax) >= MainWindow.高度km閾値 && Math.Abs(vmax) >= MainWindow.速度km_s閾値)
                {
                    this.MaximumVelocityText.Text =
                        $"{MainWindow.最高速度テキスト}{tvmax:F1}秒後に高度{hvmax / 1000.0:F1}kmで{vmax / 1000.0:F1}km/s";
                }
                else if (Math.Abs(hvmax) >= MainWindow.高度km閾値)
                {
                    this.MaximumVelocityText.Text =
                        $"{MainWindow.最高速度テキスト}{tvmax:F1}秒後に高度{hvmax / 1000.0:F1}kmで{vmax:F1}m/s";
                }
                else if (Math.Abs(vmax) >= MainWindow.速度km_s閾値)
                {
                    this.MaximumVelocityText.Text =
                        $"{MainWindow.最高速度テキスト}{tvmax:F1}秒後に高度{hvmax:F1}mで{vmax / 1000.0:F1}km/s";
                }
                else
                {
                    this.MaximumVelocityText.Text = $"{MainWindow.最高速度テキスト}{tvmax:F1}秒後に高度{hvmax:F1}mで{vmax:F1}m/s";
                }
            }
        }
        
        /// <summary>
        /// 空気抵抗のある自由落下系における運動方程式を解く等の処理を開始する
        /// </summary>
        /// <param name="dt">常微分方程式の数値解法の時間刻み（秒）</param>
        /// <param name="tintervalgraphplot">グラフプロット用の時間間隔（秒）</param>
        /// <param name="eps">常微分方程式の数値解法の許容誤差</param>
        /// <param name="m">球の質量（kg）</param>
        /// <param name="r">球の半径（m）</param>
        /// <param name="h0">初期高度（m）</param>
        /// <param name="v0">初期速度（m/s）</param>
        /// <param name="odesolver">常微分方程式の数値解法</param>
        private async Task ProgressStart(Double dt, Double tintervalgraphplot, Double eps, Double m, Double r, Double h0, Double v0, Int32 odesolver)
        {
            using (this.cts = new CancellationTokenSource())
            {
                // 非同期で計算処理を実行
                await Task.Run(
                    () =>
                    {
                        try
                        {
                            ffse = new FreefallSolveEom(this.sdm.SaveData, this.altitudeLineSeries,
                                this.velocityLineSeries, dt, tintervalgraphplot, eps, m, r, h0, v0, odesolver);

                            while (UnsafeNativeMethods.IsCalculationFinished() == 0)
                            {
                                // キャンセルされた場合例外をスロー           
                                this.cts.Token.ThrowIfCancellationRequested();

                                (t, h, v, stateofhmax, stateofvmax, stateofkarmanline, stateofexosphere) = ffse.FreefallSolveEomNextStep();

                                if (!this.経過時間高度縦軸単位km変更フラグ && FreefallSolveEom.高度単位km変更フラグ)
                                {
                                    this.経過時間高度Graph縦軸設定();
                                    this.経過時間高度縦軸単位km変更フラグ = true;
                                }

                                if (this.経過時間速度縦軸単位km_s変更フラグ || !FreefallSolveEom.速度単位km_s変更フラグ)
                                {
                                    continue;
                                }

                                this.経過時間速度Graph縦軸設定();
                                this.経過時間速度縦軸単位km_s変更フラグ = true;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            this.isProgressCanceled = true;
                        }
                    },
                    this.cts.Token);
            }

            // 計算処理が終わった後に終了処理を行う
            this.DoEnd();
        }

        /// <summary>
        /// 計算等の処理を行う
        /// </summary>
        private void Run()
        {
            // 現在の動作状態を「処理中」にする
            this.os = OperationState.処理中;

            // ウィンドウタイトル変更
            this.Title = MainWindow.Freefall計算中;

            // ボタンのテキストを「キャンセル」にする
            this.計算開始キャンセルButton.Content = MainWindow.ButtonState.キャンセル.ToString();

            // PlotViewの操作を不可にする
            this.TvsAltitudePlotView.IsEnabled = false;
            this.TvsVelocityPlotView.IsEnabled = false;

            // 計算結果のUIの初期化
            this.Run前計算結果UI初期化();

            // ストップウォッチスタート
            this.sw.Start();

            // 計算に必要な各種条件を取得する
            var (m, r, h0, v0) = this.直径to半径and単位変換();
            var (dt, tintervalgraphplot, eps, odesolver) = this.Get各種条件();

            // タイマースタート
            this.dispatcherTimer.Interval = TimeSpan.FromMilliseconds(MainWindow.TimerInterval);
            this.dispatcherTimer.Start();
            
            // 計算処理開始
            this.ProgressStart(dt, tintervalgraphplot, eps, m, r, h0, v0, odesolver).ContinueWith(_ => {});
        }

        /// <summary>
        /// 計算等の処理前にUIを初期化する
        /// </summary>
        private void Run前計算結果UI初期化()
        {
            this.CalculationRequiredTimeText.Text = MainWindow.計算所要時間テキスト;

            this.MaximumVelocityText.Text = MainWindow.最高速度テキスト;

            this.MaximumReachableAltitudeText.Text = MainWindow.最高到達高度テキスト;

            this.CalculateResultText.Text = String.Empty;
            
            this.経過時間高度Graph縦軸AndフラグReset();
            this.経過時間速度Graph縦軸AndフラグReset();

            // LineSeriesのPointsを空にする
            this.altitudeLineSeries.Points.Clear();
            this.velocityLineSeries.Points.Clear();

            // 何も表示されないグラフをプロット
            this.GraphPlot(false);
        }

        /// <summary>
        /// 空気抵抗のある自由落下系に対して運動方程式を解いた計算結果をUIに表示する
        /// </summary>
        private void Run計算結果UI表示()
        {
            // 計算結果をグラフにプロット
            this.GraphPlot(false);

            // 「MaximumVelocityText」TextBlockのテキストを生成し、表示
            this.MaximumVelocityText生成();

            if (this.stateofexosphere != null && this.stateofexosphere.Value.issecondescape)
            {
                this.MaximumReachableAltitudeText.Text = MainWindow.最高到達高度テキスト + "無限遠";

                // 「CalculateResultText」TextBlockのテキストを生成し、表示
                this.CalculateResultText.Text = this.CalculateResultText生成();
            }
            else
            {
                // 「CalculateResultText」TextBlockのテキストを生成し、表示
                this.CalculateResultText.Text = this.CalculateResultText生成() +
                                                $"{ffse.地面衝突時間And速度.X:F1}秒後に{Math.Abs(ffse.地面衝突時間And速度.Y):F1}m/s（{Math.Abs(ffse.地面衝突時間And速度.Y) * 3.6:F1}km/h）で地面に衝突";
            }
        }

        /// <summary>
        /// 直径を半径に変換し、また単位を基準のものに変換する
        /// </summary>
        /// <returns>球の質量、球の半径、初期高度、初速</returns>
        private (Double m, Double r, Double h0, Double v0) 直径to半径and単位変換()
        {
            Double.TryParse(Regex.Replace(this.SphereOfMassTextBox.Text, @"[^0-9.]", String.Empty), out Double m);
            switch (Regex.Replace(this.SphereOfMassTextBox.Text, @"[^a-z]", String.Empty))
            {
                case "g":
                    m *= 0.001;
                    break;

                case "kg":
                    break;

                case "":
                    break;

                default:
                    Debug.Assert(false, "SphereOfMassTextBoxの単位があり得ない文字になっている！");
                    break;
            }

            Double.TryParse(Regex.Replace(this.SphereOfDiameterTextBox.Text, @"[^0-9.]", String.Empty), out Double d);
            switch (Regex.Replace(this.SphereOfDiameterTextBox.Text, @"[^a-z]", String.Empty))
            {
                case "cm":
                    d *= 0.01;
                    break;

                case "m":
                    break;

                case "mm":
                    d *= 0.001;
                    break;

                case "":
                    break;

                default:
                    Debug.Assert(false, "SphereOfDiameterTextBoxの単位があり得ない文字になっている！");
                    break;
            }

            // 直径を半径に変換
            var r = d * 0.5;

            Double.TryParse(Regex.Replace(this.SphereOfInitialAltitudeTextBox.Text, @"[^0-9.]", String.Empty), out Double h0);
            switch (Regex.Replace(this.SphereOfInitialAltitudeTextBox.Text, @"[^a-z]", String.Empty))
            {
                case "km":
                    h0 *= 1000.0;
                    break;

                case "m":
                    break;

                case "":
                    break;

                default:
                    Debug.Assert(false, "SphereOfInitialAltitudeTextBoxの単位があり得ない文字になっている！");
                    break;
            }

            Double.TryParse(Regex.Replace(this.SphereOfInitialVelocityTextBox.Text, @"[^-0-9.]", String.Empty), out Double v0);
            switch (Regex.Replace(this.SphereOfInitialVelocityTextBox.Text, @"[^a-z/]", String.Empty))
            {
                case "km/h":
                    v0 /= 3.6;
                    break;

                case "km/s":
                    v0 *= 1000.0;
                    break;

                case "m/h":
                    v0 /= 3600.0;
                    break;

                case "m/s":
                    break;

                case "":
                    break;

                default:
                    Debug.Assert(false, "SphereOfInitialVelocityTextBoxの単位があり得ない文字になっている！");
                    break;
            }

            return (m, r, h0, v0);
        }

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸と、それに関連するフラグを元に戻す
        /// </summary>
        private void 経過時間高度Graph縦軸AndフラグReset()
        {
            FreefallSolveEom.高度単位km変更フラグ = false;
            FreefallSolveEom.Is高度単位km = false;

            this.経過時間高度縦軸単位km変更フラグ = false;
            this.経過時間高度Graph縦軸設定();
        }

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸と、それに関連するフラグを元に戻す
        /// </summary>
        private void 経過時間速度Graph縦軸AndフラグReset()
        {
            FreefallSolveEom.速度単位km_s変更フラグ = false;
            FreefallSolveEom.Is速度単位km_s = false;

            this.経過時間速度縦軸単位km_s変更フラグ = false;
            this.経過時間速度Graph縦軸設定();
        }

        /// <summary>
        /// 経過時間－高度の関係のグラフの縦軸を設定する
        /// </summary>
        private void 経過時間高度Graph縦軸設定()
        {
            // 経過時間－高度の関係のグラフの縦軸を削除
            this.mwvm.TvsAltitudePlotModel.Axes.RemoveAt(1);

            // 経過時間－高度の関係のグラフの軸を設定
            this.mwvm.TvsAltitudePlotModel.Axes.Add(FreefallSolveEom.Is高度単位km
                ? new LinearAxis {Minimum = 0.0, Position = AxisPosition.Left, Title = "高度（km）"}
                : new LinearAxis {Minimum = 0.0, Position = AxisPosition.Left, Title = "高度（m）"});
        }

        /// <summary>
        /// 経過時間－速度の関係のグラフの縦軸を設定する
        /// </summary>
        private void 経過時間速度Graph縦軸設定()
        {
            // 経過時間－速度の関係のグラフの縦軸を削除
            this.mwvm.TvsVelocityPlotModel.Axes.RemoveAt(1);

            // 経過時間－速度の関係のグラフの軸を設定
            this.mwvm.TvsVelocityPlotModel.Axes.Add(FreefallSolveEom.Is速度単位km_s
                ? new LinearAxis {Position = AxisPosition.Left, Title = "速度（km/s）"}
                : new LinearAxis {Position = AxisPosition.Left, Title = "速度（m/s）"});
        }

        #endregion メソッド

        #region イベントハンドラ

        /// <summary>
        /// [設定] - [設定]をクリックしたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ApplicationSettinged(object sender, ExecutedRoutedEventArgs e)
        {
            // Instantiate the dialog box
            var ew = new SettingWindow(this.sdm.SaveData)
            {
                Owner = this
            };

            // Open the dialog box modally
            ew.ShowDialog();
        }

        /// <summary>
        /// [ヘルプ] - [バージョン情報]をクリックしたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ApplicationVersioned(object sender, ExecutedRoutedEventArgs e)
        {
            // Instantiate the dialog box
            var ew = new VersionInformationWindow()
            {
                Owner = this
            };

            // Open the dialog box modally
            ew.ShowDialog();
        }

        /// <summary>
        /// タイマーのコールバックメソッド
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.CalculationRequiredTimeText.Text = $"{MainWindow.計算所要時間テキスト}{this.sw.Elapsed.TotalSeconds:F2}秒";

            this.MaximumVelocityText生成();

            if (this.stateofhmax != null)
            {
                var (thmax, hmax) = this.stateofhmax.Value;
                this.MaximumReachableAltitudeText.Text =
                    hmax >= MainWindow.高度km閾値 ? $"{MainWindow.最高到達高度テキスト}{thmax:F1}秒後に{hmax / 1000.0:F1}km" :
                                                    $"{MainWindow.最高到達高度テキスト}{thmax:F1}秒後に{hmax:F1}m";
            }

            var 高度str = "現在高度：";
            if (FreefallSolveEom.高度単位km変更フラグ && Math.Abs(h) < 10.0)
            {
                高度str += $"{h * 1000.0:F1}m{Environment.NewLine}";
            }
            else if (FreefallSolveEom.高度単位km変更フラグ)
            {
                高度str += $"{h:F1}km{Environment.NewLine}";
            }
            else
            {
                高度str += $"{h:F1}m{Environment.NewLine}";
            }

            var 速度str = "現在速度：";
            if (FreefallSolveEom.速度単位km_s変更フラグ && Math.Abs(v) < 1.0)
            {
                速度str += $"{v * 1000.0:F1}m/s（{v * 3600.0:F1}km/h）";
            }
            else if (FreefallSolveEom.速度単位km_s変更フラグ)
            {
                速度str += $"{v:F1}km/s（{v * 3600.0:F1}km/h）";
            }
            else
            {
                速度str += $"{v:F1}m/s（{v * 3.6:F1}km/h）";
            }

            // 経過時間、現在高度、現在速度を表示
            this.CalculateResultText.Text = this.CalculateResultText生成() + $"経過時間：{t:F1}秒{Environment.NewLine}" + 高度str + 速度str;

            // 計算結果をリアルタイムにグラフプロット
            this.GraphPlot(true);
        }

        /// <summary>
        /// 「IntervalOfGraphPlotTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void IntervalOfGraphPlotTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.mwvm.IntervalOfGraphPlotHasError = 
                UtilityFunc.TextBoxOnErrorButton無効(this.計算開始キャンセルButton, "IntervalOfGraphPlot", this.mwvm);
        }

        /// <summary>
        /// 「IntervalOfGraphPlotTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void IntervalOfGraphPlotTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UtilityFunc.TextBoxOnError解除Button有効(this.計算開始キャンセルButton, this.mwvm);
        }

        /// <summary>
        /// [ファイル] - [終了]をクリックしたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void MainWindowClosed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ウィンドウが閉じる場合に呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (this.os == OperationState.処理中)
            {
                this.Cancel();
            }

            var sd = this.sdm.SaveData;
           
            sd.IntervalOfGraphPlot =
                String.IsNullOrEmpty(this.mwvm.IntervalOfGraphPlotHasError)
                    ? this.IntervalOfGraphPlotTextBox.Text
                    : DefaultData.DefaultDataDefinition.DefaultIntervalOfGraphPlot;

            sd.DiameterOfSphere =
                String.IsNullOrEmpty(this.mwvm.DiameterOfSphereHasError)
                    ? this.SphereOfDiameterTextBox.Text
                    : DefaultData.DefaultDataDefinition.DefaultDiameterOfSphere;

            sd.InitialAltitudeOfSphere =
                String.IsNullOrEmpty(this.mwvm.InitialAltitudeOfSphereHasError)
                    ? this.SphereOfInitialAltitudeTextBox.Text
                    : DefaultData.DefaultDataDefinition.DefaultInitialAltitudeOfSphere;

            sd.InitialVelocityOfSphere =
                String.IsNullOrEmpty(this.mwvm.InitialVelocityOfSphereHasError)
                    ? this.SphereOfInitialVelocityTextBox.Text
                    : DefaultData.DefaultDataDefinition.DefaultInitialVelocityOfSphere;

            sd.MassOfSphere =
                String.IsNullOrEmpty(this.mwvm.MassOfSphereHasError)
                    ? this.SphereOfMassTextBox.Text
                    : DefaultData.DefaultDataDefinition.DefaultMassofSphere;

            sd.BallKindComboBoxItem = (DefaultData.BallKind)this.ボールの種類ComboBox.SelectedIndex;

            this.sdm.dataSave();
        }

        /// <summary>
        /// ウィンドウを開くときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            this.sdm.dataLoad();

            var sd = this.sdm.SaveData;

            this.DataContext = this.mwvm = new MainWindowViewModel(sd);
            
            this.SphereOfInitialVelocityTextBox.Text = sd.InitialVelocityOfSphere;

            this.SphereOfInitialAltitudeTextBox.Text = sd.InitialAltitudeOfSphere;

            this.SphereOfDiameterTextBox.Text = sd.DiameterOfSphere;

            this.SphereOfMassTextBox.Text = sd.MassOfSphere;

            this.IntervalOfGraphPlotTextBox.Text = sd.IntervalOfOutputToCsvFile;

            this.ボールの種類ComboBox.SelectedIndex = (Int32)sd.BallKindComboBoxItem;
            
            // タイマーを作成する
            this.dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
        }

        /// <summary>
        /// 「SphereOfDiameterTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfDiameterTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.mwvm.DiameterOfSphereHasError = 
                UtilityFunc.TextBoxOnErrorButton無効(this.計算開始キャンセルButton, "DiameterOfSphere", this.mwvm);
        }

        /// <summary>
        /// 「SphereOfDiameterTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfDiameterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.textBox自動変更)
            {
                this.ボールの種類ComboBox.SelectedIndex = 0;
            }

            UtilityFunc.TextBoxOnError解除Button有効(this.計算開始キャンセルButton, this.mwvm);
        }

        /// <summary>
        /// 「SphereOfInitialAltitudeTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfInitialAltitudeTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.mwvm.InitialAltitudeOfSphereHasError = 
                UtilityFunc.TextBoxOnErrorButton無効(this.計算開始キャンセルButton, "InitialAltitudeOfSphere", this.mwvm);
        }

        /// <summary>
        /// 「SphereOfInitialAltitudeTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfInitialAltitudeTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UtilityFunc.TextBoxOnError解除Button有効(this.計算開始キャンセルButton, this.mwvm);
        }

        /// <summary>
        /// 「SphereOfInitialVelocityTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfInitialVelocityTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.mwvm.InitialVelocityOfSphereHasError = 
                UtilityFunc.TextBoxOnErrorButton無効(this.計算開始キャンセルButton, "InitialVelocityOfSphere", this.mwvm);
        }
        
        /// <summary>
        /// 「SphereOfInitialVelocityTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfInitialVelocityTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var str = this.SphereOfInitialVelocityTextBox.Text;
            if (String.IsNullOrEmpty(str) || str.First() == '0')
            {
                this.から落とすから投げるText.Text = MainWindow.から落とすテキスト;
            }
            else
            {
                this.から落とすから投げるText.Text = MainWindow.から投げるテキスト;
            }

            UtilityFunc.TextBoxOnError解除Button有効(this.計算開始キャンセルButton, this.mwvm);
        }

        /// <summary>
        /// 「SphereOfMassTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfMassTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.mwvm.MassOfSphereHasError = 
                UtilityFunc.TextBoxOnErrorButton無効(this.計算開始キャンセルButton, "MassOfSphere", this.mwvm);
        }

        /// <summary>
        /// 「SphereOfMassTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SphereOfMassTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.textBox自動変更)
            {
                this.ボールの種類ComboBox.SelectedIndex = 0;
            }

            UtilityFunc.TextBoxOnError解除Button有効(this.計算開始キャンセルButton, this.mwvm);
        }

        /// <summary>
        /// 「ボールの種類ComboBox」の選択要素が変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ボールの種類ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String diameter = String.Empty, mass = String.Empty;

            switch ((BallKind)this.ボールの種類ComboBox.SelectedIndex)
            {
                case BallKind.カスタム:
                    return;

                // 以下は https://matome.naver.jp/odai/2142915666380590401 を参考
                case BallKind.ゴルフボール:
                    diameter = "42.67mm";
                    mass = "45.93g";
                    break;

                case BallKind.サッカーボール:
                    diameter = "69cm";
                    mass = "430g";
                    break;

                case BallKind.ソフトボール:
                    diameter = "30.48cm";
                    mass = "190g";
                    break;

                case BallKind.テニスボール:
                    diameter = "6.7cm";
                    mass = "57.7g";
                    break;

                case BallKind.バスケットボール:
                    diameter = "76.5cm";
                    mass = "625g";
                    break;

                case BallKind.パチンコ玉:
                    // http://www.pachinkovillage.com/diary/sanopi-/?p=2314 を参考
                    diameter = "11mm";
                    mass = "5.55g";
                    break;

                case BallKind.ラムネ玉:
                    // http://www.kazariya8740.com/cathand/detail-87623.html と、
                    // http://www.matsuno-b.com/products.html を参考
                    diameter = "17mm";
                    mass = "6.538g";
                    break;

                case BallKind.卓球ボール:
                    diameter = "40.3mm";
                    mass = "2.72g";
                    break;

                case BallKind.硬式野球のボール:
                    diameter = "23.2cm";
                    mass = "145.25g";
                    break;

                default:
                    Debug.Assert(false, "BallKindがあり得ない値になっている！");
                    break;
            }

            this.textBox自動変更 = true;
            this.SphereOfDiameterTextBox.Text = diameter;
            this.SphereOfMassTextBox.Text = mass;
            this.textBox自動変更 = false;
        }

        /// <summary>
        /// 「計算開始・キャンセル」ボタンを押したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void 計算開始キャンセルButton_Click(object sender, RoutedEventArgs e)
        {
            switch (this.os)
            {
                case OperationState.処理中:
                    this.Cancel();
                    break;

                case OperationState.処理開始待機中:
                    this.Run();
                    break;

                default:
                    Debug.Assert(false, "OperationStateがあり得ない値になっている！");
                    break;
            }
        }
        
        #endregion イベントハンドラ
    }
}
