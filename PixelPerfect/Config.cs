using System;
using System.Diagnostics;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Dalamud.Interface.ImGuiNotification;


namespace PixelPerfect;

public partial class PixelPerfect
{
    private void DrawConfig()
    {
        if (_firstTime && !_bitch)
        {
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.Begin("歡迎使用 Pixel Perfect！", ref _firstTime);
            ImGui.TextWrapped("嗨，感謝你安裝這款外掛！");
            ImGui.Text("");
            ImGui.TextWrapped("使用設定選單並新增一個塗鴉即可開始使用。");
            if (ImGui.Button("開啟設定"))
            {
                _config = true;
            }
        }

        var deleteNum = -1;
        var moveNum = -1;
        var moveUp = false;

        if (_config)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(750, 650));
            ImGui.SetNextWindowSize(new Vector2(750, 650), ImGuiCond.FirstUseEver);
            ImGui.Begin("Pixel Perfect 設定", ref _config);

            ImGui.BeginTabBar("Config Tabs");

            if (ImGui.BeginTabItem("設定##Doodles"))
            {
                var number2 = 0;
                ImGui.Checkbox("隱藏更新訊息", ref _bitch);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("永不顯示任何訊息。");
                }

                ImGui.Separator();
                foreach (var doodle in _doodleBag)
                {
                    var enabled = doodle.Enabled;
                    var combat = doodle.Combat;
                    var instance = doodle.Instance;
                    var unsheathed = doodle.Unsheathed;

                    var name = doodle.Name;
                    ImGui.Checkbox($"啟用 ##{number2}", ref enabled);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("完全開啟/關閉此塗鴉");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox($"戰鬥中 ##{number2}", ref combat);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("僅在戰鬥中顯示");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox($"副本中 ##{number2}", ref instance);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("僅在副本中顯示（地下城/團隊等）");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox($"拔劍時 ##{number2}", ref unsheathed);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("僅在武器拔出時顯示");
                    }

