//-----------------------------------------------------------------------
// <copyright file="DeltatBehaviors.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// TextBox 添付ビヘイビア
    /// </summary>
    internal static class DeltatBehaviors
    {
        #region フィールド

        /// <summary>
        /// True なら入力を制限する
        /// 距離に対する依存プロパティ
        /// </summary>
        public static readonly DependencyProperty IsDeltatProperty =
            DependencyProperty.RegisterAttached(
                "IsDeltat",
                typeof(Boolean),
                typeof(DeltatBehaviors),
                new UIPropertyMetadata(false, IsDeltatChanged));

        #endregion フィールド

        #region メソッド
        
        /// <summary>
        /// 距離に対するプロパティの値を取得する
        /// </summary>
        /// <param name="obj">距離に対する依存プロパティ</param>
        /// <returns>距離に対するプロパティの値</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static Boolean GetIsDeltat(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(DeltatBehaviors.IsDeltatProperty);
        }

        /// <summary>
        /// 距離に対するプロパティを設定する
        /// </summary>
        /// <param name="obj">距離に対する依存プロパティ</param>
        /// <param name="value">距離に対するプロパティの値</param>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static void SetIsDeltat(DependencyObject obj, Boolean value)
        {
            obj.SetValue(DeltatBehaviors.IsDeltatProperty, value);
        }

        /// <summary>
        /// 入力されたΔtが正しいかどうかチェックする
        /// </summary>
        /// <param name="deltat">入力されたΔt</param>
        /// <returns>入力された距離が正しいかどうか</returns>
        private static Boolean CheckDeltatText(String deltat)
        {
            return new Regex(@"(^[0-9]+\.?[0-9]*$)|(^[1-9]\.[0-9]+[eE]-[0-9]+$)").IsMatch(deltat);
        }

        #endregion メソッド

        #region イベントハンドラ

        /// <summary>
        /// テキストボックスに何か文字が入力されたときに呼ばれるイベントハンドラ
        /// </summary>
        /// <param name="sender">テキストボックス</param>
        /// <param name="e">依存プロパティが変更されたときのイベント</param>
        private static void IsDeltatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.KeyDown -= OnKeyDown;
                DataObject.RemovePastingHandler(textBox, TextBoxPastingEventHandler);

                if ((Boolean)e.NewValue)
                {
                    textBox.KeyDown += OnKeyDown;
                    DataObject.AddPastingHandler(textBox, TextBoxPastingEventHandler);
                }
            }
        }

        /// <summary>
        /// テキストボックス内で、何かキーが押された時に呼ばれるイベントハンドラ。
        /// テキストボックスへの入力を、数値、「E」キー、「-」キー、「Delete」キーおよび「.」キーのみに制限する
        /// </summary>
        /// <param name="sender">入力制限を行うテキストボックス</param>
        /// <param name="e">入力されたキー</param>
        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox)
            {
                if ((Key.Back == e.Key) ||
                    (Key.D0 <= e.Key && e.Key <= Key.D9) ||
                    (Key.Decimal == e.Key) ||
                    (Key.Delete == e.Key) ||
                    (Key.E == e.Key) ||
                    (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) ||
                    (Key.OemMinus == e.Key) ||
                    (Key.OemPeriod == e.Key) ||
                    (Key.Subtract == e.Key))
                {
                    e.Handled = false;
                }
                else
                {
                    // ここで止める
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// テキストボックスに、クリップボード経由で貼り付けが行われたとき呼ばれるイベントハンドラ。
        /// テキストボックスへの入力を、数値、「E」、「-」および「.」のみに制限する。
        /// クリップボード経由の貼り付けを監視
        /// </summary>
        /// <param name="sender">入力制限を行うテキストボックス</param>
        /// <param name="e">ペーストされた値</param>
        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox &&
                CheckDeltatText(e.DataObject.GetData(typeof(String)) as String))
            {
                return;
            }

            e.CancelCommand();
            e.Handled = true;
        }

        #endregion イベントハンドラ
    }
}
