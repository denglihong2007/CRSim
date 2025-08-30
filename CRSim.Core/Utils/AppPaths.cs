namespace CRSim.Core.Utils
{
    public static class AppPaths
    {
        public static string AppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CRSim");
        public static string TempPath =>
            Path.Combine(Path.GetTempPath(), "CRSim");

        public static string ConfigFilePath =>
            Path.Combine(AppDataPath, "data.json");

        public static string PluginsRootPath =>
            Path.Combine(AppDataPath, "Plugins");
    }
}