                    ImGui.SameLine();
                    ImGui.PushItemWidth(150);
                    ImGui.InputText($"名稱##{number2}", ref name, 20);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("為塗鴉命名！");
                    }

                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                    if (number2 > 0)
                    {
                        if (ImGui.Button($"↑##{number2}"))
                        {
                            moveNum = number2;
                            moveUp = true;
                        }

                        ImGui.SameLine();
                    }

                    if (number2 + 1 < _doodleBag.Count)
                    {
                        if (ImGui.Button($"↓##{number2}"))
                        {
                            moveNum = number2;
                        }

                        ImGui.SameLine();
                    }

                    if (ImGui.Button($"刪除##{number2}"))
                    {
                        deleteNum = number2;
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("刪除此塗鴉");
                    }

                    number2++;
                    doodle.Enabled = enabled;
                    doodle.Unsheathed = unsheathed;
                    doodle.Instance = instance;
                    doodle.Combat = combat;
                    doodle.Name = name;
                }

                ImGui.Separator();
                if (ImGui.Button("新增塗鴉"))
                {
                    _doodleBag.Add(new Drawing());
                }

                if (ImGui.Button("顯示編輯器"))
                {
                    _editor = !_editor;
                }

                ImGui.Separator();
                ImGui.TextWrapped("你可以使用下方按鈕匯出並匯入你的塗鴉以便分享。");
                ImGui.TextWrapped(
                    "可以將目前的塗鴉匯出到剪貼簿，並將字串分享給朋友；或使用匯入按鈕，將目前複製的匯出字串匯入到你自己的塗鴉中！");
                if (ImGui.Button("匯出"))
                {
                    var json = JsonConvert.SerializeObject(this._doodleBag);
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                    ImGui.SetClipboardText(base64);
                    this.AddNotification("已複製到剪貼簿", NotificationType.Info);
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("將目前的塗鴉匯出到剪貼簿以便分享！");
                }

                ImGui.SameLine();

                if (ImGui.Button("從剪貼簿匯入"))
                {
                    try
                    {
                        var base64 = ImGui.GetClipboardText();
                        var jsonBytes = Convert.FromBase64String(base64);
                        var json = Encoding.UTF8.GetString(jsonBytes);
                        var bag = JsonConvert.DeserializeObject<List<Drawing>>(json);
                        _doodleBag.AddRange(bag);
                        SaveConfig();
                        this.AddNotification("匯入成功", NotificationType.Success);
                    }
                    catch
                    {
                        this.AddNotification("無法匯入", NotificationType.Error);
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("將目前剪貼簿的內容匯入你的塗鴉！");
                }

                ImGui.EndTabItem();
            }

            var number = 0;
            _selected = -1;
            foreach (var doodle in _doodleBag)
            {
                if (ImGui.BeginTabItem($"{doodle.Name}##{number}"))
                {
                    _selected = number;

                    var type = doodle.Type;
                    var colour = doodle.Colour;
                    var north = doodle.North;
                    var thickness = doodle.Thickness;
                    var segments = doodle.Segments;
                    var vector = doodle.Vector;
                    var filled = doodle.Filled;
                    var x1 = doodle.Vector.X;
                    var z1 = doodle.Vector.Y;
                    var x2 = doodle.Vector.Z;
                    var z2 = doodle.Vector.W;
                    var zed = doodle.Zed;
                    var zedding = doodle.Zedding;
                    var radius = doodle.Radius;
                    var job = doodle.Job;
                    var jobsBool = doodle.JobsBool;
                    var offset = doodle.Offset;
                    var rotateOffset = doodle.RotateOffset;
                    var outline = doodle.Outline;
                    var outlineColour = doodle.OutlineColour;

                    ImGui.PushItemWidth(300);
                    ImGui.Combo($"類型 ##{number}", ref type, _doodleOptions, _doodleOptions.Length);
                    ImGui.ColorEdit4($"顏色 ##{number}", ref colour, ImGuiColorEditFlags.NoInputs);
                    if (ImGui.TreeNode($"職業##{number}"))
                    {
                        var loop = 0;
                        ImGui.Columns(6);
                        foreach (var jobb in doodle.JobsBool)
                        {
                            ImGui.Checkbox($"{_doodleJobs[loop]}", ref jobsBool[loop]);

                            if (loop == 0 | loop == 4 | loop == 8 | loop == 14 | loop == 17)
                            {
                                ImGui.NextColumn();
                            }

                            loop++;
                        }

                        ImGui.Columns(1);
                        ImGui.TreePop();
                    }

                    ImGui.InputFloat($"粗細 ##{number}", ref thickness, 0.1f, 1f);

                    if (type == 0) //ring
                    {
                        ImGui.InputFloat($"半徑##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"段數 ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"偏移##{number}", ref offset);
                        ImGui.Checkbox($"填滿##{number}", ref filled);
                        ImGui.Checkbox($"Z 軸##{number}", ref zedding);
                        if (zedding)
                        {
                            ImGui.InputFloat($"Z 軸數值##{number}", ref zed, 0.01f, 0.1f);
                        }
                        if (offset)
                        {
                            ImGui.SameLine();
                            ImGui.Checkbox($"旋轉##{number}", ref rotateOffset);
                            ImGui.InputFloat($"偏移 X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"偏移 Y##{number}", ref z1, 0.1f, 1f);
                        }
                    }

                    if (type == 1) //line
                    {
                        ImGui.Checkbox($"固定朝北 ##{number}", ref north);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("否則將以玩家為相對基準");
                        }
                        ImGui.Checkbox($"Z 軸##{number}", ref zedding);
                        if (zedding)
                        {
                            ImGui.InputFloat($"Z 軸數值##{number}", ref zed, 0.01f, 0.1f);
                        }

                        ImGui.PushItemWidth(100);
                        ImGui.InputFloat($"X 1##{number}", ref x1, 0.1f, 1f);
                        ImGui.SameLine();
                        ImGui.InputFloat($"Y 1##{number}", ref z1, 0.1f, 1f);
                        ImGui.InputFloat($"X 2##{number}", ref x2, 0.1f, 1f);
                        ImGui.SameLine();
                        ImGui.InputFloat($"Y 2##{number}", ref z2, 0.1f, 1f);
                        ImGui.PopItemWidth();
                    }

                    if (type == 2) //dot
                    {
                        ImGui.InputFloat($"半徑##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"段數 ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"填滿##{number}", ref filled);
                        ImGui.SameLine();
                        ImGui.Checkbox($"偏移##{number}", ref offset);
                        ImGui.SameLine();
                        ImGui.Checkbox($"外框##{number}", ref outline);
                        if (outline)
                        {
                            ImGui.ColorEdit4($"外框顏色 ##{number}", ref outlineColour,
                                ImGuiColorEditFlags.NoInputs);
                        }
                        ImGui.Checkbox($"Z 軸##{number}", ref zedding);
                        if (zedding)
                        {
                            ImGui.InputFloat($"Z 軸數值##{number}", ref zed, 0.01f, 0.1f);
                        }

                        ImGui.Checkbox($"固定朝北 ##{number}", ref north);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("否則將以玩家為相對基準");
                        }

                        if (offset)
                        {
                            ImGui.Checkbox($"以玩家為基準旋轉偏移##{number}", ref rotateOffset);
                            ImGui.InputFloat($"偏移 X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"偏移 Y##{number}", ref z1, 0.1f, 1f);
                        }

                        if (!north)
                        {
                            ImGui.InputFloat($"偏移 X2##{number}", ref x2, 0.1f, 1f);
                            ImGui.InputFloat($"偏移 Y2##{number}", ref z2, 0.1f, 1f);
                        }
                    }

                    if (type == 3) //dashed ring
                    {
                        ImGui.InputFloat($"半徑##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"段數 ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"Z 軸##{number}", ref zedding);
                        if (zedding)
                        {
                            ImGui.InputFloat($"Z 軸數值##{number}", ref zed, 0.01f, 0.1f);
                        }
                        ImGui.Checkbox($"偏移##{number}", ref offset);
                        if (offset)
                        {
                            ImGui.SameLine();
                            ImGui.Checkbox($"旋轉##{number}", ref rotateOffset);
                            ImGui.InputFloat($"偏移 X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"偏移 Y##{number}", ref z1, 0.1f, 1f);
                        }
                    }

                    if (type == 4) //Cone
                    {
                        if (_cs.LocalPlayer?.TargetObject != null) {
                            ImGui.Text($"{_cs.LocalPlayer.TargetObject.Position.X}");
                            ImGui.Text($"{_cs.LocalPlayer.TargetObject.Position.Z}");
                            var atan = Math.Atan2(_cs.LocalPlayer.TargetObject.Position.X - _cs.LocalPlayer.Position.X, _cs.LocalPlayer.TargetObject.Position.Z - _cs.LocalPlayer.Position.Z);
                            var degr = atan * (180 / Math.PI);
                            ImGui.Text($"{atan}");
                            ImGui.Text($"{degr}");
                        }

                        ImGui.Checkbox($"固定朝北 ##{number}", ref north);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("否則將以玩家為相對基準");
                        }
                        ImGui.InputFloat($"半徑##{number}", ref radius, 0.1f, 1f);
                        ImGui.InputInt($"角度 ##{number}", ref segments, 1, 10);
                        ImGui.Checkbox($"偏移##{number}", ref offset);
                        ImGui.Checkbox($"填滿##{number}", ref filled);
                        ImGui.Checkbox($"目標##{number}", ref outline);
                        ImGui.Checkbox($"Z 軸##{number}", ref zedding);
                        if (zedding)
                        {
                            ImGui.InputFloat($"Z 軸數值##{number}", ref zed, 0.01f, 0.1f);
                        }
                        if (offset)
                        {
                            ImGui.SameLine();
                            ImGui.Checkbox($"旋轉##{number}", ref rotateOffset);
                            ImGui.InputFloat($"偏移 X##{number}", ref x1, 0.1f, 1f);
                            ImGui.InputFloat($"偏移 Y##{number}", ref z1, 0.1f, 1f);
                        }
                    }
                    ImGui.PopItemWidth();
                    doodle.Type = type;
                    doodle.Colour = colour;
                    doodle.North = north;
                    if (thickness < 0f)
                    {
                        thickness = 0f;
                    }

                    doodle.Thickness = thickness;
                    if (segments > 1000)
                    {
                        segments = 1000;
                    }

                    if (segments < 4)
                    {
                        segments = 4;
                    }

                    doodle.Segments = segments;
                    doodle.Vector = vector;
                    doodle.Filled = filled;
                    doodle.Radius = radius;
                    doodle.Zed = zed;
                    doodle.Zedding = zedding;
                    doodle.Vector = new Vector4(x1, z1, x2, z2);
                    doodle.Job = job;
                    doodle.JobsBool = jobsBool;
                    doodle.Offset = offset;
                    doodle.RotateOffset = rotateOffset;
                    doodle.Outline = outline;
                    doodle.OutlineColour = outlineColour;

                    if (ImGui.Button($"顯示編輯器##{number}"))
                    {
                        _editor = !_editor;
                    }

                    ImGui.EndTabItem();
                }

                number++;
            }

            ImGui.EndTabBar();

            ImGui.Separator();

            if (ImGui.Button("關閉"))
            {
                SaveConfig();
                _config = false;
            }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

            if (ImGui.Button("請 Haplo 喝杯熱可可"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/haplo",
                    UseShellExecute = true
                });
            }

            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar();
            ImGui.End();

            if (_dirtyHack > 100)
            {
                SaveConfig();
                _dirtyHack = 0;
            }

            _dirtyHack++;
            if (deleteNum != -1)
            {
                _doodleBag.RemoveAt(deleteNum);
            }

            if (moveNum == -1) return;
            var doodleA = _doodleBag[moveNum];
            _doodleBag.RemoveAt(moveNum);
            if (moveUp)
            {
                _doodleBag.Insert(moveNum - 1, doodleA);
            }
            else
            {
                _doodleBag.Insert(moveNum + 1, doodleA);
            }
        }
    }

    public void AddNotification(
        string message,
        NotificationType type = NotificationType.Info,
        uint durationInMs = 3000,
        string title = "PixelPerfect")
    {
        Notification notification = new()
        {
            Title = title,
            Content = message,
            Type = type,
            InitialDuration = TimeSpan.FromMilliseconds(durationInMs)
        };

        _nm.AddNotification(notification);
    }
}