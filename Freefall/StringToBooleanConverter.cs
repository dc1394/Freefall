//-----------------------------------------------------------------------
// <copyright file="StringToBooleanConverter.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Freefall
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// StringをBoolean値に変換するConverterクラス
    /// </summary>
    [ValueConversion(typeof(String), typeof(Boolean))]
    internal sealed class StringToBooleanConverter : IValueConverter
    {
        #region メソッド

        /// <summary>
        /// StringをBoolean値に変換する
        /// </summary>
        /// <param name="value">変換したいString</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>変換後のBoolean値</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return String.IsNullOrEmpty(value as String);
        }

        /// <summary>
        /// Boolean値をStringに変換する（未使用）
        /// </summary>
        /// <param name="value">変換したいBoolean値</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>なし</returns>
        /// <remarks>
        /// 使用されたらNotImplementedExceptionを投げる
        /// </remarks>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion メソッド
    }
}
