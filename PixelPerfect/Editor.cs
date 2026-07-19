using System;
using ImGuiNET;
using System.Numerics;

namespace PixelPerfect
{
    public partial class PixelPerfect
    {
        private void DrawEditor()
        {
            if (_editor)
            {
                if (_cs.LocalPlayer == null) return;

                var mX = ImGui.GetMousePos().X;
                var mY = ImGui.GetMousePos().Y;

                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect 編輯器", ref _editor);
                ImGui.PushItemWidth(100);
                ImGui.InputFloat("縮放", ref _editorScale, 0.1f, 1f);
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("每個方格為 1 雅魯 x 1 雅魯"); }
                ImGui.PopItemWidth();
                if (_editorScale <= 0.1f) _editorScale = 0.1f;

                var windowPos = ImGui.GetWindowPos();
                var windowMax = ImGui.GetWindowContentRegionMax();

                var linesY = Math.Ceiling(windowMax.Y / (10 * _editorScale));
                var linesX = Math.Ceiling(windowMax.X / (10 * _editorScale));

                if (ImGui.Button("說明"))
                {
                    _editorHelp = !_editorHelp;
                }

                //Drawing the yalm Grid
                for (int i = 0; i < linesY; i++)
                {
                    ImGui.GetWindowDrawList().AddLine(windowPos with { Y = windowPos.Y + 100 + (10 * i * _editorScale) }, new Vector2(windowPos.X + windowMax.X, windowPos.Y + 100 + (10 * i * _editorScale)), ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));
                }
                for (int i = 0; i < linesX; i++)
                {
                    ImGui.GetWindowDrawList().AddLine(new Vector2(windowPos.X + (10 * i * _editorScale), windowPos.Y + 100), new Vector2(windowPos.X + (10 * i * _editorScale), windowPos.Y + windowMax.Y + 100), ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));
                }
                var dotPosX = windowPos.X + (windowMax.X / 2);
                var dotPosY = windowPos.Y + 50 + (windowMax.Y / 2);
                ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(dotPosX, dotPosY), 10f, ImGui.GetColorU32(new Vector4(0.8f, 0.8f, 0.8f, 0.5f)));

                var loop = 0;
                var skip = false;
                foreach (var doodle in _doodleBag)
                {
                    if (!doodle.Enabled)
                    {
                        loop++;
                        continue;
                    }
                    if(!CheckJob(_cs.LocalPlayer.ClassJob.RowId, doodle.JobsBool))
                    {
                        loop++;
                        continue;
                    }

                    int alpha;
                    if (loop == _selected)
                    {
                        alpha = 4;
                        if (mX > dotPosX + (doodle.Vector.W * 10 * _editorScale) - 20
                            && mX < dotPosX + (doodle.Vector.W * 10 * _editorScale) + 20
                            && mY > dotPosY + (doodle.Vector.X * 10 * _editorScale) - 20
                            && mY < dotPosY + (doodle.Vector.X * 10 * _editorScale) + 20)
                        {
                            if (_grabbed == -1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = 1;
                            }
                            if (_grabbed == 1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = -1;
                            }
                        }
                        if (_grabbed == 1)
                        {
                            doodle.Vector = doodle.Vector with { X = (mY - dotPosY) / (10 * _editorScale), W = (mX - dotPosX) / (10 * _editorScale) };
                        }

                        if (mX > dotPosX + (doodle.Vector.Y * 10 * _editorScale) - 20
                                && mX < dotPosX + (doodle.Vector.Y * 10 * _editorScale) + 20
                                && mY > dotPosY + (doodle.Vector.Z * 10 * _editorScale) - 20
                                && mY < dotPosY + (doodle.Vector.Z * 10 * _editorScale) + 20)
                        {
                            if (_grabbed == -1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = 2;
                            }
                            if (_grabbed == 2 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !skip)
                            {
                                skip = true;
                                _grabbed = -1;
                            }
                        }
                        if (_grabbed == 2)
                        {
                            doodle.Vector = doodle.Vector with { Y = (mX - dotPosX) / (10 * _editorScale), Z = (mY - dotPosY) / (10 * _editorScale) };
                        }
                    }
                    else
                    {
                        alpha = 1;
                    }
                    if (doodle.Type == 0)//Ring
                    {
                        if (doodle.Offset && !doodle.RotateOffset)
                        {
                            dotPosX += (doodle.Vector.X * 10 * _editorScale);
                            dotPosY += (doodle.Vector.Y * 10 * _editorScale);
                        }
                        
                        if (doodle.RotateOffset)
                        {
                            var angle = -_cs.LocalPlayer.Rotation;
                            var cosTheta = MathF.Cos(angle);
                            var sinTheta = MathF.Sin(angle);
                            dotPosX += (cosTheta * (doodle.Vector.X * 10 * _editorScale) - sinTheta * (doodle.Vector.Y * 10 * _editorScale));
                            dotPosY += (sinTheta * (doodle.Vector.X * 10 * _editorScale) + cosTheta * (doodle.Vector.Y * 10 * _editorScale));
                        }
                        DrawRingEditor(dotPosX, dotPosY,
                            doodle.Radius * 10 * _editorScale,
                            doodle.Segments,
                            doodle.Thickness,
                            ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }));
                    }
                    if (doodle.Type == 1)//Line
                    {
                        var x1 = dotPosX + (doodle.Vector.W * 10 * _editorScale);
                        var y1 = dotPosY + (doodle.Vector.X * 10 * _editorScale);

                        var x2 = dotPosX + (doodle.Vector.Y * 10 * _editorScale);
                        var y2 = dotPosY + (doodle.Vector.Z * 10 * _editorScale);

                        if (doodle.North)
                        {

                            ImGui.GetWindowDrawList().AddLine(
                                new Vector2(x1, y1),
                                new Vector2(x2, y2),
                                ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }), doodle.Thickness);
                        }
                        else
                        {
                            var sin = Math.Sin(-_cs.LocalPlayer.Rotation + Math.PI);
                            var cos = Math.Cos(-_cs.LocalPlayer.Rotation + Math.PI);
                            var xr1 = cos * (x1 - dotPosX) - sin * (y1 - dotPosY) + dotPosX;
                            var yr1 = sin * (x1 - dotPosX) + cos * (y1 - dotPosY) + dotPosY;

                            var xr2 = cos * (x2 - dotPosX) - sin * (y2 - dotPosY) + dotPosX;
                            var yr2 = sin * (x2 - dotPosX) + cos * (y2 - dotPosY) + dotPosY;

                            ImGui.GetWindowDrawList().AddLine(
                                new Vector2((float)xr1, (float)yr1),
                                new Vector2((float)xr2, (float)yr2),
                                ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }), doodle.Thickness);
                        }
                    }
                    if (doodle.Type == 2)//Dot
                    {
                        if (doodle.Offset)
                        {
                            dotPosX += (doodle.Vector.X * 10 * _editorScale);
                            dotPosY += (doodle.Vector.Y * 10 * _editorScale);
                        }

                        if (doodle.North)
                        {
                            if (doodle.Outline)
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                     new Vector2(dotPosX, dotPosY),
                                    doodle.Radius + doodle.Thickness * 0.6f,
                                    ImGui.GetColorU32(doodle.OutlineColour with { W = doodle.OutlineColour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                            if (doodle.Filled)
                            {
                                ImGui.GetWindowDrawList().AddCircleFilled(
                                    new Vector2(dotPosX, dotPosY),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments);
                            }
                            else
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                    new Vector2(dotPosX, dotPosY),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                        }
                        else
                        {
                            var x1 = dotPosX + (doodle.Vector.W * 10 * _editorScale);
                            var y1 = dotPosY + (doodle.Vector.X * 10 * _editorScale);

                            var sin = Math.Sin(-_cs.LocalPlayer.Rotation + Math.PI);
                            var cos = Math.Cos(-_cs.LocalPlayer.Rotation + Math.PI);
                            var xr1 = cos * (x1 - dotPosX) - sin * (y1 - dotPosY) + dotPosX;
                            var yr1 = sin * (x1 - dotPosX) + cos * (y1 - dotPosY) + dotPosY;

                            if (doodle.Outline)
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                     new Vector2((float)xr1, (float)yr1),
                                    doodle.Radius + doodle.Thickness * 0.6f,
                                    ImGui.GetColorU32(doodle.OutlineColour with { W = doodle.OutlineColour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                            if (doodle.Filled)
                            {
                                ImGui.GetWindowDrawList().AddCircleFilled(
                                    new Vector2((float)xr1, (float)yr1),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments);
                            }
                            else
                            {
                                ImGui.GetWindowDrawList().AddCircle(
                                    new Vector2((float)xr1, (float)yr1),
                                    doodle.Radius,
                                    ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) }),
                                    doodle.Segments, doodle.Thickness);
                            }
                        }
                    }
                    if (doodle.Type == 3)//Dashed ring
                    {
                        if (doodle.Offset && !doodle.RotateOffset)
                        {
                            dotPosX += (doodle.Vector.X * 10 * _editorScale);
                            dotPosY += (doodle.Vector.Y * 10 * _editorScale);
                        }
                        if (doodle.RotateOffset)
                        {
                            var angle = -_cs.LocalPlayer.Rotation;
                            var cosTheta = MathF.Cos(angle);
                            var sinTheta = MathF.Sin(angle);
                            dotPosX += (cosTheta * (doodle.Vector.X * 10 * _editorScale) - sinTheta * (doodle.Vector.Y * 10 * _editorScale));
                            dotPosY += (sinTheta * (doodle.Vector.X * 10 * _editorScale) + cosTheta * (doodle.Vector.Y * 10 * _editorScale));
                        }
                        float segAng = MathF.Tau / doodle.Segments;
                        uint col = ImGui.GetColorU32(doodle.Colour with { W = doodle.Colour.W * (0.25f * alpha) });
                        for (int i = 0; i < doodle.Segments; i++)
                        {
                            Vector2 pos1 = new Vector2(
                                dotPosX + doodle.Radius * 10 * _editorScale * MathF.Sin(segAng * i),
                                dotPosY + doodle.Radius * 10 * _editorScale * MathF.Cos(segAng * i));
                            Vector2 pos2 = new Vector2(
                                dotPosX + doodle.Radius * 10 * _editorScale * MathF.Sin(segAng * (i + 0.4f)),
                                dotPosY + doodle.Radius * 10 * _editorScale * MathF.Cos(segAng * (i + 0.4f)));
                            ImGui.GetWindowDrawList().AddLine(pos1, pos2, col, doodle.Thickness);
                        }
                    }
                    if (doodle.Type == 4)//Cone
                    {
                    //Not used
                    }
                    loop++;

                }
                ImGui.End();
            }

            if (_editorHelp)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect 編輯器說明", ref _editorHelp);
                ImGui.TextWrapped("在此可即時檢視並編輯你的疊加層繪製。");
                ImGui.TextWrapped("在設定中選擇「塗鴉」分頁，所選的塗鴉將在編輯器中反白顯示。");
                ImGui.TextWrapped("若你有「線條」類型的塗鴉，可以<左鍵點擊>端點來移動它們，再次<左鍵點擊>即可放置。");
                ImGui.TextWrapped("正中央的圓點代表你的角色。");
                ImGui.End();
            }

            if (_update)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect 更新資訊", ref _update);
                ImGui.TextWrapped("已更新至 7.0！");
                ImGui.TextWrapped("- 新增絕槍戰士與繪靈法師");
                ImGui.TextWrapped("- 新增物件繪製用的錐形");
                ImGui.TextWrapped("- 新增 Z 軸編輯");
                if (ImGui.Button("開啟設定"))
                {
                    _config = true;
                }
                ImGui.End();
            }
        }
    }
}
