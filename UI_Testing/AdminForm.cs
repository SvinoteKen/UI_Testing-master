using ClosedXML.Excel;
using HtmlAgilityPack;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI_Testing
{
    public partial class AdminForm : MaterialForm
    {
        private readonly Font tabFont = new Font("Roboto", 14f, FontStyle.Regular);
        private readonly Font textFont = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular);
        private string? login;
        private string? password;
        private readonly Dictionary<string, ReportStats> reportStats = new Dictionary<string, ReportStats>();
        private TextBox manualInputTextBox;
        public AdminForm(string? _login, string? _password)
        {
            InitializeComponent();
            toolStripMenuItem2.Enabled = false;
            login = _login;
            password = _password;
            this.StartPosition = FormStartPosition.CenterScreen;
            enterFirst();
        }

        private async void materialButton1_Click(object sender, EventArgs e)
        {
            var jira = new JiraClient(login, password);
            string jql = BuildJql(dateTimePicker1.Value, dateTimePicker2.Value);
            string filePath = await jira.DownloadJiraReportAsync(dateTimePicker1.Value, dateTimePicker2.Value, jql);

            // 🔥 ВСЕГДА приводим к xlsx
            string xlsxPath = ConvertToXlsxWithLayout(filePath);

            // удаляем старый "xls/html"
            if (File.Exists(filePath))
                File.Delete(filePath);

            var issueKeys = await jira.GetIssueKeysAsync(jql);

            await BuildTabsAsync(issueKeys);

            UpdateExcelReport(xlsxPath);

        }
        private (string productName, int tasks, int functional, int regression) ParseManualData(string input)
        {
            string productName = "(не указано)";
            int tasks = 0;
            int functional = 0;
            int regression = 0;

            // Ищем название релиза после "релиза " (до конца строки)
            var productMatch = Regex.Match(input, @"релиза\s+(.+?)(?:\r?\n|$)", RegexOptions.IgnoreCase);
            if (productMatch.Success)
                productName = productMatch.Groups[1].Value.Trim();

            // Ищем "Количество задач: число"
            var tasksMatch = Regex.Match(input, @"Количество задач:\s*(\d+)", RegexOptions.IgnoreCase);
            if (tasksMatch.Success)
                int.TryParse(tasksMatch.Groups[1].Value, out tasks);

            // Ищем "Количество ФЦ тк: число"
            var funcMatch = Regex.Match(input, @"Количество ФЦ тк:\s*(\d+)", RegexOptions.IgnoreCase);
            if (funcMatch.Success)
                int.TryParse(funcMatch.Groups[1].Value, out functional);

            // Ищем "Количество РЦ тк: число"
            var regMatch = Regex.Match(input, @"Количество РЦ тк:\s*(\d+)", RegexOptions.IgnoreCase);
            if (regMatch.Success)
                int.TryParse(regMatch.Groups[1].Value, out regression);

            return (productName, tasks, functional, regression);
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyThemeToForm(this);
            if (menuStrip != null) // если есть MenuStrip
                ThemeManager.ApplyThemeToMenuStrip(menuStrip, ThemeManager.CurrentMode);

            radioWeek.Checked = true;   // активная по умолчанию
            SetCurrentWeek();
        }
        private string BuildJql(DateTime from, DateTime to)
        {
            string f = from.ToString("yyyy-MM-dd");
            string t = to.ToString("yyyy-MM-dd");

            return $@"issuetype in (Task) AND status in (Resolved, Closed)
                AND labels in (приёмка,KO)
                AND Resolved >= {f}
                AND Resolved <= {t}
                AND labels not in (всяприемка, НТ, AT)
                ORDER BY resolved DESC";
        }

        public async Task BuildTabsAsync(List<string> issueKeys)
        {
            var jira = new JiraClient(login, password);

            tabControl1.TabPages.Clear();
            
            
            foreach (var key in issueKeys)
            {
                string comment = await jira.GetLastReportCommentAsync(key);

                if (string.IsNullOrWhiteSpace(comment))
                    continue;

                var stats = ParseReportStats(comment);

                reportStats[key] = stats;
                var page = new TabPage(
                    $"{key} | ТК: {stats.TotalTests} | Ревью: {stats.Review}")
                {
                    Font = tabFont,
                    Tag = key
                };

                var tb = new System.Windows.Forms.TextBox
                {
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
                    Font = textFont,
                    Text = comment
                };

                page.Controls.Add(tb);

                tabControl1.TabPages.Add(page);
                ThemeManager.ApplyThemeToControl(tb);
            }
        }
        private void UpdateDateRange(object sender, EventArgs e)
        {
            if (radioWeek.Checked)
            {
                SetCurrentWeek();
            }
            else if (radioMonth.Checked)
            {
                SetCurrentMonth();
            }
            else if (radioPrevMonth.Checked)
            {
                SetPreviousMonth();
            }
        }
        private void SetCurrentWeek()
        {
            DateTime today = DateTime.Today;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime monday = today.AddDays(-diff);
            DateTime sunday = monday.AddDays(6);

            dateTimePicker1.Value = monday;
            dateTimePicker2.Value = sunday;
        }
        private void SetCurrentMonth()
        {
            DateTime now = DateTime.Today;

            DateTime start = new DateTime(now.Year, now.Month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            dateTimePicker1.Value = start;
            dateTimePicker2.Value = end;
        }
        private void SetPreviousMonth()
        {
            DateTime now = DateTime.Today;

            DateTime start = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            DateTime end = new DateTime(now.Year, now.Month, 1).AddDays(-1);

            dateTimePicker1.Value = start;
            dateTimePicker2.Value = end;
        }

        private async void materialButton2_Click(object sender, EventArgs e)
        {
            string searchKey = materialTextBox1.Text.Trim();

            if (toolStripMenuItem1.Enabled)
            {
                if (string.IsNullOrEmpty(searchKey))
                    return;
                OpenIssueTab(searchKey);
            }
            else
            {
                if (tabControl1.TabPages.Count >= 2)
                {
                    tabControl1.TabPages.RemoveAt(1);
                }
                if (materialCheckbox1.Checked)
                {
                    if (string.IsNullOrEmpty(searchKey))
                        return;
                    var jira = new JiraClient(login, password);
                    var json = await jira.GetIssueJson(searchKey);
                    var extractor = new ExtractInfoFromTask(json);

                    var header = extractor.ExtractReportHeader();
                    string productName = header.ProductName;

                    string scopeUrl = header.ScopeUrl;
                    string version = scopeUrl.Substring(scopeUrl.LastIndexOf('/') + 1);
                    var issues = await jira.GetIssuesByVersion(version);
                    var resolvedIssues = issues
                    .Where(i => i.Fields?.Status?.Equals("Resolved", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
                    int tasks = resolvedIssues.Count;

                    var urls = extractor.ExtractRegressionInfo();
                    var urls2 = extractor.ExtractCycleUrls();

                    var regressionStats = urls.Count > 0
                    ? await jira.GetRegressionCycleStats(urls)
                    : new List<(string Name, int TotalCount, string URL)>();

                    var functionalStats = urls2.Count > 0
                    ? await jira.GetFunctionalCycleStats(urls2)
                    : new List<(string Name, int TotalCount, string URL)>();

                    var sb = new StringBuilder();

                    sb.AppendLine(CalculateEstimate(regressionStats, functionalStats, productName, tasks));

                    sb.AppendLine($"Скоуп: {scopeUrl}");
                    sb.AppendLine($"Количетсво задач в статусе Resolved: {tasks}");
                    sb.AppendLine();
                    foreach (var stats in functionalStats)
                    {
                        string cycleInfo = string.IsNullOrWhiteSpace(stats.Name)
                        ? ""
                        : $" {stats.Name}";
                        sb.AppendLine($"Функциональное тестирование в цикле{cycleInfo}: {stats.URL}");
                        sb.AppendLine($"Количетсво кейсов: {stats.TotalCount}");
                        sb.AppendLine();
                    }

                    foreach (var stats in regressionStats)
                    {
                        string cycleInfo = string.IsNullOrWhiteSpace(stats.Name)
                        ? ""
                        : $" {stats.Name}";
                        sb.AppendLine($"Регрессионное тестирование в цикле{cycleInfo}: {stats.URL}");
                        sb.AppendLine($"Количетсво кейсов: {stats.TotalCount}");
                        sb.AppendLine();
                    }
                    var page = new TabPage(
                    $"Оценка {productName}")
                    {
                        Font = tabFont,
                        Tag = productName
                    };

                    var tb = new System.Windows.Forms.TextBox
                    {
                        Multiline = true,
                        Dock = DockStyle.Fill,
                        ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
                        Font = textFont,
                        Text = sb.ToString()
                    };

                    page.Controls.Add(tb);

                    tabControl1.TabPages.Add(page);
                    tabControl1.SelectedTab = page;
                }
                else 
                {
                    string input = manualInputTextBox.Text;
                    // Парсим данные
                    var (productName, tasksCount, functionalCount, regressionCount) = ParseManualData(input);
                    var functionalStats = new List<(string Name, int TotalCount, string URL)> { ("", functionalCount, "") };
                    var regressionStats = new List<(string Name, int TotalCount, string URL)> { ("", regressionCount, "") };

                    // Вызываем метод полного отчёта (тот же, что для автоматической оценки)
                    string report = CalculateEstimate(regressionStats, functionalStats, productName, tasksCount);
                    var page = new TabPage(
                    $"Оценка {productName}")
                    {
                        Font = tabFont,
                        Tag = productName
                    };

                    var tb = new System.Windows.Forms.TextBox
                    {
                        Multiline = true,
                        Dock = DockStyle.Fill,
                        ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
                        Font = textFont,
                        Text = report
                    };

                    page.Controls.Add(tb);

                    tabControl1.TabPages.Add(page);
                    tabControl1.SelectedTab = page;
                }
            }
        }

        private static int RoundToNearestHalfHour(int minutes)
        {
            double hours = minutes / 60.0;
            double roundedHours = Math.Round(hours * 2) / 2;
            return (int)(roundedHours * 60);
        }

        private static string FormatHalfHours(int minutes)
        {
            int hours = minutes / 60;
            int mins = minutes % 60;
            if (mins == 30)
                return $"{hours},5";
            else
                return hours.ToString();
        }

        public static string CalculateEstimate(
            List<(string Name, int TotalCount, string URL)> regressionStats,
            List<(string Name, int TotalCount, string URL)> functionalStats,
            string productName, int tasks)
        {
            // 1. Сырые минуты на тестирование
            int regressionMinutes = regressionStats.Sum(r => r.TotalCount * 25);
            int functionalMinutes = functionalStats.Sum(f => f.TotalCount * 45) + tasks * 45;
            int testingRawMinutes = regressionMinutes + functionalMinutes;

            // 2. Сырые минуты на дефекты/уточнения/ревью = 30% от сырого тестирования
            double secondaryRawMinutes = testingRawMinutes * 0.30;

            // 3. Округляем каждую часть до 0.5 часа
            int testingRoundedMinutes = RoundToNearestHalfHour(testingRawMinutes);
            int secondaryRoundedMinutes = RoundToNearestHalfHour((int)secondaryRawMinutes);

            // 4. Итог = округлённое тестирование + округлённые дефекты + 30 минут (0.5 часа)
            int totalMinutes = testingRoundedMinutes + secondaryRoundedMinutes + 30;

            // 5. Итог тоже кратен 30, форматируем
            string formattedTotal = FormatHalfHours(totalMinutes);

            
            double secondaryRawHours = secondaryRawMinutes / 60.0;
            double testingRoundedHours = testingRoundedMinutes / 60.0;
            double secondaryRoundedHours = secondaryRoundedMinutes / 60.0;
            double testingRawHours = testingRawMinutes / 60.0;
            int funcCount = functionalStats.Sum(f => f.TotalCount);
            int regCount = regressionStats.Sum(r => r.TotalCount);
            // 6. Формируем отладочное сообщение (как в исходном примере)
            var sb = new StringBuilder();
            sb.AppendLine($"Предварительная оценка трудозатрат на тестирование релиза {productName}:");
            sb.AppendLine($"Оценка - 0,5 чч");
            sb.AppendLine($"Тестирование - {FormatHalfHours(testingRoundedMinutes)} чч");
            sb.AppendLine($"Заведение дефектов/уточнения/ревью тестов - {FormatHalfHours(secondaryRoundedMinutes)} чч");
            sb.AppendLine($"Итого: {formattedTotal} чч");
            sb.AppendLine();
            sb.AppendLine("Данные на основе которых был произведен расчет:");
            sb.AppendLine($"• Функциональное тестирование: ({funcCount} кейсов + {tasks} задач) × 45 мин = {functionalMinutes} мин");
            sb.AppendLine($"• Регрессионное тестирование: {regCount} кейсов × 25 мин = {regressionMinutes} мин");
            sb.AppendLine($"• Время тестирования: {functionalMinutes} + {regressionMinutes} = {testingRawMinutes} мин = {testingRawHours:F2} ч");
            sb.AppendLine($"• Округление до 0,5 ч: {testingRawHours:F2} ч → {testingRoundedHours:F1} ч ({FormatHalfHours(testingRoundedMinutes)} чч)");
            sb.AppendLine();
            sb.AppendLine($"• 30% от {testingRawMinutes} мин = {secondaryRawMinutes:F1} мин = {secondaryRawHours:F2} ч");
            sb.AppendLine($"• Округление до 0,5 ч: {secondaryRawHours:F2} ч → {secondaryRoundedHours:F1} ч ({FormatHalfHours(secondaryRoundedMinutes)} чч)");
            sb.AppendLine();
            sb.AppendLine($"• Тестирование (округл.): {FormatHalfHours(testingRoundedMinutes)} чч");
            sb.AppendLine($"• Дефекты (округл.): {FormatHalfHours(secondaryRoundedMinutes)} чч");
            sb.AppendLine($"• Оценка: 0,5 чч");
            sb.AppendLine($"• Сумма: {FormatHalfHours(testingRoundedMinutes)} + {FormatHalfHours(secondaryRoundedMinutes)} + 0,5 = {formattedTotal} чч");
            sb.AppendLine();
            return sb.ToString();
        }


        private void OpenIssueTab(string issueKey)
        {
            var page = tabControl1.TabPages
                    .Cast<TabPage>()
                    .FirstOrDefault(p =>
                        p.Text.Contains(issueKey));

            if (page == null)
            {
                MaterialMessageBox.Show($"Задача {issueKey} не найдена", "Поиск",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            tabControl1.SelectedTab = page;
        }

        private ReportStats ParseReportStats(string text)
        {
            text = text.Replace("\r", "");

            int planned = SumMatches(text, new[]
            {
            @"Количество\s+запланированных\s+тест[\-\s]?кейсов[:\s]+(\d+)",
            @"Количество\s+запланированных\s+тестов[:\s]+(\d+)",
            @"Количество\s+запланированных\s+тест\s+кейсов[:\s]+(\d+)",
            @"Всего\s+ТК\s*[:\s]+(\d+)",
            @"ФЦ\s+(\d+)",
            @"РЦ\s+(\d+)"
            });

            int blocked = SumMatches(text, new[]
            {
                @"Заблокированных[:\s]+(\d+)",
                @"Блокировано[:\s]+(\d+)",
                @"Проверка\s+заблокирована[:\s]+(\d+)",
                @"Заблокировано[:\s]+(\d+)",
                @"BLOCKED[:\s]+(\d+)"
            });

            int autoTests = SumMatches(text, new[]
            {
                @"Пройдено\s+АТ[:\s]+(\d+)",
                @"из\s+них\s+(\d+)\s+пройдено\s+АТ",
                @"из\s+них\s+auto\s*[-:]\s*(\d+)"
            });

            int review = SumMatches(text, new[]
            {
                @"Проведено\s+ревью\s+тест[\-\s]?кейсов[:\s]+(\d+)",
                @"Проведено\s+ревью[:\s]+(\d+)",
                @"Проведено\s+ревью\s+ТК[:\s]+(\d+)",
                @"Общее\s+количество[:\s]+(\d+)",
                @"Фильтр\s+ТК.*?\((\d+)\)"
            });

            int total = planned - blocked - autoTests;

            if (total < 0)
                total = 0;

            return new ReportStats
            {
                Planned = planned,
                Blocked = blocked,
                AutoTests = autoTests,
                TotalTests = total,
                Review = review
            };
        }
        private int SumMatches(string text, string[] patterns)
        {
            int total = 0;

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(
                    text,
                    pattern,
                    RegexOptions.IgnoreCase |
                    RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count < 2)
                        continue;

                    if (int.TryParse(match.Groups[1].Value, out int value))
                    {
                        total += value;
                    }
                }
            }

            return total;
        }

        public void UpdateExcelReport(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);

            var ws = workbook.Worksheet(1);

            // вставляем новые столбцы начиная с D
            ws.Column("D").InsertColumnsBefore(4);

            // заголовки
            ws.Cell("D3").Value = "Вид тестирования (ФТ, АТ, НТ)";
            ws.Cell("E3").Value = "Отладка/разработка для АТ";
            ws.Cell("F3").Value = "Количество выполненных кейсов";
            ws.Cell("G3").Value = "Ревью";

            // стиль заголовков
            var headerRange = ws.Range("D3:G3");

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontName = "Arial";
            headerRange.Style.Font.FontSize = 11;

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            headerRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            headerRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            int lastRow = ws.LastRowUsed().RowNumber();

            for (int row = 5; row <= lastRow; row++)
            {
                // предполагаем что issue key в A
                string issueKey = ws.Cell(row, 1).GetString().Trim();

                if (string.IsNullOrWhiteSpace(issueKey))
                    continue;

                if (!reportStats.TryGetValue(issueKey, out var stats))
                    continue;

                // D = ФТ
                ws.Cell(row, "D").Value = "ФТ";

                // E пустой

                // F = количество кейсов
                ws.Cell(row, "F").Value = stats.TotalTests;

                // G = ревью
                ws.Cell(row, "G").Value = stats.Review;

                // границы
                var rowRange = ws.Range(row, 4, row, 7);

                rowRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                rowRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;

                // выравнивание
                rowRange.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                rowRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                // шрифт
                rowRange.Style.Font.FontName = "Arial";
                rowRange.Style.Font.FontSize = 11;

                // F красный если 0
                if (stats.TotalTests == 0)
                {
                    ws.Cell(row, "F").Style.Fill.BackgroundColor =
                        XLColor.Red;
                }

                // F желтый если blocked == 0
                if (stats.Blocked == 0)
                {
                    ws.Cell(row, "F").Style.Fill.BackgroundColor =
                        XLColor.Yellow;
                }

                // G красный если review == 0
                if (stats.Review == 0)
                {
                    ws.Cell(row, "G").Style.Fill.BackgroundColor =
                        XLColor.Red;
                }
            }

            int dataStartRow = 5;          // у тебя данные начинаются с 5 строки
            int dataEndRow = lastRow - 1;
            lastRow++;
            ws.Cell(lastRow, 6).FormulaA1 = $"SUM(F{dataStartRow}:F{dataEndRow})";
            ws.Cell(lastRow, 7).FormulaA1 = $"SUM(G{dataStartRow}:G{dataEndRow})";
            ws.Cell(lastRow, 11).FormulaA1 = $"SUM(K{dataStartRow}:K{dataEndRow})/3600";
            ws.Cell(lastRow, 8).FormulaA1 =
                $"SUM(F{dataStartRow}:F{dataEndRow}) + SUM(G{dataStartRow}:G{dataEndRow})";
            // ширина
            ws.Columns("D:G").AdjustToContents();
            ws.Column(4).Width = 18; // примерно 320px
            ws.Column(5).Width = 20; // примерно 320px
            ws.Column(6).Width = 20; // примерно 320px
            ws.Column(7).Width = 10; // примерно 320px
            workbook.Save();

            MaterialMessageBox.Show($"Отчет установлен на рабочий стол", "Отчет",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public string ConvertToXlsxWithLayout(string filePath)
        {
            var report = ParseFullJiraHtml(filePath);

            var xlsxPath = Path.ChangeExtension(filePath, ".xlsx");

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Jira Report");

            int row = 1;

            // =========================
            // 🔝 HEADER (2 строки)
            // =========================
            foreach (var header in report.HeaderRows)
            {
                var parts = header.Split('|');

                var cell = ws.Cell(row, 1);
                ws.Range(row, 1, row, 8).Merge();

                if (parts.Length == 2 && parts[1].StartsWith("http"))
                {
                    cell.Value = parts[0];

                    cell.SetHyperlink(new XLHyperlink(parts[1]));
                    cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                    cell.Style.Font.FontColor = XLColor.Blue;
                }
                else
                {
                    cell.Value = header;
                }

                var r = ws.Range(row, 1, row, 8);
                r.Style.Font.FontName = "Arial";
                r.Style.Font.FontSize = 11;
                r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }

            // =========================
            // 📊 TABLE HEADER
            // =========================
            var headers = new[]
            {
                "Key", "Issue Type", "Summary", "Status",
                "Resolved", "Created", "Σ Time Spent", "Assignee"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var c = ws.Cell(row, i + 1);
                c.Value = headers[i];

                c.Style.Font.Bold = true;
                c.Style.Font.FontName = "Arial";
                c.Style.Font.FontSize = 11;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            row++;

            // =========================
            // 📊 DATA
            // =========================
            foreach (var issue in report.Issues)
            {
                var cell = ws.Cell(row, 1);

                cell.Value = issue.Key;

                cell.SetHyperlink(new XLHyperlink($"https://jira.mos.social/browse/{issue.Key}"));
                cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                cell.Style.Font.FontColor = XLColor.Blue;

                ws.Cell(row, 2).Value = issue.issueType;
                ws.Cell(row, 3).Value = issue.Summary;
                ws.Cell(row, 4).Value = issue.Status;
                ws.Cell(row, 5).Value = issue.resolutionDate;
                ws.Cell(row, 6).Value = issue.Created;
                long timeSpentSec = 0;
                long.TryParse(issue.TimeSpent, out timeSpentSec);
                ws.Cell(row, 7).Value = timeSpentSec;
                ws.Cell(row, 8).Value = issue.Assignee;

                var rng = ws.Range(row, 1, row, 8);

                rng.Style.Font.FontName = "Arial";
                rng.Style.Font.FontSize = 11;
                rng.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rng.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            // =========================
            // 🔻 FOOTER
            // =========================
            if (report.FooterRows.Count > 0)
            {

                foreach (var footer in report.FooterRows)
                {
                    var cell = ws.Cell(row, 1);
                    cell.Value = footer;

                    ws.Range(row, 1, row, 8).Merge();

                    var r = ws.Range(row, 1, row, 8);

                    r.Style.Font.FontName = "Arial";
                    r.Style.Font.FontSize = 11;

                    row++;
                }
            }

            ws.Columns().AdjustToContents();
            
            ws.Column(3).Width = 45; // примерно 320px
            ws.Column(3).Style.Alignment.WrapText = true;



            wb.SaveAs(xlsxPath);

            return xlsxPath;
        }
        public JiraReport ParseFullJiraHtml(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var doc = new HtmlAgilityPack.HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };

            // 🔥 FIX ENCODING (Jira часто кривой HTML)
            var html = File.ReadAllText(filePath, Encoding.UTF8);
            doc.LoadHtml(html);

            var report = new JiraReport();
            // =========================
            // 🔝 HEADER (2 строки TABLE)
            // =========================
            var headerTable = doc.DocumentNode.SelectSingleNode("//table[tr/td[@colspan='8']]");

            if (headerTable != null)
            {
                var rowsS = headerTable.SelectNodes(".//tr");

                foreach (var r in rowsS)
                {
                    var linkNode = r.SelectSingleNode(".//a");

                    if (linkNode != null)
                    {
                        var text = HtmlEntity.DeEntitize(linkNode.InnerText).Trim();
                        var href = linkNode.GetAttributeValue("href", "");

                        if (!string.IsNullOrWhiteSpace(text))
                            report.HeaderRows.Add($"{text}|{href}");

                        continue;
                    }

                    var textOnly = HtmlEntity.DeEntitize(r.InnerText);
                    textOnly = FixBrokenEncoding(textOnly)?.Trim();

                    // ❗ ГЛАВНЫЙ ФИКС — фильтр пустых строк
                    if (string.IsNullOrWhiteSpace(textOnly))
                        continue;

                    // убираем мусорные строки типа только пробелы/переносы
                    if (textOnly.Length < 3)
                        continue;

                    report.HeaderRows.Add(textOnly);
                }
            }

            // =========================
            // 📊 ISSUES
            // =========================
            var rows = doc.DocumentNode.SelectNodes("//tr[contains(@class,'issuerow')]");

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var key = row.SelectSingleNode(".//td[contains(@class,'issuekey')]//a")?.InnerText?.Trim();
                    var issuetype = row.SelectSingleNode(".//td[contains(@class,'issuetype')]")?.InnerText?.Trim();
                    var summary = row.SelectSingleNode(".//td[contains(@class,'summary')]")?.InnerText?.Trim();
                    var status = row.SelectSingleNode(".//td[contains(@class,'status')]")?.InnerText?.Trim();

                    var resolutiondate = row.SelectSingleNode(".//td[contains(@class,'resolutiondate')]")?.InnerText?.Trim();
                    var created = row.SelectSingleNode(".//td[contains(@class,'created')]")?.InnerText?.Trim();

                    var time = row.SelectSingleNode(".//td[contains(@class,'aggregatetimespent')]")?.InnerText?.Trim();
                    var assignee = row.SelectSingleNode(".//td[contains(@class,'assignee')]")?.InnerText?.Trim();

                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    report.Issues.Add(new JiraIssueRow
                    {
                        Key = key,
                        issueType = issuetype,
                        Summary = summary,
                        Status = status,
                        resolutionDate = resolutiondate,
                        Created = created,
                        TimeSpent = time,
                        Assignee = assignee
                    });
                }
            }

            // =========================
            // 🔻 FOOTER (последняя таблица)
            // =========================
            var footerTable = doc.DocumentNode.SelectSingleNode("//div[@class='end-of-stable-message']/following::table[1]");

            if (footerTable != null)
            {
                var text = HtmlEntity.DeEntitize(footerTable.InnerText);

                text = Regex.Replace(text, @"\s+", " ").Trim();

                report.FooterRows.Add(text);
            }

            return report;
        }
        private string FixBrokenEncoding(string input)
        {
            try
            {
                var bytes = Encoding.GetEncoding(1252).GetBytes(input);
                return Encoding.GetEncoding(1251).GetString(bytes);
            }
            catch
            {
                return input;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            materialLabel1.Visible = true;
            materialLabel2.Visible = true;
            dateTimePicker1.Visible = true;
            dateTimePicker2.Visible = true;
            radioMonth.Visible = true;
            radioPrevMonth.Visible = true;
            radioWeek.Visible = true;
            materialButton1.Visible = true;
            materialButton2.Text = "Поиск";
            tabControl1.TabPages.Clear();
            this.Text = "Отчет за месяц";
            toolStripMenuItem1.Enabled = true;
            toolStripMenuItem2.Enabled = false;
            toolTip1.SetToolTip(materialTextBox1, "Пример QA-1111 или 1111");
            var page = new TabPage(
            $"Page1")
            {
                Font = tabFont
            };
            tabControl1.TabPages.Add(page);
            ThemeManager.ApplyThemeToControl(tabControl1);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            enterFirst();
        }
        private void enterFirst()
        {
            materialLabel1.Visible = false;
            materialLabel2.Visible = false;
            dateTimePicker1.Visible = false;
            dateTimePicker2.Visible = false;
            radioMonth.Visible = false;
            radioPrevMonth.Visible = false;
            radioWeek.Visible = false;
            materialButton1.Visible = false;
            materialButton2.Text = "Оценить";
            tabControl1.TabPages.Clear();
            this.Text = "Оценка";
            toolStripMenuItem1.Enabled = false;
            toolStripMenuItem2.Enabled = true;
            toolTip1.SetToolTip(materialTextBox1, "Пример QA-1111");
            var page = new TabPage(
            $"Ручная оценка (заполните данные и уберите чекбокс)")
            {
                Font = tabFont
            };

            var txtData = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
                Font = textFont,
                Text =
                @"Предварительная оценка трудозатрат на тестирование релиза Название_релиза
Количество задач: 0
Количество ФЦ тк: 0
Количество РЦ тк: 0"
            };
            manualInputTextBox = txtData;
            page.Controls.Add(txtData);

            tabControl1.TabPages.Add(page);
            ThemeManager.ApplyThemeToControl(txtData);
        }
    }


    public class JiraIssueRow
    {
        public string Key { get; set; }
        public string issueType { get; set; }
        public string Summary { get; set; }
        public string Status { get; set; }
        public string resolutionDate { get; set; }
        public string Created { get; set; }
        public string TimeSpent { get; set; }
        public string Assignee { get; set; }
    }
    public class JiraReport
    {
        public List<string> HeaderRows { get; set; } = new List<string>();
        public List<JiraIssueRow> Issues { get; set; } = new List<JiraIssueRow>();
        public List<string> FooterRows { get; set; } = new List<string>();
    }
}
