using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace UI_Testing
{
    public class YandexStyleResult
    {
        public int DefaultStyleId { get; set; }
        public int HeaderStyleId { get; set; }
        public int LinkStyleId { get; set; }
        public List<object> Bundle { get; set; } = new List<object>();
        public int CenterXfId { get; set; }

        public int WrapXfId { get; set; }
    }
    internal class YandexStyleResolver
    {
        public static YandexStyleResult Resolve(string stylesJson)
        {
            var result = new YandexStyleResult();

            using var doc = JsonDocument.Parse(stylesJson);
            var root = doc.RootElement;

            // 👉 НАХОДИМ styleSheet
            var styleSheet = root;

            var nodes = styleSheet.GetProperty("c_");

            var fontsNode = FindNode(nodes, "s:fonts");
            var fillsNode = FindNode(nodes, "s:fills");
            var bordersNode = FindNode(nodes, "s:borders");
            var xfsNode = FindNode(nodes, "s:cellXfs");

            var fonts = fontsNode.GetProperty("c_");
            var fills = fillsNode.GetProperty("c_");
            var borders = bordersNode.GetProperty("c_");
            var xfs = xfsNode.GetProperty("c_");


            int maxFontId = fonts.GetArrayLength() - 1;
            int maxFillId = fills.GetArrayLength() - 1;
            int maxBorderId = borders.GetArrayLength() - 1;
            int maxXfId = xfs.GetArrayLength() - 1;

            int borderId = FindBorder(borders);
            if (borderId == -1)
            {
                borderId = ++maxBorderId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "borders", "border", borderId },
                    content = new
                    {
                        t_ = "border",
                        c_ = new object[]
                        {
                            CreateSide("left"),
                            CreateSide("right"),
                            CreateSide("top"),
                            CreateSide("bottom")
                        }
                    }
                });
            }


            // =========================
            // 🔹 FONT: обычный Arial 11
            // =========================
            int defaultFontId = FindFont(fonts, "Arial", "11", false, null);
            if (defaultFontId == -1)
            {
                defaultFontId = ++maxFontId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "fonts", "font", defaultFontId },
                    content = new
                    {
                        t_ = "font",
                        name = new { val = "Arial" },
                        sz = new { val = 11 }
                    }
                });
            }

            // =========================
            // 🔹 FONT: header (bold)
            // =========================
            int headerFontId = FindFont(fonts, "Arial", "11", true, null);
            if (headerFontId == -1)
            {
                headerFontId = ++maxFontId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "fonts", "font", headerFontId },
                    content = new
                    {
                        t_ = "font",
                        name = new { val = "Arial" },
                        b = new { val = true },
                        sz = new { val = 11 }
                    }
                });
            }

            // =========================
            // 🔹 FONT: link
            // =========================
            int linkFontId = FindFont(fonts, "Arial", "11", false, "FF2F69C7", true);
            if (linkFontId == -1)
            {
                linkFontId = ++maxFontId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "fonts", "font", linkFontId },
                    content = new
                    {
                        t_ = "font",
                        name = new { val = "Arial" },     // 🔥 добавили
                        sz = new { val = 11 },            // 🔥 добавили
                        color = new { t_ = "color", rgb = "FF2F69C7" },
                        u = new { val = "single" }
                    }
                });
            }

            // =========================
            // 🔹 FILL (зеленый)
            // =========================
            int headerFillId = FindFill(fills, "FFD9EAD3");
            if (headerFillId == -1)
            {
                headerFillId = ++maxFillId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "fills", "fill", headerFillId },
                    content = new
                    {
                        t_ = "fill",
                        patternFill = new
                        {
                            t_ = "patternFill",
                            patternType = "solid",
                            fgColor = new { t_ = "fgColor", rgb = "FFD9EAD3" }
                        }
                    }
                });
            }

            // =========================
            // 🔹 XF: обычный
            // =========================
            result.DefaultStyleId = FindDefaultXf(xfs, defaultFontId, borderId);
            if (result.DefaultStyleId == -1)
            {
                result.DefaultStyleId = ++maxXfId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "cellXfs", "xf", result.DefaultStyleId },
                    content = new
                    {
                        t_ = "xf",
                        fontId = defaultFontId,
                        borderId = borderId,
                        applyFont = true,
                        applyBorder = true
                    }
                });
            }

            // =========================
            // 🔹 XF: header
            // =========================
            result.HeaderStyleId = FindXf(xfs, headerFontId, headerFillId, borderId);
            if (result.HeaderStyleId == -1)
            {
                result.HeaderStyleId = ++maxXfId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "cellXfs", "xf", result.HeaderStyleId },
                    content = new
                    {
                        t_ = "xf",
                        fontId = headerFontId,
                        fillId = headerFillId,
                        borderId = borderId,
                        applyFont = true,
                        applyBorder = true,
                        applyFill = true,
                        applyAlignment = true,
                        alignment = new
                        {
                            t_ = "alignment",
                            horizontal = "center",
                            vertical = "center"
                        }
                    }
                });
            }

            // =========================
            // 🔹 XF: link
            // =========================
            result.LinkStyleId = FindXf(xfs, linkFontId, null, borderId);
            if (result.LinkStyleId == -1)
            {
                result.LinkStyleId = ++maxXfId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "cellXfs", "xf", result.LinkStyleId },
                    content = new
                    {
                        t_ = "xf",
                        fontId = linkFontId,
                        borderId = borderId,
                        applyFont = true,
                        applyBorder = true
                    }
                });
            }

            result.CenterXfId = FindCenterXf(xfs, defaultFontId, borderId);
            if (result.CenterXfId == -1)
            {
                result.CenterXfId = ++maxXfId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "cellXfs", "xf", result.CenterXfId },
                    content = new
                    {
                        t_ = "xf",
                        fontId = defaultFontId,
                        applyAlignment = true,
                        borderId = borderId,
                        applyFont = true,
                        applyBorder = true,
                        alignment = new
                        {
                            t_ = "alignment",
                            horizontal = "center",
                            vertical = "center",
                            wrapText = "1"
                        }
                    }
                });
            }

            result.WrapXfId = FindWrapXf(xfs, defaultFontId, borderId);

            if (result.WrapXfId == -1)
            {
                result.WrapXfId = ++maxXfId;

                result.Bundle.Add(new
                {
                    t = "ie",
                    path = new object[] { "styleSheet", "cellXfs", "xf", result.WrapXfId },
                    content = new
                    {
                        t_ = "xf",
                        fontId = defaultFontId,
                        borderId = borderId,
                        applyFont = true,
                        applyBorder = true,
                        applyAlignment = true,
                        alignment = new
                        {
                            t_ = "alignment",
                            wrapText = "1"
                        }
                    }
                });
            }
            return result;
        }

        static object CreateSide(string side) => new
        {
            t_ = side,
            style = "thin"
        };

        static bool MatchInt(JsonElement el, string prop, int expected)
        {
            if (!el.TryGetProperty(prop, out var val))
                return false;

            if (val.ValueKind == JsonValueKind.Number)
                return val.GetInt32() == expected;

            if (val.ValueKind == JsonValueKind.String &&
                int.TryParse(val.GetString(), out var parsed))
                return parsed == expected;

            return false;
        }
        static int FindXfWithAlignment(JsonElement xfs, int fontId, int? fillId, bool center, int borderId)
        {
            for (int i = 0; i < xfs.GetArrayLength(); i++)
            {
                var xf = xfs[i];

                // font
                if (!MatchInt(xf, "fontId", fontId))
                    continue;

                // fill (если нужен)
                if (fillId != null && !MatchInt(xf, "fillId", fillId.Value))
                    continue;

                // 🔥 ВАЖНО: проверяем borderId
                if (!MatchInt(xf, "borderId", borderId))
                    continue;

                bool hasAlignment = false;

                if (xf.TryGetProperty("c_", out var children))
                {
                    foreach (var c in children.EnumerateArray())
                    {
                        if (c.GetProperty("t_").GetString() == "s:alignment")
                        {
                            hasAlignment = true;

                            var h = c.TryGetProperty("horizontal", out var hv) ? hv.GetString() : null;
                            var v = c.TryGetProperty("vertical", out var vv) ? vv.GetString() : null;

                            if (center)
                            {
                                if (h == "center" && v == "center")
                                    return i;
                            }
                        }
                    }
                }

                // если ищем без центрирования
                if (!center && !hasAlignment)
                    return i;
            }

            return -1;
        }

        static int FindDefaultXf(JsonElement xfs, int fontId, int borderId)
        {
            for (int i = 0; i < xfs.GetArrayLength(); i++)
            {
                var xf = xfs[i];

                if (!MatchInt(xf, "fontId", fontId))
                    continue;

                if (!MatchInt(xf, "borderId", borderId))
                    continue;

                // ❌ НЕ ДОЛЖНО БЫТЬ alignment
                if (xf.TryGetProperty("c_", out var children))
                {
                    bool hasAlignment = children.EnumerateArray()
                        .Any(c => c.GetProperty("t_").GetString() == "s:alignment");

                    if (hasAlignment)
                        continue;
                }

                // 🎨 fill проверка
                if (xf.TryGetProperty("fillId", out var fill))
                {
                    int fillId = GetIntSafe(fill);

                    if (!IsWhiteFill(fillId))
                        continue;
                }

                return i;
            }

            return -1;
        }

        static int FindCenterXf(JsonElement xfs, int fontId, int borderId)
        {
            for (int i = 0; i < xfs.GetArrayLength(); i++)
            {
                var xf = xfs[i];

                if (!MatchInt(xf, "fontId", fontId))
                    continue;

                if (!MatchInt(xf, "borderId", borderId))
                    continue;

                if (xf.TryGetProperty("fillId", out var fill))
                {
                    int fillId = GetIntSafe(fill);

                    if (!IsWhiteFill(fillId))
                        continue;
                }

                bool valid = false;

                if (xf.TryGetProperty("c_", out var children))
                {
                    foreach (var c in children.EnumerateArray())
                    {
                        if (c.GetProperty("t_").GetString() == "s:alignment")
                        {
                            var h = c.TryGetProperty("horizontal", out var hv) ? hv.GetString() : null;
                            var v = c.TryGetProperty("vertical", out var vv) ? vv.GetString() : null;
                            var wrap = c.TryGetProperty("wrapText", out var wv) ? wv.GetString() : null;

                            if (h == "center" && v == "center" && wrap == "1")
                                valid = true;
                        }
                    }
                }

                if (!valid)
                    continue;


                return i;
            }

            return -1;
        }


        static int FindWrapXf(JsonElement xfs, int fontId, int borderId)
        {
            for (int i = 0; i < xfs.GetArrayLength(); i++)
            {
                var xf = xfs[i];

                if (!MatchInt(xf, "fontId", fontId))
                    continue;

                if (!MatchInt(xf, "borderId", borderId))
                    continue;

                if (xf.TryGetProperty("fillId", out var fill))
                {
                    int fillId = GetIntSafe(fill);

                    if (!IsWhiteFill(fillId))
                        continue;
                }

                bool hasWrapOnly = false;
                bool invalidAlignment = false;

                if (xf.TryGetProperty("c_", out var children))
                {
                    foreach (var c in children.EnumerateArray())
                    {
                        if (c.GetProperty("t_").GetString() == "s:alignment")
                        {
                            var wrap = c.TryGetProperty("wrapText", out var wv) ? wv.GetString() : null;
                            var h = c.TryGetProperty("horizontal", out var hv) ? hv.GetString() : null;
                            var v = c.TryGetProperty("vertical", out var vv) ? vv.GetString() : null;

                            // ✅ должен быть wrap
                            if (wrap == "1")
                                hasWrapOnly = true;

                            // ❌ НИКАКОГО center и вообще ничего кроме wrap
                            if (h != null || v != null)
                                invalidAlignment = true;
                        }
                    }
                }

                if (hasWrapOnly && !invalidAlignment)
                    return i;
            }

            return -1;
        }

        static bool IsWhiteFill(int fillId)
        {
            // если у тебя есть fills — лучше проверить по ним
            // но минимально можно так:

            return fillId == 0 || fillId == 1;
        }
        // =========================
        // 🔍 HELPERS
        // =========================

        static JsonElement FindNode(JsonElement nodes, string type)
        {
            foreach (var el in nodes.EnumerateArray())
            {
                if (el.GetProperty("t_").GetString() == type)
                    return el;
            }

            throw new Exception($"Node {type} not found");
        }

        static int FindFont(JsonElement fonts, string name, string size, bool bold, string color, bool underline = false)
        {
            for (int i = 0; i < fonts.GetArrayLength(); i++)
            {
                if (!fonts[i].TryGetProperty("c_", out var items))
                    continue;

                bool ok = true;

                if (name != null)
                    ok &= HasVal(items, "s:name", name);

                if (size != null)
                    ok &= HasVal(items, "s:sz", size);

                if (bold)
                    ok &= HasTag(items, "s:b");
                else
                    ok &= !HasTag(items, "s:b"); // 🔥 ВАЖНО

                if (color != null)
                {
                    ok &= HasColor(items, color);
                }
                else
                {
                    ok &= IsDefaultColor(items); // 🔥 ВАЖНО
                }

                if (underline)
                    ok &= HasTag(items, "s:u");

                if (ok)
                    return i;
            }

            return -1;
        }
        static bool IsDefaultColor(JsonElement items)
        {
            foreach (var el in items.EnumerateArray())
            {
                if (el.GetProperty("t_").GetString() == "s:color")
                {
                    if (el.TryGetProperty("rgb", out var rgb))
                    {
                        var val = rgb.GetString();

                        // допустимые "дефолтные"
                        return val == "FF000000" || val == "FF3D3D3D";
                    }

                    return false; // есть цвет, но не rgb
                }
            }

            // цвета вообще нет → это ок
            return true;
        }
        static int FindFill(JsonElement fills, string color)
        {
            for (int i = 0; i < fills.GetArrayLength(); i++)
            {

                if (!fills[i].TryGetProperty("c_", out var items))
                    continue;

                if (HasColorDeep(items, color))
                    return i;
            }

            return -1;
        }

        static int FindXf(JsonElement xfs, int fontId, int? fillId, int? borderId)
        {
            for (int i = 0; i < xfs.GetArrayLength(); i++)
            {
                var xf = xfs[i];

                if (xf.TryGetProperty("fontId", out var fId) &&
                    GetIntSafe(fId) == fontId)
                {
                    bool fillOk = fillId == null ||
                                  (xf.TryGetProperty("fillId", out var fl) &&
                                   GetIntSafe(fl) == fillId);

                    bool borderOk = borderId == null ||
                                    (xf.TryGetProperty("borderId", out var br) &&
                                     GetIntSafe(br) == borderId);

                    if (fillOk && borderOk)
                        return i;
                }
            }

            return -1;
        }
        static int FindXfWithWrap(JsonElement xfs, int fontId, int? fillId, int borderId)
        {
            for (int i = 0; i < xfs.GetArrayLength(); i++)
            {
                var xf = xfs[i];

                if (!MatchInt(xf, "fontId", fontId))
                    continue;

                if (fillId != null && !MatchInt(xf, "fillId", fillId.Value))
                    continue;

                if (!MatchInt(xf, "borderId", borderId))
                    continue;

                if (xf.TryGetProperty("c_", out var children))
                {
                    foreach (var c in children.EnumerateArray())
                    {
                        if (c.GetProperty("t_").GetString() == "s:alignment")
                        {
                            var wrap = c.TryGetProperty("wrapText", out var wv) ? wv.GetString() : null;

                            // ❗ БЕЗ center — только wrap
                            if (wrap == "1")
                                return i;
                        }
                    }
                }
            }

            return -1;
        }
        static int GetIntSafe(JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.Number => el.GetInt32(),
                JsonValueKind.String => int.TryParse(el.GetString(), out var v) ? v : -1,
                _ => -1
            };
        }
        static bool HasVal(JsonElement arr, string tag, string val)
        {
            foreach (var el in arr.EnumerateArray())
            {
                if (el.GetProperty("t_").GetString() == tag &&
                    el.TryGetProperty("val", out var v) &&
                    v.GetString() == val)
                    return true;
            }
            return false;
        }

        static bool HasTag(JsonElement arr, string tag)
        {
            foreach (var el in arr.EnumerateArray())
                if (el.GetProperty("t_").GetString() == tag)
                    return true;

            return false;
        }

        static bool HasColor(JsonElement arr, string rgb)
        {
            foreach (var el in arr.EnumerateArray())
            {
                if (el.TryGetProperty("rgb", out var c) && c.GetString() == rgb)
                    return true;
            }
            return false;
        }

        static bool HasColorDeep(JsonElement arr, string rgb)
        {
            foreach (var el in arr.EnumerateArray())
            {
                if (el.TryGetProperty("rgb", out var c) && c.GetString() == rgb)
                    return true;

                if (el.TryGetProperty("c_", out var inner))
                    if (HasColorDeep(inner, rgb))
                        return true;
            }

            return false;
        }
        static int FindBorder(JsonElement borders)
        {
            for (int i = 0; i < borders.GetArrayLength(); i++)
            {
                if (!borders[i].TryGetProperty("c_", out var items) || items.ValueKind != JsonValueKind.Array)
                    continue;

                bool left = false, right = false, top = false, bottom = false;

                foreach (var el in items.EnumerateArray())
                {
                    var type = el.GetProperty("t_").GetString();
                    if (!el.TryGetProperty("style", out var style) || style.GetString() != "thin")
                        continue;

                    bool isAutoColor = false;

                    if (el.TryGetProperty("c_", out var children) && children.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var c in children.EnumerateArray())
                        {
                            if (c.GetProperty("t_").GetString() == "s:color" &&
                                c.TryGetProperty("auto", out var autoVal) &&
                                autoVal.GetString() == "1")
                            {
                                isAutoColor = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Если детей нет, тоже считаем автоцветом?
                        isAutoColor = true;
                    }

                    if (!isAutoColor) continue;

                    if (type == "s:left") left = true;
                    if (type == "s:right") right = true;
                    if (type == "s:top") top = true;
                    if (type == "s:bottom") bottom = true;
                }

                if (left && right && top && bottom)
                    return i;
            }

            return -1;
        }
    }
}