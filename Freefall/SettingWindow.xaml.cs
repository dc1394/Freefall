//-----------------------------------------------------------------------
// <copyright file="SettingWindow.xaml.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using MyLogic;

    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow
    {
        #region フィールド

        /// <summary>
        /// 保存された設定情報のオブジェクト
        /// </summary>
        private readonly SaveDataManage.SaveData sd;

        /// <summary>
        /// SettingWindowに対応するView
        /// </summary>
        private SettingWindowViewModel swvm;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sd">保存された設定情報のオブジェクト</param>
        internal SettingWindow(SaveDataManage.SaveData sd)
        {
            this.InitializeComponent();

            this.sd = sd;
        }

        #endregion 構築

        #region イベントハンドラ
        
        /// <summary>
        /// 「キャンセル」ボタンをクリックしたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 「DeltatOfOdeSolverTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DeltatOfOdeSolverTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.swvm.DeltatOfOdeSolverHasError =
                UtilityFunc.TextBoxOnErrorButton無効(this.OkButton, "DeltatOfOdeSolver", this.swvm);
        }

        /// <summary>
        /// 「デフォルトに戻す」ボタンをクリックしたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DefaultButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.swvm.CsvFileNameFullPath = DefaultData.DefaultDataDefinition.DefaultCsvFileNameFullPath;

            this.swvm.DeltatOfOdeSolver = DefaultData.DefaultDataDefinition.DefaultDeltatOfOdeSolver;
            
            this.swvm.EpsOfSolveOde = DefaultData.DefaultDataDefinition.DefaultEpsOfSolveOde;

            this.swvm.IntervalOfOutputToCsvFile = DefaultData.DefaultDataDefinition.DefaultIntervalOfOutputToCsvFile;

            this.IsOutputToCsvFileCheckBox.IsChecked = DefaultData.DefaultDataDefinition.DefaultIsOutputToCsvFile;

            switch (DefaultData.DefaultDataDefinition.DefaultOdeSolverType)
            {
                case DefaultData.OdeSolverType.ADAMS_BASHFORTH_MOULTON:
                    this.AdamsBashforthMoultonRadioButton.IsChecked = true;
                    break;

                case DefaultData.OdeSolverType.BULIRSCH_STOER:
                    this.BulirschStoerRadioButton.IsChecked = true;
                    break;

                case DefaultData.OdeSolverType.CONTROLLED_RUNGE_KUTTA:
                    this.ControlledRungeKuttaRadioButton.IsChecked = true;
                    break;

                default:
                    Debug.Assert(false, "DefaultData.DefaultDataDefinition.DefaultOdeSolverTypeがあり得ない値になっている！");
                    break;
            }
        }

        /// <summary>
        /// 「DeltatOfOdeSolverTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void DeltatOfOdeSolverTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UtilityFunc.TextBoxOnError解除Button有効(this.OkButton, this.swvm);
        }
        
        /// <summary>
        /// 「IntervalOfOutputToCsvFileTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void IntervalOfOutputToCsvFileTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.swvm.IntervalOfOutputToCsvFileHasError =
                UtilityFunc.TextBoxOnErrorButton無効(this.OkButton, "IntervalOfOutputToCsvFile", this.swvm);
        }

        /// <summary>
        /// 「IntervalOfOutputToCsvFileTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void IntervalOfOutputToCsvFileTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UtilityFunc.TextBoxOnError解除Button有効(this.OkButton, this.swvm);
        }

        /// <summary>
        /// 「IsOutputToCsvFileCheckBox」がチェックされたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void IsOutputToCsvFileCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            this.IntervalOfOutputToCsvFileTextBox.IsEnabled = true;
            this.CsvFileNameFullPathTextBox.IsEnabled = true;
            this.参照Button.IsEnabled = true;
        }

        /// <summary>
        /// 「IsOutputToCsvFileCheckBox」のチェックが外れたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void IsOutputToCsvFileCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.IntervalOfOutputToCsvFileTextBox.IsEnabled = false;
            this.CsvFileNameFullPathTextBox.IsEnabled = false;
            this.参照Button.IsEnabled = false;
        }

        /// <summary>
        /// 「OK」ボタンをクリックしたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void OkButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.sd.CsvFileNameFullPath = this.swvm.CsvFileNameFullPath;

            this.sd.DeltatOfOdeSolver = this.swvm.DeltatOfOdeSolver;

            this.sd.EpsOfSolveOde = this.swvm.EpsOfSolveOde;

            this.sd.IntervalOfOutputToCsvFile = this.swvm.IntervalOfOutputToCsvFile;

            this.sd.IsOutputToCsvFile = this.IsOutputToCsvFileCheckBox.IsChecked ?? false;
            
            this.sd.OdeSolver = this.swvm.OdeSolver;

            this.Close();
        }

        /// <summary>
        /// ウィンドウがロードされるとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SettingWindow_Loaded(object sender, EventArgs e)
        {
            this.DataContext = this.swvm = new SettingWindowViewModel();

            this.swvm.CsvFileNameFullPath = this.sd.CsvFileNameFullPath;

            this.swvm.DeltatOfOdeSolver = this.sd.DeltatOfOdeSolver;

            this.swvm.IntervalOfOutputToCsvFile = this.sd.IntervalOfOutputToCsvFile;

            this.swvm.OdeSolver = this.sd.OdeSolver;

            this.swvm.EpsOfSolveOde = this.sd.EpsOfSolveOde;

            this.IsOutputToCsvFileCheckBox.IsChecked = this.sd.IsOutputToCsvFile;

            this.IsOutputToCsvFileCheckBox.RaiseEvent(this.sd.IsOutputToCsvFile
                ? new RoutedEventArgs(ToggleButton.CheckedEvent)
                : new RoutedEventArgs(ToggleButton.UncheckedEvent));
        }

        /// <summary>
        /// 「SolveOdeEpsTextBox」がエラーを検知したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SolveOdeEpsTextBox_OnError(object sender, ValidationErrorEventArgs e)
        {
            this.swvm.SolveOdeEpsHasError =
                UtilityFunc.TextBoxOnErrorButton無効(this.OkButton, "EpsOfSolveOde", this.swvm);
        }

        /// <summary>
        /// 「SolveOdeEpsTextBox」のテキストが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void SolveOdeEpsTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UtilityFunc.TextBoxOnError解除Button有効(this.OkButton, this.swvm);
        }

        /// <summary>
        /// ラジオボタンが変更されたとき呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            switch (this.swvm.OdeSolver)
            {
                case DefaultData.OdeSolverType.ADAMS_BASHFORTH_MOULTON:
                    this.SolveOdeEpsTextBox.IsEnabled = false;
                    return;

                case DefaultData.OdeSolverType.BULIRSCH_STOER:
                case DefaultData.OdeSolverType.CONTROLLED_RUNGE_KUTTA:
                    this.SolveOdeEpsTextBox.IsEnabled = true;
                    return;

                default:
                    Debug.Assert(false, "DefaultData.OdeSolverTypeがあり得ない値になっている！");
                    return;
            }
        }

        /// <summary>
        /// 「参照」ボタンを押したときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void 参照Button_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog()
            {
                // はじめのファイル名を指定する
                // はじめに「ファイル名」で表示される文字列を指定する
                FileName = Path.GetFileName(this.sd.CsvFileNameFullPath) ?? "result.csv",

                // [ファイルの種類]に表示される選択肢を指定する
                Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.*)|*.*",

                // はじめに表示されるフォルダを指定する
                // 指定しない（空の文字列）の時は、現在のディレクトリが表示される
                InitialDirectory = Path.GetDirectoryName(this.sd.CsvFileNameFullPath) ?? String.Empty,

                // タイトルを設定する
                Title = "CSVファイルのファイル名を指定："
            };

            // ダイアログを表示する
            // ReSharper disable once PossibleInvalidOperationException
            if (sfd.ShowDialog().Value)
            {
                // フルパスを含むファイル名を保存
                this.swvm.CsvFileNameFullPath = sfd.FileName;
            }
        }

        #endregion イベントハンドラ
    }
}
