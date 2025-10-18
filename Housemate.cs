// Housemate.cs
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace Housemate
{
    public class Housemate : IDalamudPlugin
    {
        private const string CommandName = "/housemate";

        private readonly HousemateUI _ui;
        private readonly Configuration _configuration;

        public Housemate(IDalamudPluginInterface pi)
        {
            DalamudApi.Initialize(pi);

            _configuration = DalamudApi.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            _ui = new HousemateUI(_configuration);

            DalamudApi.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage =
                    $"Display the Housemate configuration interface.\n" +
                    $"Toggle the Housemate object overlay with '{CommandName} toggle'\n" +
                    $"Copy all current housing data to clipboard with '{CommandName} copyall'"
            });

            HousingData.Init();
            HousingMemory.Init();

            DalamudApi.PluginInterface.UiBuilder.Draw += DrawUI;
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            _ui.Dispose();
            DalamudApi.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            if (args == "toggle")
            {
                _configuration.Render = !_configuration.Render;
                return;
            }

            // === 新增：一键复制所有数据到系统剪贴板 ===
            if (args == "copyall")
            {
                var text = SnapshotText.BuildAll(maxDistance: null, includeFixtures: true);
                Win32Clipboard.CopyTextToClipboard(text);
                return;
            }

            // 无参数：开/关配置 UI
            _ui.Visible = !_ui.Visible;
        }

        private void DrawConfigUI()
        {
            _ui.Visible = !_ui.Visible;
        }

        private void DrawUI()
        {
            _ui.Draw();
        }
    }
}
