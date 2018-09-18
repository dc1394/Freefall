namespace MyLogic

module SaveDataManage =
    open System
    open System.IO
    open System.Runtime.Serialization.Formatters.Binary
    open DefaultData
    open MyError

    /// <summary>
    /// 設定情報データを集めたクラス
    /// </summary>
    [<Serializable>]
    [<Sealed>]
    type SaveData() =
        // #region フィールド
        
        /// <summary>
        // ボールの種類ComboBoxの要素
        /// </summary>
        let mutable ballKindComboBoxItem = DefaultDataDefinition.DefaultBallKindComboBoxItem

        /// <summary>
        // 計算結果保存用のCSVファイル名（フルパスを含む）
        /// </summary>
        let mutable csvFileNameFullPath = DefaultDataDefinition.DefaultCsvFileNameFullPath

        /// <summary>
        /// 常微分方程式の数値解法の時間刻みΔt（秒）
        /// </summary>
        let mutable deltatOfOdeSolver = DefaultDataDefinition.DefaultDeltatOfOdeSolver

        /// <summary>
        /// 球の直径
        /// </summary>
        let mutable diameterOfSphere = DefaultDataDefinition.DefaultDiameterOfSphere

        /// <summary>
        /// 常微分方程式の数値解法の許容誤差
        /// </summary>
        let mutable epsOfSolveOde = DefaultDataDefinition.DefaultEpsOfSolveOde;

        /// <summary>
        /// 球の初期高度
        /// </summary>
        let mutable initialAltitudeOfSphere = DefaultDataDefinition.DefaultInitialAltitudeOfSphere

        /// <summary>
        /// 球の初速
        /// </summary>
        let mutable initialVelocityOfSphere = DefaultDataDefinition.DefaultInitialVelocityOfSphere

        /// <summary>
        /// グラフプロットの際の時間間隔（秒）
        /// </summary>
        let mutable intervalOfGraphPlot = DefaultDataDefinition.DefaultIntervalOfGraphPlot

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）
        /// </summary>
        let mutable intervalOfOutputToCsvFile = DefaultDataDefinition.DefaultIntervalOfOutputToCsvFile

        /// <summary>
        // 計算結果をCSVファイルに出力するかどうか
        /// </summary>
        let mutable isOutputToCsvFile = DefaultDataDefinition.DefaultIsOutputToCsvFile

        /// <summary>
        // 球の質量
        /// </summary>
        let mutable massOfSphere = DefaultDataDefinition.DefaultMassofSphere
        
        /// <summary>
        /// 常微分方程式の数値解法
        /// </summary>
        let mutable odeSolver = DefaultDataDefinition.DefaultOdeSolverType;
                
        // #endregion フィールド

        // #region プロパティ 

        /// <summary>
        // ボールの種類ComboBoxの要素
        /// </summary>
        member public this.BallKindComboBoxItem
            with get() = ballKindComboBoxItem
            and set(value) = ballKindComboBoxItem <- value
        
        /// <summary>
        // 計算結果出力用のCSVファイル名（フルパスを含む）
        /// </summary>
        member public this.CsvFileNameFullPath
            with get() = csvFileNameFullPath
            and set(value) = csvFileNameFullPath <- value
        
        /// <summary>
        /// 常微分方程式の数値解法の時間刻みΔt（秒）
        /// </summary>
        member public this.DeltatOfOdeSolver
            with get() = deltatOfOdeSolver
            and set(value) = deltatOfOdeSolver <- value

        /// <summary>
        /// 球の直径
        /// </summary>
        member public this.DiameterOfSphere
            with get() = diameterOfSphere
            and set(value) = diameterOfSphere <- value

        /// <summary>
        /// 常微分方程式の数値解法の許容誤差
        /// </summary>
        member public this.EpsOfSolveOde
            with get() = epsOfSolveOde
            and set(value) = epsOfSolveOde <- value

        /// <summary>
        /// 球の初期高度
        /// </summary>
        member public this.InitialAltitudeOfSphere
            with get() = initialAltitudeOfSphere
            and set(value) = initialAltitudeOfSphere <- value

        /// <summary>
        /// 球の初速
        /// </summary>
        member public this.InitialVelocityOfSphere
            with get() = initialVelocityOfSphere
            and set(value) = initialVelocityOfSphere <- value
        
        /// <summary>
        /// グラフプロットの際の時間間隔（秒）
        /// </summary>
        member public this.IntervalOfGraphPlot
            with get() = intervalOfGraphPlot
            and set(value) = intervalOfGraphPlot <- value

        /// <summary>
        /// 計算結果をCSVファイルに出力する際の時間間隔（秒）
        /// </summary>
        member public this.IntervalOfOutputToCsvFile
            with get() = intervalOfOutputToCsvFile
            and set(value) = intervalOfOutputToCsvFile <- value
        
        /// <summary>
        /// 計算結果をCSVファイルに出力するかどうか
        /// </summary>
        member public this.IsOutputToCsvFile
            with get() = isOutputToCsvFile
            and set(value) = isOutputToCsvFile <- value

        /// <summary>
        // 球の質量
        /// </summary>
        member public this.MassOfSphere
            with get() = massOfSphere
            and set(value) = massOfSphere <- value

        /// <summary>
        /// 常微分方程式の数値解法
        /// </summary>
        member public this.OdeSolver
            with get() = odeSolver
            and set(value) = odeSolver <- value
        
        // #endregion プロパティ

    /// <summary>
    /// 設定情報データをインポート・エクスポートするクラス
    /// </summary>
    [<Sealed>]
    type SaveDataManage() =
        // #region フィールド

        /// <summary>
        /// 設定情報データのオブジェクト
        /// </summary>
        let mutable sd = new SaveData()

        /// <summary>
        /// 設定情報データ保存ファイル名
        /// </summary>
        static member val DATFILENAME = "Freefalldata.dat"
        
        // #endregion フィールド

        // #region プロパティ

        /// <summary>
        /// 設定情報データのオブジェクト
        /// </summary>
        member public this.SaveData
            with get() = sd
            and set(value) = sd <- value

        // #endregion プロパティ
        
        // #region メソッド
        
        /// <summary>
        /// シリアライズされたデータを読み込みし、メモリに保存する
        /// </summary>
        member public this.dataLoad() =
            try
                using (new FileStream(SaveDataManage.DATFILENAME,
                                      FileMode.Open,
                                      FileAccess.Read)) (fun fs ->
                                                         // 逆シリアル化する
                                                         sd <- (new BinaryFormatter()).Deserialize(fs) :?> SaveData)
            with
                | :? FileNotFoundException -> ()
                | :? InvalidOperationException -> MyError.CallErrorMessageBox(
                                                    String.Format("データを保存したdatファイルが壊れています。{0}datファイルを削除してデフォルトデータを読み込みます"))
                                                  File.Delete(SaveDataManage.DATFILENAME);
                | _ -> reraise ()

        /// <summary>
        /// データをシリアライズし、ファイルに保存する
        /// </summary>
        member public this.dataSave() =
            try
                using (new FileStream(SaveDataManage.DATFILENAME,
                                      FileMode.Create,
                                      FileAccess.Write)) (fun fs ->
                                                          //シリアル化して書き込む
                                                          (new BinaryFormatter()).Serialize(fs, sd))
            with
                | e -> let message = String.Format(
                                        "{0}データファイルの書き込みに失敗しました。{0}{1}にログを保存しました。",
                                        Environment.NewLine,
                                        MyError.ErrorLogFileName)
                       CallErrorMessageBox (e.Message + message)
                       WriteLog e

        // #endregion メソッド