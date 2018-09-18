//-----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;
    using MyLogic;
    using OxyPlot;
    using OxyPlot.Axes;

    /// <summary>
    /// MainWindowに対応するView
    /// </summary>
    internal sealed class MainWindowViewModel : ViewModelBase
    {
        #region フィールド

        /// <summary>
        /// 高度のエラー検出用の正規表現
        /// </summary>
        private static readonly Regex AltitudeErrorRegex = new Regex(@"(^0\.0+k?m$)|(^0\.0+$)|(^0k?m$)|(^0$)");

        /// <summary>
        /// 初速のエラー検出用の正規表現
        /// </summary>
        private static readonly Regex VelocityErrorRegex = new Regex(@"(^0\.0+k?m/[sh]$)|(^0\.0+$)|(^0k?m/[sh]$)|(^\-.*$)|(^0$)");

        /// <summary>
        /// ファイルに保存される設定情報データのオブジェクト
        /// </summary>
        private readonly SaveDataManage.SaveData sd;

        /// <summary>
        /// グラフプロットの際の時間間隔（秒）
        /// </summary>
        private String intervalOfGraphPlot;

        /// <summary>
        /// グラフプロットの際の時間間隔が異常だった場合のエラー文字列
        /// </summary>
        private String intervalOfGraphPlotHasError = String.Empty;

        /// <summary>
        /// 球の直径
        /// </summary>
        private String diameterOfSphere;

        /// <summary>
        /// 球の直径が異常だった場合のエラー文字列
        /// </summary>
        private String sphereOfDiameterHasError = String.Empty;

        /// <summary>
        /// 球の初期高度
        /// </summary>
        private String initialAltitudeOfSphere;

        /// <summary>
        /// 球を落とす高度が異常だった場合のエラー文字列
        /// </summary>
        private String sphereOfInitialAltitudeHasError = String.Empty;
   
        /// <summary>
        /// 球の初速
        /// </summary>
        private String initialVelocityOfSphere;

        /// <summary>
        /// 球を落とす初速が異常だった場合のエラー文字列
        /// </summary>
        private String sphereOfInitialVelocityHasError = String.Empty;

        /// <summary>
        /// 球の質量
        /// </summary>
        private String massOfSphere;

        /// <summary>
        /// 球の質量が異常だった場合のエラー文字列
        /// </summary>
        private String sphereOfMassHasError = String.Empty;

        /// <summary>
        /// 経過時間－高度の関係のグラフ
        /// </summary>
        private PlotModel tvsAltitudePlotModel;

        /// <summary>
        /// 経過時間－速度の関係のグラフ
        /// </summary>
        private PlotModel tvsVelocityPlotModel;
        
        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">ファイルに保存される設定情報データのオブジェクト</param>
        internal MainWindowViewModel(SaveDataManage.SaveData sd)
        {
            this.sd = sd;

            this.IntervalOfGraphPlot = sd.IntervalOfGraphPlot;

            this.DiameterOfSphere = sd.DiameterOfSphere;

            this.InitialAltitudeOfSphere = sd.InitialAltitudeOfSphere;

            this.InitialVelocityOfSphere = sd.InitialVelocityOfSphere;

            this.MassOfSphere = sd.MassOfSphere;

            this.InitializePlotModel();
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// 球の直径
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[1-9][0-9]*\.?[0-9]*$)|(^[1-9][0-9]*\.?[0-9]*[cm]?m$)|(^0\.[0-9]+$)|(^0\.[0-9]+[cm]?m$)", ErrorMessage = "入力された値が正しくありません")]
        public String DiameterOfSphere
        {
            get => this.diameterOfSphere;

            set
            {
                this.SetProperty(ref this.diameterOfSphere, value);
                this.ValidateProperty("DiameterOfSphere", value);
            }
        }

        /// <summary>
        /// 球の直径が異常だった場合のエラー文字列
        /// </summary>
        public String DiameterOfSphereHasError
        {
            get => this.sphereOfDiameterHasError;

            internal set => this.SetProperty(ref this.sphereOfDiameterHasError, value);
        }

        /// <summary>
        /// 球を落とす高度
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[1-9][0-9]*\.?[0-9]*$)|(^[1-9][0-9]*\.?[0-9]*k?m$)|(^0\.[0-9]+$)|(^0\.[0-9]+k?m$)|(^0k?m$)|(^0$)", ErrorMessage = "入力された値が正しくありません")]
        public String InitialAltitudeOfSphere
        {
            get => this.initialAltitudeOfSphere;

            set
            {
                this.SetProperty(ref this.initialAltitudeOfSphere, value);
                this.ValidateProperty("InitialAltitudeOfSphere", value);

                var velstr = this.initialVelocityOfSphere;

                if (MainWindowViewModel.AltitudeErrorRegex.IsMatch(this.InitialAltitudeOfSphere) &&
                    !String.IsNullOrEmpty(velstr) &&
                    MainWindowViewModel.VelocityErrorRegex.IsMatch(velstr))
                {
                    this.AddError("InitialVelocityOfSphere", "高度が0mの場合は、初速は0m/sより大きくなければなりません");
                }
                else
                {
                    this.RemoveError("InitialVelocityOfSphere", "高度が0mの場合は、初速は0m/sより大きくなければなりません");
                }
            }
        }

        /// <summary>
        /// 球を落とす高度が異常だった場合のエラー文字列
        /// </summary>
        public String InitialAltitudeOfSphereHasError
        {
            get => this.sphereOfInitialAltitudeHasError;

            internal set => this.SetProperty(ref this.sphereOfInitialAltitudeHasError, value);
        }
        
        /// <summary>
        /// 球の初速
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^-?[1-9][0-9]*\.?[0-9]*$)|(^-?[1-9][0-9]*\.?[0-9]*k?m/[sh]$)|(^[-]?0\.[0-9]+$)|(^-?0\.[0-9]+[k]?m/[sh]$)|(^-?0k?m/[sh]$)|(^-?0$)", ErrorMessage = "入力された値が正しくありません")]
        public String InitialVelocityOfSphere
        {
            get => this.initialVelocityOfSphere;

            set
            {
                this.SetProperty(ref this.initialVelocityOfSphere, value);
                this.ValidateProperty("InitialVelocityOfSphere", value);

                if (MainWindowViewModel.AltitudeErrorRegex.IsMatch(this.InitialAltitudeOfSphere) &&
                    !String.IsNullOrEmpty(value) &&
                    MainWindowViewModel.VelocityErrorRegex.IsMatch(value))
                {
                    this.AddError("InitialVelocityOfSphere", "高度が0mの場合は、初速は0m/sより大きくなければなりません");
                }
                else
                {
                    this.RemoveError("InitialVelocityOfSphere", "高度が0mの場合は、初速は0m/sより大きくなければなりません");
                }
            }
        }

        /// <summary>
        /// 球の初速が異常だった場合のエラー文字列
        /// </summary>
        public String InitialVelocityOfSphereHasError
        {
            get => this.sphereOfInitialVelocityHasError;

            internal set => this.SetProperty(ref this.sphereOfInitialVelocityHasError, value);
        }

        /// <summary>
        /// グラフプロットの際の時間間隔
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[0-9]+\.?[0-9]*$)|(^[1-9]\.[0-9]+[eE]-[0-9]+$)", ErrorMessage = "入力された値が正しくありません")]
        public String IntervalOfGraphPlot
        {
            get => this.intervalOfGraphPlot;

            set
            {
                this.SetProperty(ref this.intervalOfGraphPlot, value);
                this.ValidateProperty("IntervalOfGraphPlot", value);

                if (Double.TryParse(value, out Double dtgraphplot))
                {
                    Double.TryParse(this.sd.DeltatOfOdeSolver, out Double dtodesolver);
                    if (dtgraphplot < dtodesolver)
                    {
                        this.AddError("IntervalOfGraphPlot", "グラフプロットの際の時間間隔の値は、常微分方程式の数値解法用の時間刻みΔt以上でなければなりません");

                        return;
                    }

                    var dtdiv = dtgraphplot / dtodesolver;
                    if (dtdiv - Math.Floor(dtdiv) > UtilityFunc.ZeroDecision)
                    {
                        this.AddError("IntervalOfGraphPlot", "グラフプロットの際の時間間隔の値は、常微分方程式の数値解法用の時間刻みΔtの整数倍でなければなりません");
                    }
                }
            }
        }

        /// <summary>
        /// グラフプロットの際の時間間隔が異常だった場合のエラー文字列
        /// </summary>
        public String IntervalOfGraphPlotHasError
        {
            get => this.intervalOfGraphPlotHasError;

            internal set
            {
                this.SetProperty(ref this.intervalOfGraphPlotHasError, value);
            }
        }

        /// <summary>
        /// 球の質量
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[1-9][0-9]*\.?[0-9]*$)|(^[1-9][0-9]*\.?[0-9]*k?g$)|(^0\.[0-9]+$)|(^0\.[0-9]+k?g$)", ErrorMessage = "入力された値が正しくありません")]
        public String MassOfSphere
        {
            get => this.massOfSphere;

            set
            {
                this.SetProperty(ref this.massOfSphere, value);
                this.ValidateProperty("MassOfSphere", value);
            }
        }

        /// <summary>
        /// 球の直径が異常だった場合のエラー文字列
        /// </summary>
        public String MassOfSphereHasError
        {
            get => this.sphereOfMassHasError;

            internal set => this.SetProperty(ref this.sphereOfMassHasError, value);
        }
        
        /// <summary>
        /// 経過時間－高度の関係のグラフ
        /// </summary>
        public PlotModel TvsAltitudePlotModel
        {
            get => this.tvsAltitudePlotModel;
            internal set => this.SetProperty(ref this.tvsAltitudePlotModel, value);
        }

        /// <summary>
        /// 経過時間－速度の関係のグラフ
        /// </summary>
        public PlotModel TvsVelocityPlotModel
        {
            get => this.tvsVelocityPlotModel;
            internal set => this.SetProperty(ref this.tvsVelocityPlotModel, value);
        }

        #endregion プロパティ

        #region メソッド

        /// <summary>
        /// PlotModelを初期化する
        /// </summary>
        private void InitializePlotModel()
        {
            this.tvsAltitudePlotModel = new PlotModel()
            {
                Axes =
                {
                    new LinearAxis { Minimum = 0.0, Position = AxisPosition.Bottom, Title = "経過時間（秒）" },
                    new LinearAxis { Minimum = 0.0, Position = AxisPosition.Left, Title = "高度（m）" }
                },
                Title = "経過時間と高度の関係"
            };

            this.tvsVelocityPlotModel = new PlotModel()
            {
                Axes =
                {
                    new LinearAxis { Minimum = 0.0, Position = AxisPosition.Bottom, Title = "経過時間（秒）" },
                    new LinearAxis { Position = AxisPosition.Left, Title = "速度（m/s）" }
                },
                Title = "経過時間と速度の関係"
            };
        }

        #endregion メソッド
    }
}
