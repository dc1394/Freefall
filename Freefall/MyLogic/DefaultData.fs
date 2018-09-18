namespace MyLogic
 
module DefaultData =
    open System

    /// <summary>
    /// ボールの種類を表す列挙型
    /// </summary>
    [<Serializable>]
    type BallKind =
         | カスタム         = 0
         | ゴルフボール     = 1
         | サッカーボール   = 2
         | ソフトボール     = 3
         | テニスボール     = 4
         | バスケットボール = 5
         | パチンコ玉       = 6
         | ラムネ玉         = 7
         | 卓球ボール       = 8
         | 硬式野球のボール = 9

    /// <summary>
    /// 微分方程式の数値解法の種類を表す列挙型
    /// </summary>
    [<Serializable>]
    type OdeSolverType =
         | ADAMS_BASHFORTH_MOULTON = 0
         | BULIRSCH_STOER          = 1
         | CONTROLLED_RUNGE_KUTTA  = 2
 
    /// <summary>
    /// Freefallのデフォルト値を集めた構造体
    /// </summary>
    type DefaultDataDefinition =
        struct
            /// <summary>
            // デフォルトのボールの種類ComboBoxの要素
            /// </summary>
            static member DefaultBallKindComboBoxItem = BallKind.ゴルフボール
            
            /// <summary>
            // デフォルトの計算結果出力用のCSVファイル名（フルパスを含む）
            /// </summary>
            static member DefaultCsvFileNameFullPath = System.Environment.CurrentDirectory + "\\result.csv"
            
            /// <summary>
            /// 常微分方程式の数値解法の時間刻みΔt（秒）
            /// </summary>
            static member DefaultDeltatOfOdeSolver = "1.0E-4"

            /// <summary>
            /// デフォルトのグラフプロットの際の時間間隔（秒）
            /// </summary>
            static member DefaultIntervalOfGraphPlot = "0.1"
                        
            /// <summary>
            /// デフォルトのCSVファイル出力の際の時間間隔（秒）
            /// </summary>
            static member DefaultIntervalOfOutputToCsvFile = "0.1"
            
            /// <summary>
            // デフォルトで計算結果をCSVファイルに保存する
            /// </summary>
            static member DefaultIsOutputToCsvFile = false
            
            /// <summary>
            /// デフォルトの常微分方程式の数値解法
            /// </summary>
            static member DefaultOdeSolverType = OdeSolverType.ADAMS_BASHFORTH_MOULTON
            
            /// <summary>
            /// デフォルトの常微分方程式の数値解法の許容誤差
            /// </summary>
            static member DefaultEpsOfSolveOde = "1.0E-7"

            /// <summary>
            // デフォルトの球の直径
            /// </summary>
            static member DefaultDiameterOfSphere = "42.67mm"

            /// <summary>
            /// デフォルトの球の初期高度（m）
            /// </summary>
            static member DefaultInitialAltitudeOfSphere = "20km"

            /// <summary>
            /// デフォルトの球の速度（m）
            /// </summary>
            static member DefaultInitialVelocityOfSphere = "11.2km/s"
            
            /// <summary>
            // デフォルトの球の質量
            /// </summary>
            static member DefaultMassofSphere = "45.93g"
        end