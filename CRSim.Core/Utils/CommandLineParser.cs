using CRSim.Core.Models;
namespace CRSim.Core.Utils
{
    public static class CommandLineParser
    {
        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();

            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];

                if (!arg.StartsWith("-"))
                    continue;

                string key = arg.TrimStart('-', '/').ToLower();

                // 是否为最后一个参数 或 下一个参数也是一个key（说明当前是布尔/无值参数）
                bool isLastArg = (i + 1 >= args.Length);
                bool nextIsFlag = !isLastArg && args[i + 1].StartsWith("-");

                string? value = null;
                if (!isLastArg && !nextIsFlag)
                {
                    value = args[i + 1];
                    i++; // skip value
                }

                switch (key)
                {
                    case "epp":
                        options.ExternalPluginPath = value;
                        break;
                    case "debug":
                        options.Debug = true;
                        break;
                }
            }

            return options;
        }
    }
}
