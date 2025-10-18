using CRSim.Core.Models;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Reflection;

namespace CRSim.PlatformDiagram
{
    public class Generator
    {
        public static void Generate(Station station, string savePath, float pageWidth)
        { 
            float Padding = 40;
            float HeaderHeight = 16;
            float PlatformTableVerticalMargin = 9;
            float PlatformTableHorizontalMargin = 10;
            float TimelineHeight = 10;
            float TimelineExtLength = 4;
            float PlatformHeight = 55;
            float InfoBarWidth = 50;

            PdfWriter writer = new(savePath);
            PdfDocument pdf = new(writer);
            PdfFont font = PdfFontFactory.CreateFont("C:/Windows/Fonts/simsun.ttc,1", PdfEncodings.IDENTITY_H);

            int platformCount = station.Platforms.Count;
            float pageHeight = (Padding + TimelineHeight + PlatformTableVerticalMargin) * 2 + platformCount * PlatformHeight + HeaderHeight;
            Document doc = new(pdf, new PageSize(pageWidth, pageHeight)); doc.SetMargins(Padding, Padding, Padding, Padding);
            var canvas = new PdfCanvas(pdf.AddNewPage());
            TimeSpan startTime = TimeSpan.FromHours(0);
            TimeSpan endTime = TimeSpan.FromHours(24);

            #region 时间线
            int intervalMin = 10;
            Color lineColor = new DeviceRgb(0xAA, 0xAA, 0x7E);
            canvas.SetStrokeColor(lineColor);

            // 绘制整条横线
            canvas.SetLineWidth(1f);
            var xStart = Padding + InfoBarWidth + PlatformTableHorizontalMargin - TimelineExtLength;
            var xEnd = pageWidth - Padding;
            var yTop = pageHeight - Padding - HeaderHeight - TimelineHeight;
            var yBottom = Padding + TimelineHeight;
            canvas.MoveTo(xStart, yTop).LineTo(xEnd, yTop).Stroke();
            canvas.MoveTo(xStart, yBottom).LineTo(xEnd, yBottom).Stroke();

            // 画线和时间文字
            var xBase = Padding + InfoBarWidth + PlatformTableHorizontalMargin;
            yTop = pageHeight - Padding - HeaderHeight - TimelineHeight - PlatformTableVerticalMargin;
            yBottom = Padding + TimelineHeight + PlatformTableVerticalMargin;
            var PlatformTableLength = pageWidth - Padding * 2 - InfoBarWidth - PlatformTableHorizontalMargin - TimelineExtLength;
            for (TimeSpan t = startTime; t <= endTime; t += TimeSpan.FromMinutes(intervalMin))
            {
                float x = xBase + MapTime(t, startTime, endTime, PlatformTableLength);
                // 设置竖线样式
                if (t.Minutes % 60 == 0)
                {
                    canvas.SetLineWidth(1f);
                    canvas.SetLineDash(0);
                }
                else if (t.Minutes % 30 == 0)
                {
                    canvas.SetLineWidth(0.5f);
                    canvas.SetLineDash(0);
                }
                else // 10分钟
                {
                    canvas.SetLineWidth(0.5f);
                    canvas.SetLineDash(3, 3);
                }
                // 竖线从下横线到上横线
                canvas.MoveTo(x, yBottom).LineTo(x, yTop).Stroke();
                static void DrawLabel(PdfCanvas canvas, PdfFont font, string text, float x, float y, float width, float height, Color color, bool alignBottom)
                {
                    // 获取文字度量信息
                    float fontSize = text != "30" ? 14 : 10;
                    float textWidth = font.GetWidth(text, fontSize);
                    float ascent = font.GetAscent(text, fontSize);
                    float descent = font.GetDescent(text, fontSize);
                    float textHeight = ascent - descent;

                    // 水平居中
                    float textX = x + (width - textWidth) / 2;

                    // 垂直位置
                    float textY;
                    if (alignBottom)
                    {
                        textY = y - descent;
                    }
                    else
                    {
                        textY = y + height - ascent;
                    }
                    // 绘制文字
                    canvas.BeginText();
                    canvas.SetFontAndSize(font, fontSize);
                    canvas.SetFillColor(color);
                    canvas.MoveText(textX, textY);
                    canvas.ShowText(text);
                    canvas.EndText();
                }
                if (t.Minutes % 60 == 0) // 整点显示小时
                {
                    string label = $"{t.Hours:D1}";
                    DrawLabel(canvas, font, label, x - 10, yTop + PlatformTableVerticalMargin + 2, 20, TimelineHeight, lineColor, alignBottom: true);
                    DrawLabel(canvas, font, label, x - 10, yBottom - PlatformTableVerticalMargin - TimelineHeight - 2, 20, TimelineHeight, lineColor, alignBottom: false);
                }
                else if (t.Minutes % 30 == 0) // 半点显示“30”
                {
                    DrawLabel(canvas, font, "30", x - 10, yTop + PlatformTableVerticalMargin + 2, 20, TimelineHeight, lineColor, alignBottom: true);
                    DrawLabel(canvas, font, "30", x - 10, yBottom - PlatformTableVerticalMargin - TimelineHeight - 2, 20, TimelineHeight, lineColor, alignBottom: false);
                }
            }
            
            Dictionary<string, float> platformHeights = station.TrainStops
                .Select(x => x.Platform)
                .Where(p => p != null)
                .Distinct()
                .ToDictionary(p => p!, p => 0f);
            canvas.MoveTo(xBase, yBottom).LineTo(xEnd - TimelineExtLength, yBottom).Stroke();
            for (int i = 0; i < platformCount; i++)
            {
                string name = station.Platforms[i].Name;
                if (double.TryParse(name, out double platformNumber))
                {
                    platformHeights[name] = yBottom + (platformCount - i - 1f) * PlatformHeight;
                }
                canvas.SetLineWidth(1f);
                canvas.MoveTo(xBase, yBottom + (platformCount - i) * PlatformHeight)
                      .LineTo(xEnd - TimelineExtLength, yBottom + (platformCount - i) * PlatformHeight)
                      .Stroke();
            }
            #endregion

            #region 列车
            foreach (var train in station.TrainStops)
            {
                float barHeight = PlatformHeight / 2.5f - 4;       
                TimeSpan fixedDuration = TimeSpan.FromMinutes(2);
                if (train.Platform is null || !platformHeights.TryGetValue(train.Platform, out var yBottom1))
                    continue;
                TimeSpan arrival = (TimeSpan)(train.ArrivalTime ?? train.DepartureTime - fixedDuration);
                TimeSpan departure = (TimeSpan)(train.DepartureTime ?? train.ArrivalTime + fixedDuration);

                var trainColors = new Dictionary<string, string>
                {
                    { "G", "FF00BE" }, // pink
                    { "D", "004C99" }, // deep blue
                    { "C", "009999" }, // cyan
                    { "T", "0000FF" }, // blue
                    { "Z", "804000" }, // brown
                    { "K", "FF0000" }, // red
                    { "Y", "FF0000" }, // red
                    { "L", "008000" }, // dark green
                    { "default", "008000" } // dark green
                };
                string firstChar = train.Number[..1].ToUpper();
                string colorHex = trainColors.TryGetValue(firstChar, out string? value) ? value : trainColors["default"];
                var color = new DeviceRgb(
                    Convert.ToInt32(colorHex[..2], 16),
                    Convert.ToInt32(colorHex.Substring(2, 2), 16),
                    Convert.ToInt32(colorHex.Substring(4, 2), 16)
                );
                canvas.SetFillColor(color);
                canvas.SetStrokeColor(color);

                // 检查是否跨零点
                if (departure < arrival)
                {
                    float xStart1 = Padding + InfoBarWidth + PlatformTableHorizontalMargin + MapTime(arrival, startTime, endTime, PlatformTableLength);
                    float xEnd1 = Padding + InfoBarWidth + PlatformTableHorizontalMargin + MapTime(TimeSpan.FromHours(24), startTime, endTime, PlatformTableLength);
                    canvas.RoundRectangle(xStart1, yBottom1 + 4, xEnd1 - xStart1, barHeight, 3f);
                    var rect = new Rectangle(xStart1, yBottom1 + 4 + barHeight, 150, PlatformHeight - barHeight - 8);
                    var pdfCanvas = new Canvas(canvas, rect);
                    string timeStr = train.ArrivalTime.HasValue && train.DepartureTime.HasValue
                        ? $"{train.ArrivalTime.Value.Minutes} {train.DepartureTime.Value.Minutes}"
                        : train.ArrivalTime?.Minutes.ToString() ?? train.DepartureTime?.Minutes.ToString() ?? "";
                    pdfCanvas.Add(new Paragraph($"{train.Number}\n{train.Origin}->{train.Terminal}\n{timeStr}")
                        .SetFont(font)
                        .SetFontSize(8)
                        .SetMultipliedLeading(1f)
                        .SetMargins(0, 0, 0, 0));
                    canvas.Fill();
                    float xStart2 = Padding + InfoBarWidth + PlatformTableHorizontalMargin + MapTime(TimeSpan.Zero, startTime, endTime, PlatformTableLength);
                    float xEnd2 = Padding + InfoBarWidth + PlatformTableHorizontalMargin + MapTime(departure, startTime, endTime, PlatformTableLength);
                    canvas.RoundRectangle(xStart2, yBottom1 + 4, xEnd2 - xStart2, barHeight, 3f);
                    canvas.Fill();
                }
                else
                {
                    xStart = Padding + InfoBarWidth + PlatformTableHorizontalMargin + MapTime(arrival, startTime, endTime, PlatformTableLength);
                    xEnd = Padding + InfoBarWidth + PlatformTableHorizontalMargin + MapTime(departure, startTime, endTime, PlatformTableLength);
                    canvas.RoundRectangle(xStart, yBottom1 + 4, xEnd - xStart, barHeight, 3f);
                    var rect = new Rectangle(xStart, yBottom1 + 4 + barHeight, 150, PlatformHeight - barHeight - 8);
                    var pdfCanvas = new Canvas(canvas, rect);
                    string timeStr = train.ArrivalTime.HasValue && train.DepartureTime.HasValue
                        ? $"{train.ArrivalTime.Value.Minutes} {train.DepartureTime.Value.Minutes}"
                        : train.ArrivalTime?.Minutes.ToString() ?? train.DepartureTime?.Minutes.ToString() ?? "";
                    pdfCanvas.Add(new Paragraph($"{train.Number + (train.ArrivalTime.HasValue ? "" : "*") + (train.DepartureTime.HasValue ? "" : "#")}\n{train.Origin}->{train.Terminal}\n{timeStr}")
                        .SetFont(font)
                        .SetFontSize(8)
                        .SetMultipliedLeading(1f)
                        .SetMargins(0,0,0,0)); 
                    canvas.Fill();
                }
            }
            #endregion

            #region 基本信息
            Rectangle rect1 = new(Padding, pageHeight - Padding, 500, HeaderHeight);
            var layoutCanvas = new Canvas(canvas, rect1);
            layoutCanvas.Add(new Paragraph($"{station.Name}站台占用示意图")
                .SetFont(font)
                .SetFontSize(18)
                .SetFontColor(ColorConstants.BLUE)
                .SetMargins(0,0,0,0));

            string text = $"使用 CRSim v{Assembly.GetExecutingAssembly().GetName().Version} 绘制";
            float fontSize = 12f;
            var para = new Paragraph(text)
                .SetFont(font)
                .SetFontSize(fontSize)
                .SetFontColor(ColorConstants.BLUE)
                .SetMargins(0, 0, 0, 0);
            rect1 = new(pageWidth - Padding - font.GetWidth(text, fontSize), pageHeight - Padding, 500, HeaderHeight);
            layoutCanvas = new Canvas(canvas, rect1);
            layoutCanvas.Add(para);

            foreach (var kvp in platformHeights)
            {
                rect1 = new(Padding, kvp.Value + PlatformHeight / 2 - 10, InfoBarWidth, 20);
                layoutCanvas = new Canvas(canvas, rect1);
                layoutCanvas.Add(new Paragraph($"{kvp.Key}站台")
                    .SetFont(font)
                    .SetFontSize(14)
                    .SetMargins(0, 0, 0, 0)
                    .SetFontColor(ColorConstants.BLUE)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE));
            }
            #endregion
            doc.Close();
        }
        static float MapTime(TimeSpan t, TimeSpan start, TimeSpan end, float width)
        {
            double totalMinutes = (end - start).TotalMinutes;
            double posMinutes = (t - start).TotalMinutes;
            return (float)(posMinutes / totalMinutes * width);
        }
    }
}
