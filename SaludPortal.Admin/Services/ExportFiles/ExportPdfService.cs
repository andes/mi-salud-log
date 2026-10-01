using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services.ExportFiles;

public class ExportPdfService : IExportFiles
{
    public MemoryStream GenerateLogs(IEnumerable<LogEntry> items)
    {
        // Widths in points. 0 = fill remaining width.
        // Landscape A4 usable ≈ 762 pts (842 - 2×40 margin).
        var headers = new[] { "Timestamp", "IP", "PacienteId", "Nivel", "Categoría", "Mensaje" };
        var colWidths = new[] { 100.0, 80.0, 120.0, 60.0, 180.0, 0.0 };

        var rows = items.Select(l => new[]
        {
            l.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
            l.ClientIp ?? "-",
            l.PatientId ?? "-",
            l.Level ?? "",
            l.Category ?? "",
            l.Message ?? "",
        }).ToList();

        return BuildPdf($"Logs del sitio — {DateTime.Now:dd/MM/yyyy HH:mm}", headers, colWidths, rows, maxLines: 3);
    }

    public MemoryStream GenerateTelemetry(IEnumerable<TelemetryEvent> items)
    {
        var headers = new[] { "Timestamp", "IP", "PacienteId", "Dispositivo", "Evento", "Ruta", "Target" };
        var colWidths = new[] { 100.0, 80.0, 120.0, 100.0, 80.0, 120.0, 0.0 };

        var rows = items.Select(t => new[]
        {
            t.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
            t.ClientIp ?? "-",
            t.PatientId ?? "-",
            FormatDevice(t.Browser, t.OsName, t.DeviceType),
            t.EventType ?? "",
            Clip(t.Route, 32),
            Clip(t.Target, 36),
        }).ToList();

        return BuildPdf($"Telemetría de uso — {DateTime.Now:dd/MM/yyyy HH:mm}", headers, colWidths, rows);
    }

    // ── PDF core ─────────────────────────────────────────────────────────────

    private static MemoryStream BuildPdf(string title, string[] headers, double[] colWidths, List<string[]> rows, int maxLines = 3)
    {
        const double marginH = 40;
        const double marginTop = 50;
        const double marginBottom = 28;
        const double rowH = 14;
        const double headerH = 18;

        var headerFont = new XFont("Courier New", 8, XFontStyleEx.Bold);
        var bodyFont = new XFont("Courier New", 7.5, XFontStyleEx.Regular);
        var titleFont = new XFont("Courier New", 11, XFontStyleEx.Bold);
        var footerFont = new XFont("Courier New", 7, XFontStyleEx.Regular);

        var doc = new PdfDocument();
        doc.Info.Title = title;
        doc.Info.Creator = "SaludPortal Admin";

        PdfPage? page = null;
        XGraphics? gfx = null;
        double y = 0;
        double pageW = 0;
        double pageH = 0;
        double usableW = 0;
        double[] resolvedWidths = Array.Empty<double>();

        void AddPage()
        {
            gfx?.Dispose();
            page = doc.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;
            pageW = page.Width.Point;
            pageH = page.Height.Point;
            usableW = pageW - 2 * marginH;

            double fixedSum = colWidths.Where(w => w > 0).Sum();
            resolvedWidths = colWidths.Select(w => w == 0 ? usableW - fixedSum : w).ToArray();

            gfx = XGraphics.FromPdfPage(page);

            // Title line
            gfx.DrawString(title, titleFont, XBrushes.Black, marginH, marginTop - 12);
            gfx.DrawLine(new XPen(XColor.FromArgb(180, 180, 180), 0.5), marginH, marginTop - 4, marginH + usableW, marginTop - 4);

            // Header row
            var hdrBrush = new XSolidBrush(XColor.FromArgb(42, 90, 160));
            double hx = marginH;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(hdrBrush, hx, marginTop, resolvedWidths[i], headerH);
                gfx.DrawString(headers[i], headerFont, XBrushes.White,
                    new XRect(hx + 3, marginTop + 2, resolvedWidths[i] - 6, headerH - 2),
                    XStringFormats.TopLeft);
                hx += resolvedWidths[i];
            }

            y = marginTop + headerH;
        }

        AddPage();

        var altBrush = new XSolidBrush(XColor.FromArgb(237, 242, 252));
        var separatorPen = new XPen(XColor.FromArgb(215, 215, 215), 0.4);

