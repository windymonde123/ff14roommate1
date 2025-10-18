// HousemateUI.cs 中的 SettingsTab() —— 完整替换本方法
private void SettingsTab()
{
    if (!ImGui.BeginTabItem("Settings")) return;

    var render = _configuration.Render;
    var renderDistance = _configuration.RenderDistance;
    var sortedObjects = _configuration.SortObjectLists;
    var sortType = _configuration.SortType;

    if (ImGui.Checkbox("Display housing object overlay", ref render))
        _configuration.Render = render;

    if (render && ImGui.SliderFloat("View distance", ref renderDistance, 0f, 100f))
        _configuration.RenderDistance = renderDistance;

    if (ImGui.Checkbox("Sort housing object lists", ref sortedObjects))
        _configuration.SortObjectLists = sortedObjects;

    if (sortedObjects)
    {
        ImGui.Text("Sort objects by:");
        ImGui.SameLine();
        if (ImGui.BeginCombo("##sortCombo", sortType.ToString()))
        {
            if (ImGui.Selectable(SortType.Distance.ToString()))
                _configuration.SortType = SortType.Distance;
            if (ImGui.Selectable(SortType.Name.ToString()))
                _configuration.SortType = SortType.Name;
            ImGui.EndCombo();
        }
    }

    if (ImGui.Button("Save"))
        _configuration.Save();

    ImGui.Separator();

    // === 新增：一键复制按钮 ===
    ImGui.Text("复制：把当前内存中的全部房屋数据整理为文本后，直接放入系统剪贴板。");
    if (ImGui.Button("复制全部数据到剪贴板"))
    {
        var text = SnapshotText.BuildAll(maxDistance: null, includeFixtures: true);
        Win32Clipboard.CopyTextToClipboard(text);
    }

    ImGui.EndTabItem();
}
