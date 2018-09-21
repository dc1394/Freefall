//-----------------------------------------------------------------------
// <copyright file="SettingWindowViewModel.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using MyLogic;

    /// <summary>
    /// SettingWindowに対応するView
    /// </summary>
    internal sealed class SettingWindowViewModel : ViewModelBase
    {
        #region フィールド

        /// <summary>
        /// 計算結果出力用のCSVファイル名（フルパスを含む）
        /// </summary>
        private String csvFileNameFullPath;

        /// <summary>
        /// 常微分方程式の数値解法用の時間刻みΔt（秒）
        /// </summary>
        private String deltatOfOdeSolver;

        /// <summary>
        /// 常微分方程式の数値解法用の時間刻みΔtが異常だった場合のエラー文字列
        /// </summary>
        private String deltatOfOdeSolverHasError = String.Empty;

        /// <summary>
        /// 常微分方程式の数値解法の許容誤差
        /// </summary>
        private String epsOfSolveOde;

        /// <summary>
        /// 常微分方程式の数値解法の許容誤差のエラー文字列
        /// </summary>
        private String epsOfSolveOdeHasError;

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）
        /// </summary>
        private String intervalOfOutputToCsvFile;

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）が異常だった場合のエラー文字列
        /// </summary>
        private String intervalOfOutputToCsvFileHasError;

        /// <summary>
        /// 常微分方程式の数値解法
        /// </summary>
        private DefaultData.OdeSolverType odeSolver;

        #endregion フィールド

        #region プロパティ

        /// <summary>
        /// 計算結果出力用のCSVファイル名（フルパスを含む）
        /// </summary>
        public String CsvFileNameFullPath
        {
            get => this.csvFileNameFullPath;

            set => this.SetProperty(ref this.csvFileNameFullPath, value);
        }

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[0-9]+[.]?[0-9]*$)|(^[1-9]\.[0-9]+[eE]-[0-9]+$)", ErrorMessage = "入力された値が正しくありません")]
        public String DeltatOfOdeSolver
        {
            get => this.deltatOfOdeSolver;

            set
            {
                this.SetProperty(ref this.deltatOfOdeSolver, value);
                this.ValidateProperty("DeltatOfOdeSolver", value);
            }
        }

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）のエラー文字列
        /// </summary>
        public String DeltatOfOdeSolverHasError
        {
            get => this.deltatOfOdeSolverHasError;

            internal set => this.SetProperty(ref this.deltatOfOdeSolverHasError, value);
        }

        /// <summary>
        /// 常微分方程式の数値解法の許容誤差
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[0-9]+[.]?[0-9]*$)|(^[1-9]\.[0-9]+[eE]-[0-9]+$)", ErrorMessage = "入力された値が正しくありません")]
        public String EpsOfSolveOde
        {
            get => this.epsOfSolveOde;

            set
            {
                this.SetProperty(ref this.epsOfSolveOde, value);
                this.ValidateProperty("EpsOfSolveOde", value);
            }
        }
        
        /// <summary>
        /// 常微分方程式の数値解法の許容誤差のエラー文字列
        /// </summary>
        public String EpsOfSolveOdeHasError
        {
            get => this.epsOfSolveOdeHasError;

            internal set => this.SetProperty(ref this.epsOfSolveOdeHasError, value);
        }

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）
        /// </summary>
        [Required(ErrorMessage = "必須入力項目です")]
        [RegularExpression(@"(^[0-9]+[.]?[0-9]*$)|(^[1-9]\.[0-9]+[eE]-[0-9]+$)", ErrorMessage = "入力された値が正しくありません")]
        public String IntervalOfOutputToCsvFile
        {
            get => this.intervalOfOutputToCsvFile;

            set
            {
                this.SetProperty(ref this.intervalOfOutputToCsvFile, value);
                this.ValidateProperty("IntervalOfOutputToCsvFile", value);

                if (Double.TryParse(value, out Double dtoutputcsv))
                {
                    Double.TryParse(this.deltatOfOdeSolver, out Double dtodesolver);
                    if (dtoutputcsv < dtodesolver)
                    {
                        this.AddError("IntervalOfOutputToCsvFile", "CSVファイルに出力する際の時間間隔の値は、常微分方程式の数値解法用の時間刻みΔt以上でなければなりません");

                        return;
                    }

                    var dtdiv = dtoutputcsv / dtodesolver;
                    if (Math.Abs(dtdiv - Math.Round(dtdiv)) > UtilityFunc.ZeroDecision && Math.Abs(dtdiv - Math.Ceiling(dtdiv)) > UtilityFunc.ZeroDecision)
                    {
                        this.AddError("IntervalOfOutputToCsvFile", "CSVファイルに出力する際の時間間隔の値は、常微分方程式の数値解法用の時間刻みΔtの整数倍でなければなりません");
                    }
                }
            }
        }

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）のエラー文字列
        /// </summary>
        public String IntervalOfOutputToCsvFileHasError
        {
            get => this.intervalOfOutputToCsvFileHasError;

            internal set => this.SetProperty(ref this.intervalOfOutputToCsvFileHasError, value);
        }

        /// <summary>
        /// 常微分方程式の数値解法
        /// </summary>
        public DefaultData.OdeSolverType OdeSolver
        {
            get => this.odeSolver;

            set => this.SetProperty(ref this.odeSolver, value);
        }

        #endregion プロパティ
    }
}