        for (int ri = 0; ri < rows.Count; ri++)
        {
            var row = rows[ri];

            // Wrap every column
            string[][] wrapped = new string[resolvedWidths.Length][];
            for (int ci = 0; ci < resolvedWidths.Length; ci++)
            {
                var text = ci < row.Length ? row[ci] : "";
                wrapped[ci] = WrapText(text, gfx!, bodyFont, resolvedWidths[ci], maxLines);
            }

            double thisRowH = wrapped.Max(w => w.Length) * rowH;

            if (y + thisRowH > pageH - marginBottom)
            {
                AddPage();
                // Re-wrap with new gfx (resolvedWidths unchanged)
                for (int ci = 0; ci < resolvedWidths.Length; ci++)
                {
                    var text = ci < row.Length ? row[ci] : "";
                    wrapped[ci] = WrapText(text, gfx!, bodyFont, resolvedWidths[ci], maxLines);
                }
                thisRowH = wrapped.Max(w => w.Length) * rowH;
            }

            if (ri % 2 == 1)
                gfx!.DrawRectangle(altBrush, marginH, y, usableW, thisRowH);

            gfx!.DrawLine(separatorPen, marginH, y + thisRowH, marginH + usableW, y + thisRowH);

            double rx = marginH;
            for (int ci = 0; ci < resolvedWidths.Length; ci++)
            {
                for (int li = 0; li < wrapped[ci].Length; li++)
                    gfx!.DrawString(wrapped[ci][li], bodyFont, XBrushes.Black,
                        new XRect(rx + 3, y + li * rowH + 2, resolvedWidths[ci] - 6, rowH - 2),
                        XStringFormats.TopLeft);
                rx += resolvedWidths[ci];
            }

            y += thisRowH;
        }

        gfx?.Dispose();

        // Footer: page numbers
        int totalPages = doc.PageCount;
        for (int p = 0; p < totalPages; p++)
        {
            using var fg = XGraphics.FromPdfPage(doc.Pages[p]);
            fg.DrawString(
                $"Pág. {p + 1} de {totalPages}  ·  Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                footerFont, XBrushes.Gray,
                new XRect(marginH, doc.Pages[p].Height.Point - 20, doc.Pages[p].Width.Point - 2 * marginH, 14),
                XStringFormats.TopLeft);
        }

        var ms = new MemoryStream();
        doc.Save(ms);
        // ToArray() works even after PDFsharp closes the underlying stream
        return new MemoryStream(ms.ToArray());
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string[] WrapText(string text, XGraphics gfx, XFont font, double colWidth, int maxLines)
    {
        double maxW = colWidth - 6;
        var lines = new List<string>();
        var current = new System.Text.StringBuilder();

        // Flush current buffer as a new line; returns false if maxLines reached
        bool FlushLine()
        {
            if (current.Length == 0) return true;
            lines.Add(current.ToString());
            current.Clear();
            return lines.Count < maxLines;
        }

        // Append a single token, breaking at character level if it doesn't fit
        void AppendToken(string token, bool prependSpace)
        {
            if (lines.Count >= maxLines) return;

            var candidate = prependSpace && current.Length > 0 ? current + " " + token : current + token;
            if (gfx.MeasureString(candidate, font).Width <= maxW)
            {
                current.Clear();
                current.Append(candidate);
                return;
            }

            // Flush current line before starting the token on a new line
            if (current.Length > 0)
            {
                if (!FlushLine()) return;
            }

            // Break the token character by character
            foreach (char ch in token)
            {
                if (lines.Count >= maxLines) return;
                var attempt = current.ToString() + ch;
                if (gfx.MeasureString(attempt, font).Width <= maxW)
                {
                    current.Append(ch);
                }
                else
                {
                    if (!FlushLine()) return;
                    current.Append(ch);
                }
            }
        }

        var words = text.Split(' ');
        for (int i = 0; i < words.Length && lines.Count < maxLines; i++)
            AppendToken(words[i], prependSpace: i > 0);

        if (current.Length > 0 && lines.Count < maxLines)
            lines.Add(current.ToString());

        return lines.Count == 0 ? [text] : lines.ToArray();
    }

    private static string Clip(string? s, int max)
    {
        if (s is null) return "";
        return s.Length <= max ? s : s[..max] + "…";
    }

    private static string FormatDevice(string? browser, string? osName, string? deviceType)
    {
        var parts = new[] { browser, osName, deviceType }
            .Where(v => !string.IsNullOrWhiteSpace(v));
        var joined = string.Join("/", parts);
        return string.IsNullOrWhiteSpace(joined) ? "-" : joined;
    }
}