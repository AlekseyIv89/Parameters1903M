namespace Parameters1903M.Util
{
    internal static class GlobalVars
    {
        /// <summary>
        /// Свойство, указываещее было ли стартовое окно закрыто, для предотвращения повторного открытия
        /// </summary>
        public static bool StartWindowClosed { get; set; } = false;

        public static bool IsMeasureRunning { get; set; } = false;

        public static bool IsProvWindowOpened { get; set; } = false;

        public static bool IsDebugEnabled { get; set; } = false;

        public static double Rizm => Properties.Settings.Default.Rizm;

        public static double InputDialogValue { get; set; }

        public static bool IsReportOtkVisible { get; set; } = false;

        public static bool IsReportPzVisible { get; set; } = false;

        public static string SavePath { get; set; }

        public static string DeviceNum { get; set; }
    }
}
