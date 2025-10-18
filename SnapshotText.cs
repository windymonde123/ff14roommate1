// SnapshotText.cs
using System;
using System.Text;

namespace Housemate
{
    /// <summary>
    /// 从内存层把当前房屋对象/部件整理成可复制的纯文本。
    /// </summary>
    public static class SnapshotText
    {
        /// <param name="maxDistance">仅导出某距离内对象；null=不过滤</param>
        /// <param name="includeFixtures">是否包含室内/室外公共部件（墙/屋顶/地面…）</param>
        public static string BuildAll(float? maxDistance = null, bool includeFixtures = true)
        {
            var mem  = HousingMemory.Instance;
            var data = HousingData.Instance;
            var sb   = new StringBuilder();

            string zone = mem.IsWorkshop() ? "Workshop" : mem.IsIndoors() ? "Indoors" : "Outdoors";
            uint terr   = mem.GetTerritoryTypeId();
            float? light = mem.IsIndoors() ? mem.GetInteriorLightLevel() : null;

            sb.AppendLine($"[Snapshot @ {DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
            sb.AppendLine($"Zone={zone}  TerritoryTypeId={terr}  InteriorLight={light}");
            sb.AppendLine();

            // 1) 对象（家具/庭院物件）
            if (mem.TryGetUnsortedHousingGameObjectList(out var objs))
            {
                var player = DalamudApi.ClientState.LocalPlayer?.Position;

                sb.AppendLine("Objects:");
                foreach (var go in objs)
                {
                    string name = "";
                    if (data.TryGetFurniture(go.housingRowId, out var f)) name = f.Item.Value.Name.ToString();
                    else if (data.TryGetYardObject(go.housingRowId, out var y)) name = y.Item.Value.Name.ToString();

                    float? dist = null;
                    if (player.HasValue) dist = Utils.DistanceFromPlayer(go, player.Value);

                    if (maxDistance.HasValue && dist.HasValue && dist.Value > maxDistance.Value) continue;

                    sb.AppendLine(
                        $" - RowId={go.housingRowId}  " +
                        $"Pos=({go.X:F2},{go.Y:F2},{go.Z:F2})  " +
                        $"Color={go.color}  " +
                        $"Dist={dist?.ToString("F2") ?? "NA"}  " +
                        $"Name={name}"
                    );
                }
                sb.AppendLine();
            }

            if (!includeFixtures)
                return sb.ToString();

            // 2) 公共部件（Fixtures）
            sb.AppendLine("Fixtures:");
            if (mem.IsIndoors())
            {
                for (int floor = 0; floor < IndoorAreaData.FloorMax; floor++)
                {
                    var list = mem.GetInteriorCommonFixtures(floor);
                    if (list.Length == 0) continue;
                    sb.AppendLine($" Floor={floor}:");
                    foreach (var fx in list)
                    {
                        var item = fx.Item?.Value.Name.ExtractText();
                        var stainId = fx.Stain?.RowId;
                        var stainNm = fx.Stain?.Name.ExtractText();
                        sb.AppendLine($"  - Type={fx.FixtureType}  Key={fx.FixtureKey}  Item={item}  Stain={stainId}:{stainNm}");
                    }
                }
            }
            else
            {
                // 遍历常见 60 块地皮；按需缩小范围
                for (int plotId = 0; plotId < 60; plotId++)
                {
                    var list = mem.GetExteriorCommonFixtures(plotId);
                    if (list.Length == 0) continue;
                    sb.AppendLine($" Plot={plotId + 1}:");
                    foreach (var fx in list)
                    {
                        var item = fx.Item?.Value.Name.ExtractText();
                        var stainId = fx.Stain?.RowId;
                        var stainNm = fx.Stain?.Name.ExtractText();
                        sb.AppendLine($"  - Type={fx.FixtureType}  Key={fx.FixtureKey}  Item={item}  Stain={stainId}:{stainNm}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
