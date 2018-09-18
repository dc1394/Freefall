//-----------------------------------------------------------------------
// <copyright file="UtilityFunc.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Freefall
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;

    #region 列挙型

    /// <summary>
    /// ボールの種類の列挙型
    /// </summary>
    public enum BallKind
    {
        /// <summary>
        /// カスタム
        /// </summary>
        カスタム = 0,

        /// <summary>
        /// ゴルフボール
        /// </summary>
        ゴルフボール = 1,

        /// <summary>
        /// サッカーボール
        /// </summary>
        サッカーボール = 2,

        /// <summary>
        /// パチンコ玉
        /// </summary>
        ソフトボール = 3,

        /// <summary>
        /// テニスボール
        /// </summary>
        テニスボール = 4,

        /// <summary>
        /// バスケットボール
        /// </summary>
        バスケットボール = 5,

        /// <summary>
        /// パチンコ玉
        /// </summary>
        パチンコ玉 = 6,

        /// <summary>
        /// ラムネ玉
        /// </summary>
        ラムネ玉 = 7,

        /// <summary>
        /// 卓球のボール
        /// </summary>
        卓球ボール = 8,

        /// <summary>
        /// 硬式野球のボール
        /// </summary>
        硬式野球のボール = 9
    }

    #endregion 列挙型

    /// <summary>
    /// 便利なstatic関数を集めたクラス
    /// </summary>
    internal static class UtilityFunc
    {
        #region フィールド

        /// <summary>
        /// 0判断用の定数
        /// </summary>
        internal static readonly Double ZeroDecision = 1.0E-10;

        #endregion フィールド

        #region メソッド

        /// <summary>
        /// テキストボックスにエラーがあったときにボタンを無効にする
        /// </summary>
        /// <param name="button">無効にする対象のボタン</param>
        /// <param name="errorTarget">エラーターゲット</param>
        /// <param name="vmb">対象のVieModelBase</param>
        /// <returns>エラーメッセージ</returns>
        internal static String TextBoxOnErrorButton無効(Button button, String errorTarget, ViewModelBase vmb)
        {
            button.IsEnabled = false;

            return vmb.GetErrors(errorTarget) is IEnumerable<String> errors ? String.Join(Environment.NewLine, errors.ToArray()) : String.Empty;
        }

        /// <summary>
        /// テキストボックスのエラーが解決されたときにボタンを有効にする
        /// </summary>
        /// <param name="button">有効にする対象のボタン</param>
        /// <param name="vmb">対象のViewModelBase</param>
        internal static void TextBoxOnError解除Button有効(Button button, ViewModelBase vmb)
        {
            if (!vmb.HasErrors)
            {
                button.IsEnabled = true;
            }
        }

        #endregion メソッド
    }
}
