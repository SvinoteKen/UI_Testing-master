using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;
using static UI_Testing.ExtractInfoFromTask;
using static UI_Testing.JiraClient;

namespace UI_Testing
{
    public partial class MainForm : MaterialForm
    {
        private string? login;
        private string? password;
        public List<List<object>> rows;
        private ShowError showError = new ShowError();
        public GoogleSheetsHelper helper;
        private readonly MaterialSkinManager materialSkinManager;
        private bool theFirst = true;
        public MainForm(string? _login, string? _password)
        {
            InitializeComponent();
            // Инициализация менеджера скинов
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);

            login = _login;
            password = _password;
            toolStripMenuItem5.Enabled = false;
            tableLayoutPanel1.Visible = true;
            tableLayoutPanel3.Visible = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            List<string> headers = new List<string>
                {
                    "№", "Тип", "ТК", "Название", "Кол-во тестов",
                    "Кол-во шагов", "Ревью ТК","Статус ТК", "Тестировщик", "Комментарий", "Статус задачи"
                };
            foreach (var header in headers)
            {
                var column = new DataGridViewTextBoxColumn
                {
                    HeaderText = header,
                    Name = header,
                    ReadOnly = false
                };
                dataGridViewPreview.Columns.Add(column);
            }
            dataGridViewPreview.AllowUserToAddRows = true;
            dataGridViewPreview.AllowUserToDeleteRows = true;
            dataGridViewPreview.AllowUserToOrderColumns = false;
            dataGridViewPreview.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dataGridViewPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPreview.MultiSelect = true;
            dataGridViewPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (DataGridViewColumn column in dataGridViewPreview.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        public void SetCredentials(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
        private async Task<List<List<object>>> LoadRowsFromJira(string version)
        {
            var jira = new JiraClient(login, password);

            var issues = await jira.GetIssuesByVersion(version);
            var processedCases = new HashSet<string>();
            var rows = new List<List<object>>();
            int taskNumber = 1;

            foreach (var issue in issues)
            {
                var issueData = await jira.GetIssueData(issue.Key);
                if (issueData == null) continue;

                bool added = await ProcessIssueAndCases(jira, issueData, taskNumber, processedCases, rows);
                if (added)
                    taskNumber++;
            }
            return rows;
        }
        private async void materialButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripMenuItem1.Enabled || !toolStripMenuItem5.Enabled)
                {
                    string version = textBoxVersion.Text;
                    if (version == "") { MaterialMessageBox.Show(this, "Заполните поле \"Скоуп\""); return; }
                    MaterialMessageBox.Show(this, "Ожидайте, данные появятся в таблице предпросмотра");
                    if (!materialCheckbox1.Checked)
                    {
                        rows = await LoadRowsFromJira(version);
                    }
                    else
                    {
                        rows = await ExportTestCasesFromUrl(version);
                    }
                    if (checkBoxPreview.Checked)
                    {
                        if (materialCheckbox.Checked)
                        {
                            FillPreviewTable(rows, true);
                        }
                        else
                        {
                            FillPreviewTable(rows, false);
                        }
                    }
                }
                else
                {
                    string issueKey = ExtractInfoFromTask.ExtractIssueKey(textBoxVersion.Text.Trim());
                    if (string.IsNullOrEmpty(issueKey))
                    {
                        MaterialMessageBox.Show(this, "Не удалось извлечь ключ задачи из ссылки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 2. Получаем JSON по ключу задачи
                    var jira = new JiraClient(login, password);
                    var json = await jira.GetIssueJson(issueKey);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        MaterialMessageBox.Show(this, "Не удалось получить json содержимое задачи. Проверь авторизацию или ссылку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 3. Создаём extractor для работы с данными задачи
                    var extractor = new ExtractInfoFromTask(json);

                    // 4. Извлекаем данные из json
                    var header = extractor.ExtractReportHeader();
                    string productName = header.ProductName;
                    string scopeUrl = header.ScopeUrl;
                    string zni = header.Zni;

                    string gdUrl = textBoxSheetUrl.Text.Trim();

                    var urls = extractor.ExtractRegressionInfo();
                    var urls2 = extractor.ExtractCycleUrls();

                    if (urls == null || urls.Count == 0)
                    {
                        MaterialMessageBox.Show(this, "Не удалось найти ссылку на регрессионный цикл.\nЕсли по задаче его не предусмотрено, то все ОК", "Информирование");
                        urls = new List<RegressionInfo>();
                    }

                    // 5. Получаем статистику по регрессионным циклам
                    var regressionStats = urls.Count > 0
                    ? await jira.GetCycleTestStatsForCycles(urls)
                    : new List<CycleTestStats>();

                    // 6. Проверяем ссылку на Google Sheet
                    string sheetUrl = header.YandexDiskUrl;

                    SheetStats stats = new SheetStats();
                    if (checkBoxNoStyle.Checked)
                    {
                        if (string.IsNullOrEmpty(gdUrl))
                        {
                            MaterialMessageBox.Show(this, "Введите URL Google таблицы.");
                            return;
                        }

                        string sourcesheetID = helper.GetSpreadsheetId(gdUrl);

                        // 7. Определяем имя листа
                        string worksheetId = helper.GetWorksheetByGid(gdUrl);
                        var parts = worksheetId.Split('|');
                        string spreadsheetId = parts[0];
                        string sheetName = parts[1];

                        // 8. Загружаем данные из таблицы
                        var data = await helper.GetSheetData(sourcesheetID, $"{sheetName}!A1:Z1000");
                        stats = AnalyzeSheetData(data);
                    }
                    else 
                    {
                        stats = AnalyzeGridData(dataGridViewPreview);
                    }
                    // 9. Достаём дату создания задачи
                    string date = extractor.ExtractIsoDate();

                    // 10. Строим ссылки на дефекты
                    var urlsDefects = await BuildDefectUrls(issueKey, date);
                    
                    checkBoxPreview.Checked = false;
                    // 11. Формируем отчёт
                    textBox1.Text = FormatReport(
                        regressionStats,
                        productName,
                        scopeUrl,
                        zni,
                        urls2,
                        sheetUrl,
                        stats,
                        urlsDefects.foundBugsUrl, // foundUrl
                        urlsDefects.knownBugsUrl  // knownUrl
                    );
                }
            }
            catch (Exception ex)
            {
                showError.ShowErr(this, "Ошибка отчета", ex.ToString());
            }
        }

        private async Task <(string foundBugsUrl, string knownBugsUrl)> BuildDefectUrls(string issueKey, string date)
        {
            string baseUrl = "https://jira.mos.social/issues/?jql=";

            string foundUrl = baseUrl + Uri.EscapeDataString($"issuetype in (Bug, Improvement, Task) AND createdDate >= {date} AND issue in linkedIssues({issueKey}) ORDER BY priority DESC");
            string knownUrl = baseUrl + Uri.EscapeDataString($"issuetype in (Bug, Improvement, Task) AND createdDate <= {date} AND issue in linkedIssues({issueKey}) ORDER BY priority DESC");
            var jira = new JiraClient(login, password);
            string found = await jira.IssuesJiraReportAsync($"issuetype in (Bug, Improvement, Task) AND createdDate >= {date} AND issue in linkedIssues({issueKey}) ORDER BY priority DESC");
            string know = await jira.IssuesJiraReportAsync($"issuetype in (Bug, Improvement, Task) AND createdDate <= {date} AND issue in linkedIssues({issueKey}) ORDER BY priority DESC");
            return (found, know);
        }
        private string FormatReport(
            List<CycleTestStats> regressionStats,
            string productName,
            string scopeUrl,string zni, List<RegressionInfo> urls,
            string gdUrl, SheetStats sheetStats, string foundUrl, string knownUrl)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Отчет о тестировании продукта: {productName}");
            sb.AppendLine($"ЗНИ: {zni}");
            sb.AppendLine($"Скоуп: {scopeUrl}");
            sb.AppendLine($"Прогресс тестирования зафиксирован в ЯД: {gdUrl}");
            sb.AppendLine();
            
            sb.AppendLine($"Всего задач: {sheetStats.TotalTasks}");
            sb.AppendLine($"Закрыто: {sheetStats.ClosedTasks}");
            sb.AppendLine($"Переоткрытых задач: {sheetStats.ReopenedTasks}");

            if (sheetStats.UntestedTaskLinks.Count > 0)
            {
                var linksText = string.Join(",\n ", sheetStats.UntestedTaskLinks);

                sb.AppendLine($"Не протестированных задач: {sheetStats.UntestedTaskLinks.Count} ({linksText})");
            }
            else
            {
                sb.AppendLine("Не протестированных задач: 0");
            }
            sb.AppendLine();

            if (urls.Count > 0)
            {
                if (urls.Count == 1)
                {
                    sb.AppendLine($"Функциональное тестирование в цикле: {urls[0].Url}");
                }
                else
                {
                    sb.AppendLine("Функциональное тестирование в циклах:");
                    foreach (var url in urls)
                    {
                        sb.AppendLine($"{url.Url}");
                    }
                }
            }
            sb.AppendLine();
            sb.AppendLine($"Количество запланированных тест-кейсов: {sheetStats.PlannedTestCases}");
            sb.AppendLine();
            sb.AppendLine($"Успешно: {sheetStats.Passed}");
            sb.AppendLine($"Успешно с багом: {sheetStats.PassedWithBug}");
            sb.AppendLine($"Провалено: {sheetStats.Failed}");
            sb.AppendLine($"Заблокировано: {sheetStats.Blocked}");

            sb.AppendLine($"Проведено ревью тест-кейсов: {sheetStats.ReviewedTestCases}");
            sb.AppendLine();
            foreach (var stats in regressionStats)
            {
                string cycleInfo = string.IsNullOrWhiteSpace(stats.RegName)
                ? ""
                : $" {stats.RegName}";
                sb.AppendLine($"Регрессионное тестирование в цикле{cycleInfo}: {stats.Url}");
                sb.AppendLine();

                if (stats.JenkinsPassed > 0)
                {
                    sb.AppendLine($"Всего тест-кейсов: {stats.Total}  (из них {stats.JenkinsPassed} пройдено АТ)");
                }
                else
                {
                    sb.AppendLine($"Всего тест-кейсов: {stats.Total}");
                }
                sb.AppendLine($"Успешно пройденных тест-кейсов: {stats.Passed}");
                sb.AppendLine($"Проваленных тест-кейсов: {stats.Failed}");
                sb.AppendLine($"Пройденных с ошибкой тест-кейсов: {stats.Errored}");
                sb.AppendLine($"Заблокированных тест-кейсов: {stats.Blocked}");
                sb.AppendLine();
            }
            sb.AppendLine($"Найденные дефекты: {foundUrl}");
            sb.AppendLine();
            sb.AppendLine($"Известные дефекты: {knownUrl}");
            return sb.ToString();
        }
        private async Task<bool> ProcessIssueAndCases(JiraClient jira, JiraIssue issue, int taskNumber, HashSet<string> processedCases, List<List<object>> rows)
        {
            string issueKey = issue.Key;
            var fields = issue.Fields;
            string status = fields.Status == "Waiting for release" ? "Waiting for Release" : fields.Status;

            if (checkBoxIteration.Checked && (status == "Waiting for Release" || status == "Closed" || status == "Documentation"))
                return false; // задача пропущена

            //string link = $"=ГИПЕРССЫЛКА(\"{JiraClient.BaseUrl}/browse/{issueKey}\"; \"{issueKey}\")";
            var testCases = await jira.GetLinkedTestCases(issue, blocksTC.Checked);
            string test_count = (status == "Closed") ? "" : ((testCases == null || !testCases.Any()) ? "1" : "");

            var row = new List<object>
            {
                taskNumber.ToString(),
                fields.Type,
                issueKey,
                fields.Summary,
                test_count, "", "", // Кол-во тестов, шагов, ревью ТК
                fields.PriorityValue,
                fields.Type != "Test" ? status : "",
                "", "" // Статус проверки, тестировщик
            };
            rows.Add(row);

            foreach (var test in testCases)
            {
                //string testLink = $"=ГИПЕРССЫЛКА(\"{JiraClient.BaseUrl}/browse/{test.Key}\"; \"{test.Key}\")";
                bool isNew = processedCases.Add(test.Key);

                var caseRow = new List<object>
                {
                    "", "Test case", test.Key, test.Fields.Summary,
                    isNew ? "1" : "0", test.Fields.Steps.ToString(), // Кол-во тестов, шагов
                    test.Fields.Status == "In Review" ? "На ревью" : "",
                    test.Fields.PriorityValue, "", "", ""
                };
                rows.Add(caseRow);
            }

            return true; // задача добавлена
        }

        public async Task<List<List<object>>> ExportTestCasesFromUrl(string url)
        {
            var jira = new JiraClient(login, password);
            string zql = jira.ParseZqlUrl(url);
            var testCases = await jira.GetTestCasesFromCycle(zql);

            var rows = new List<List<object>>();
            int number = 1;

            foreach (var test in testCases)
            {
                //string testLink = $"=ГИПЕРССЫЛКА(\"{JiraClient.BaseUrl}/browse/{test.Key}\"; \"{test.Key}\")";
                string executionStatus = test.Fields.ExecutionStatus switch
                {
                    "WIP" => "В работе",
                    "PASS" => "Pass",
                    "PASS WITH BUG" => "Pass with bug",
                    "BLOCKED" => "Blocked",
                    "FAIL" => "Fail",
                    _ => test.Fields.ExecutionStatus // если вдруг новое значение
                };


                var row = new List<object>
                {
                    number.ToString(), // ← Номер строки
                    "Test case",
                    test.Key,
                    test.Fields.Summary,
                    "1", test.Fields.Steps, // Кол-во тестов, шагов
                    test.Fields.Status == "In Review" ? "На ревью" : "", // статус
                    test.Fields.PriorityValue,
                    "", executionStatus, "" // Статус проверки, тестировщик и т.д.
                };

                rows.Add(row);
                number++; // ← Увеличиваем номер
            }

            return rows;
        }
        private void FillPreviewTable(List<List<object>> rows, bool showPriority)
        {
            dataGridViewPreview.Rows.Clear();

            // Добавляем строки
            foreach (var row in rows)
            {
                string[] cells;

                if (showPriority)
                {
                    // row должен содержать значения для всех колонок приоритетного варианта
                    cells = row.Select(cell => cell?.ToString() ?? "").ToArray();
                }
                else
                {
                    // Без "Приоритета" и с "Комментариями"

                    cells = new string[]
                    {
                        row.ElementAtOrDefault(0)?.ToString() ?? "",
                        row.ElementAtOrDefault(1)?.ToString() ?? "",
                        row.ElementAtOrDefault(2)?.ToString() ?? "",
                        row.ElementAtOrDefault(3)?.ToString() ?? "",
                        row.ElementAtOrDefault(4)?.ToString() ?? "",
                        row.ElementAtOrDefault(5)?.ToString() ?? "",
                        row.ElementAtOrDefault(6)?.ToString() ?? "",  // Статус задачи
                        row.ElementAtOrDefault(9)?.ToString() ?? "",  // Ревью ТК
                        row.ElementAtOrDefault(10)?.ToString() ?? "", // Тестировщик
                        "",                                          // Комментарий — пусто, можно редактировать
                        row.ElementAtOrDefault(8)?.ToString() ?? ""  // Статус проверки
                    };
                }

                dataGridViewPreview.Rows.Add(cells);
            }
        }

        private async void checkBoxNoStyle_CheckedChanged(object sender, EventArgs e)
        {
            if (!toolStripMenuItem2.Enabled)
            {

                if (checkBoxNoStyle.Checked)
                {
                    bool canProceed = await GoogleAuthorization.CanProceedAsync();

                    if (!canProceed)
                    {
                        checkBoxNoStyle.Checked = false;
                        return;
                    }

                    materialLabel1.Visible = true;
                    textBoxSheetUrl.Visible = true;
                }
                else
                {
                    materialLabel1.Visible = false;
                    textBoxSheetUrl.Visible = false;
                }
            }
            if (!toolStripMenuItem5.Enabled)
            {
                if (checkBoxNoStyle.Checked)
                {
                    textBoxSheetUrl.Enabled = false;
                }
                else 
                {
                    textBoxSheetUrl.Enabled = true;
                }
            }
            else
            {
                if (checkBoxNoStyle.Checked)
                {
                    textBoxStyleSheetUrl.Enabled = false;
                }
                else
                {
                    textBoxStyleSheetUrl.Enabled = true;
                    textBoxSheetUrl.Enabled = true;
                }
            }}
        

        private void materialCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            bool showPriority = materialCheckbox.Checked;
            if(rows != null && checkBoxPreview.Checked) { 
                FillPreviewTable(rows, showPriority);
            }
        }

        private async void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            bool canProceed = await GoogleAuthorization.CanProceedAsync();

            if (!canProceed)
                return;

            helper = new GoogleSheetsHelper();

            materialLabel1.Visible = true;
            materialLabel2.Visible = true;
            materialLabel3.Visible = true;
            materialLabel3.Text = "Скоуп";
            textBoxSheetUrl.Visible = true;
            textBoxStyleSheetUrl.Visible = true;
            textBoxVersion.Visible = true;
            materialButton1.Visible = true;
            checkBoxIteration.Visible = true;
            checkBoxNoStyle.Visible = true;
            checkBoxPreview.Visible = true;
            materialCheckbox.Visible = false;
            dataGridViewPreview.Visible = true;
            toolStripMenuItem2.Enabled = true;
            toolStripMenuItem1.Enabled = false;
            toolStripMenuItem5.Enabled = true;
            toolStripMenuItem3.Enabled = true;
            tableLayoutPanel3.Visible = false;
            checkBoxPreview.Checked = true;
            materialCheckbox1.Visible = true;
            tableLayoutPanel1.Visible = true;
            textBox1.Visible = false;
            materialButton2.Visible = true;
            materialButton2.Enabled = true;
            blocksTC.Visible = true;
            materialButton2.Text = "Выгрузка ГД";
            checkBoxNoStyle.Checked = false;
            materialLabel1.Text = "Ссылка на ГД";
            checkBoxNoStyle.Text = "Без стиля";
            this.Text = "ГД";
        }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            materialLabel1.Visible = true;
            materialLabel2.Visible = false;
            materialLabel3.Visible = true;
            if (materialCheckbox.Checked)
            {
                materialLabel3.Text = "Скоуп";
            }
            else 
            {
                materialLabel3.Text = "Цикл";
            }
            textBoxSheetUrl.Visible = true;
            textBoxStyleSheetUrl.Visible = false;
            textBoxVersion.Visible = true;
            materialButton1.Visible = true;
            checkBoxIteration.Visible = true;
            checkBoxNoStyle.Visible = true;
            checkBoxPreview.Visible = true;
            materialCheckbox.Visible = false;
            materialCheckbox.Checked = false;
            dataGridViewPreview.Visible = true;
            toolStripMenuItem2.Enabled = true;
            toolStripMenuItem1.Enabled = true;
            toolStripMenuItem5.Enabled = false;
            toolStripMenuItem3.Enabled = true;
            tableLayoutPanel3.Visible = false;
            checkBoxPreview.Enabled = true;
            materialCheckbox1.Visible = true;
            tableLayoutPanel1.Visible = true;
            textBox1.Visible = false;
            materialButton2.Visible = true;
            materialButton2.Enabled = true;
            blocksTC.Visible = true;
            checkBoxPreview.Checked = true;
            dataGridViewPreview.Rows.Clear();
            materialButton2.Text = "Выгрузка ЯД";
            checkBoxNoStyle.Text = "Из файла";
            checkBoxNoStyle.Checked = false;
            materialLabel1.Text = "cURL созданного листа";
            this.Text = "ЯД";
        }
        private async void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

            if (theFirst) 
            {
                MaterialMessageBox.Show(this, "Информацию с задач может брать некорректно (зни,скоп,яд, рц/фц ссылки),\nиз-за изменения шаблона задач на тестирование" +
                    "\nФикс есть, но будьте внимательны. Все статистические данные корректны", "Генерация Отчетов",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                theFirst = false;
            }

            helper = new GoogleSheetsHelper();
            materialLabel2.Visible = false;
            materialLabel3.Visible = true;
            materialLabel3.Text = "Задача";
            textBoxStyleSheetUrl.Visible = false;
            textBoxVersion.Visible = true;
            materialButton1.Visible = true;
            checkBoxIteration.Visible = false;
            checkBoxNoStyle.Visible = true;
            checkBoxPreview.Visible = true;
            checkBoxPreview.Enabled = true;
            materialCheckbox.Visible = false;
            dataGridViewPreview.Visible = true;
            toolStripMenuItem2.Enabled = false;
            toolStripMenuItem5.Enabled = true;
            checkBoxPreview.Checked = true;
            materialCheckbox1.Visible = false;
            materialButton2.Visible = false;
            materialButton2.Enabled = false;
            toolStripMenuItem1.Enabled = true;
            toolStripMenuItem3.Enabled = true;
            tableLayoutPanel3.Visible = false;
            tableLayoutPanel1.Visible = true;
            blocksTC.Visible = false;
            checkBoxNoStyle.Checked = false;
            checkBoxNoStyle.Text = "Из ГД";
            this.Text = "Отчет";
            materialLabel1.Text = "Ссылка на ГД";
            materialLabel1.Visible = false;
            textBoxSheetUrl.Visible = false;
            dataGridViewPreview.Rows.Clear();
            tableLayoutPanel1.Controls.Add(textBox1, 0, 4);
            tableLayoutPanel1.SetColumnSpan(textBox1, 4);
            ThemeManager.ApplyThemeToControl(textBox1);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            tableLayoutPanel3.Visible = false;
            tableLayoutPanel1.Visible = false;
            checkBoxNoStyle.Visible = false;
            checkBoxPreview.Visible = false;
            materialCheckbox.Visible = false;
            dataGridViewPreview.Visible = false;
            materialCheckbox1.Visible = false;
            toolStripMenuItem2.Enabled = true;
            toolStripMenuItem1.Enabled = true;
            toolStripMenuItem5.Enabled = true;
            toolStripMenuItem3.Enabled = false;
            materialButton2.Visible = false;
            materialButton2.Enabled = false;
            checkBoxPreview.Enabled = false;
            textBox1.Visible= false;
            textBox1.Enabled= false;
            blocksTC.Visible = false;
            this.Text = "Поиск";
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            materialLabel1.Visible = true;
            materialLabel2.Visible = false;
            materialLabel3.Visible = true;
            materialLabel3.Text = "Задача";
            textBoxSheetUrl.Visible = true;
            textBoxStyleSheetUrl.Visible = false;
            textBoxVersion.Visible = true;
            materialButton1.Visible = true;
            checkBoxIteration.Visible = false;
            checkBoxNoStyle.Visible = false;
            checkBoxPreview.Visible = true;
            checkBoxPreview.Enabled = false;
            materialCheckbox.Visible = false;
            dataGridViewPreview.Visible = false;
            toolStripMenuItem4.Enabled = false;
            materialCheckbox1.Visible = false;
            tableLayoutPanel1.Controls.Add(textBox1, 0, 4);
            tableLayoutPanel1.SetColumnSpan(textBox1, 4);
            textBox1.Visible = true;
            textBox1.Enabled = true;
            materialButton2.Visible = false;
            materialButton2.Enabled = false;
            toolStripMenuItem1.Enabled = true;
            toolStripMenuItem3.Enabled = true;
            toolStripMenuItem2.Enabled = true;
            tableLayoutPanel3.Visible = false;
            tableLayoutPanel1.Visible = true;
            blocksTC.Visible = false;
            this.Text = "Верификация";
        }

        private async void materialButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripMenuItem1.Enabled)
                {
                    string version = textBoxVersion.Text;
                    if (version == "" && rows.Count == 0) { MaterialMessageBox.Show(this, "Заполните поле \"Скоуп\""); return; }
                    if (!materialCheckbox1.Checked)
                    {
                        rows = await LoadRowsFromJira(version);
                    }
                    else
                    {
                        rows = await ExportTestCasesFromUrl(version);
                    }

                    // 1. Получаем URL таблицы из текстбокса
                    string sheetUrl = textBoxSheetUrl.Text.Trim();
                    if (string.IsNullOrEmpty(sheetUrl))
                    {
                        MaterialMessageBox.Show(this, "Введите URL Google таблицы.");
                        return;
                    }

                    // 2. Извлекаем ID таблицы
                    string spreadsheetId = helper.GetSpreadsheetId(sheetUrl);

                    // 3. Имя листа (можно сделать вводом в отдельное поле, сейчас хардкод)
                    string worksheetId = helper.GetWorksheetByGid(sheetUrl);

                    // 4. Формируем строки
                    var lisrRows = GetRowsForExport();
                    // 5. Добавляем строки
                    helper.AddRowsToSheet($"{worksheetId}", lisrRows);

                    if (!checkBoxNoStyle.Checked)
                    {
                        string sourceUrl = textBoxStyleSheetUrl.Text.Trim();
                        if (string.IsNullOrEmpty(sourceUrl))
                        {
                            MaterialMessageBox.Show(this, "Введите URL Google таблицы.");
                            return;
                        }
                        string sourcesheetID = helper.GetSpreadsheetId(sourceUrl);
                        int sourcesheetId = helper.GetWorksheetGid(sourceUrl);
                        int spreadId = helper.GetWorksheetGid(sheetUrl);
                        int endCol = lisrRows[0].Count;
                        int numRows = lisrRows.Count;
                        helper.CopyPasteDataValidation(spreadsheetId, sourcesheetId, spreadId, 2, 3, 0, endCol, 1, numRows);
                        helper.ClearColors(spreadsheetId, spreadId, 0, endCol, 1, numRows);
                        int i = helper.FindFirstRowWithNonWhiteBackground(sourcesheetID, sourcesheetId);
                        if (i == -1) { i = 0; }
                        helper.CopyPasteDataValidation(spreadsheetId, sourcesheetId, spreadId, i, i + 1, 0, endCol, 0, 1);
                    }

                }
                else
                {
                    string version = textBoxVersion.Text;
                    if (version == "") { MaterialMessageBox.Show(this, "Заполните поле \"Скоуп\""); return; }
                    MaterialMessageBox.Show(this, "Ожидайте, выгрузка запущена");
                    if (!materialCheckbox1.Checked)
                    {
                        rows = await LoadRowsFromJira(version);
                    }
                    else
                    {
                        rows = await ExportTestCasesFromUrl(version);
                    }
                    string curlText = textBoxSheetUrl.Text;
                    var curl = new CurlRequest();
                    if (checkBoxNoStyle.Checked)
                    {
                        curl = CurlParser.ParseFromFile("curl.txt");
                    }
                    {
                        if (curlText == "") { MaterialMessageBox.Show(this, "Заполните поле \"cURL созданного листа\""); return; }
                        curl = CurlParser.Parse(curlText);
                    }
                    // 1. Получаем timeline и snapshot
                    var (timeline, snapshotIndex) = await GetStyles.GetSessionInfo(curl);
                    curl.Timeline = timeline;

                    var lisrRows = GetRowsForExport();
                    // 2. Получаем стили
                    var stylesJson = await GetStyles.GetStyle(curl, snapshotIndex);

                    GenerateAndShowYandexData(lisrRows, curl, stylesJson);
                }
            }
            catch (Exception ex)
            {
                showError.ShowErr(this, "Ошибка выгрузки", ex.Message);
            }
        }
        private List<List<object>> GetRowsForExport()
        {
            var rowsList = new List<List<object>>();

            if (checkBoxPreview.Checked || dataGridViewPreview.Rows.Count > 0)
            {
                var headers = new List<object>();
                foreach (DataGridViewColumn column in dataGridViewPreview.Columns)
                {
                    headers.Add(column.HeaderText);
                }
                rowsList.Add(headers);

                foreach (DataGridViewRow row in dataGridViewPreview.Rows)
                {
                    if (row.IsNewRow) continue;

                    var rowData = new List<object>();
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        var cellValue = row.Cells[i].Value?.ToString() ?? "";

                        // Для 3-й колонки (index 2) создаём формулу ГИПЕРССЫЛКА
                        if (i == 2 && !string.IsNullOrWhiteSpace(cellValue) && !toolStripMenuItem1.Enabled)
                        {
                            cellValue = $"=ГИПЕРССЫЛКА(\"{JiraClient.BaseUrl}/browse/{cellValue}\"; \"{cellValue}\")";
                        }

                        rowData.Add(cellValue);
                    }
                    rowsList.Add(rowData);
                }
            }
            else
            {

                //if (!materialCheckbox.Checked)
                //{
                    // Альтернативный экспорт с материалами
                    var headers = new List<object>
                    {
                        "№", "Тип", "ТК", "Название", "Кол-во тестов",
                        "Кол-во шагов", "Ревью ТК", "Статус ТК", "Тестировщик", "Комментарий", "Статус задачи"
                    };
                    rowsList.Add(headers);

                    foreach (var row in rows)
                    {
                        string issueKey = row.ElementAtOrDefault(2)?.ToString() ?? "";
                        string url;
                        if (!toolStripMenuItem1.Enabled)
                        {
                            url = !string.IsNullOrWhiteSpace(issueKey)
                                    ? $"=ГИПЕРССЫЛКА(\"https://jira.mos.social/browse/{issueKey}\"; \"{issueKey}\")"
                                    : "";
                        }
                        else 
                        {
                            url = issueKey;
                        }
                        var cells = new List<object>
                        {
                            row.ElementAtOrDefault(0)?.ToString() ?? "",
                            row.ElementAtOrDefault(1)?.ToString() ?? "",
                            url,
                            row.ElementAtOrDefault(3)?.ToString() ?? "",
                            row.ElementAtOrDefault(4)?.ToString() ?? "",
                            row.ElementAtOrDefault(5)?.ToString() ?? "",
                            row.ElementAtOrDefault(6)?.ToString() ?? "",  // Ревью ТК
                            row.ElementAtOrDefault(9)?.ToString() ?? "",  // Статус задачи
                            row.ElementAtOrDefault(10)?.ToString() ?? "", // Тестировщик
                            "",                                           // Комментарий
                            row.ElementAtOrDefault(8)?.ToString() ?? ""   // Статус проверки
                        };
                        rowsList.Add(cells);
                    }
                    /*}
                    else
                    {
                        // Стандартный экспорт
                        var headers = new List<object>
                        {
                            "№", "Тип", "ТК", "Название", "Кол-во тестов",
                            "Кол-во шагов", "Ревью ТК", "Приоритет",
                            "Статус задачи", "Статус проверки", "Тестировщик"
                        };
                        rowsList.Add(headers);

                        foreach (var row in rows)
                        {
                            // Для стандартного экспорта тоже можно при желании сделать формулу, если нужно
                            string issueKey = row.ElementAtOrDefault(2)?.ToString() ?? "";

                            var newRow = row.Select((cell, index) =>
                                index == 2 && !string.IsNullOrWhiteSpace(issueKey)
                                    ? $"=ГИПЕРССЫЛКА(\"https://jira.mos.social/browse/{issueKey}\"; \"{issueKey}\")"
                                    : cell
                            ).ToList();

                            rowsList.Add(newRow);
                        }
                    }*/
            }

            return rowsList;
        }
        private SheetStats AnalyzeSheetData(IList<IList<object>> data)
        {
            var header = data[0].Select(c => c.ToString().Trim()).ToList();
            int statusIndex = header.FindIndex(h => h.IndexOf("Статус задачи", StringComparison.OrdinalIgnoreCase) >= 0);
            int testStatusIndex = header.FindIndex(h =>
                h.IndexOf("Статус теста", StringComparison.OrdinalIgnoreCase) >= 0 ||
                h.IndexOf("Статус ТК", StringComparison.OrdinalIgnoreCase) >= 0 || h.IndexOf("Статус проверки", StringComparison.OrdinalIgnoreCase) >= 0);

            int linkIndex = 2;               // "Ссылка" всегда колонка 2
            int testCountIndex = 4;          // "Количество тестов" всегда колонка 4
            int fifthIndex = header.Count > 4 ? 4 : -1;
            int reviewIndex = 6; // "Проведено ревью тест-кейсов" — всегда колонка 7
            int typeIndex = 1;
            if (statusIndex == -1 || testStatusIndex == -1)
            {
                header = data[1].Select(c => c.ToString().Trim()).ToList();
                statusIndex = header.FindIndex(h => h.IndexOf("Статус задачи", StringComparison.OrdinalIgnoreCase) >= 0);
                testStatusIndex = header.FindIndex(h =>
                    h.IndexOf("Статус теста", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.IndexOf("Статус ТК", StringComparison.OrdinalIgnoreCase) >= 0 || h.IndexOf("Статус проверки", StringComparison.OrdinalIgnoreCase) >= 0);
                if (statusIndex == -1 || testStatusIndex == -1)
                    throw new Exception("Не удалось определить колонки 'Статус задачи' или 'Статус теста/ТК/проверки'.");
            }
            var stats = new SheetStats();

            for (int i = 1; i < data.Count; i++) // Skip(1) эквивалентно началу с i=1
            {
                var row = data[i];
                string status = row.ElementAtOrDefault(statusIndex)?.ToString()?.Trim() ?? "";
                string testStatus = row.ElementAtOrDefault(testStatusIndex)?.ToString()?.Trim() ?? "";
                string taskId = row.ElementAtOrDefault(linkIndex)?.ToString()?.Trim() ?? "";
                string fifthCol = row.ElementAtOrDefault(fifthIndex)?.ToString()?.Trim() ?? "";
                double testCount = double.TryParse(row.ElementAtOrDefault(testCountIndex)?.ToString(), out var val) ? val : 0;
                string reviewStatus = row.ElementAtOrDefault(reviewIndex)?.ToString()?.Trim() ?? "";
                if (reviewStatus == "Активный" || reviewStatus == "Требует обновления" || reviewStatus == "Устарело")
                    stats.ReviewedTestCases++;

                try
                {
                    if (int.TryParse(row[0]?.ToString(), out int firstCol))
                        stats.TotalTasks = Math.Max(stats.TotalTasks, firstCol);
                }
                catch (Exception) { continue; }

                if (status == "Waiting for Release" || status == "Documentation" || status == "Closed" || status == "Close") {
                    if (!string.IsNullOrWhiteSpace(taskId) && testCount == 0)
                    {
                        string nextType = data.ElementAtOrDefault(i + 1)?
                                              .ElementAtOrDefault(typeIndex)
                                              ?.ToString()
                                              ?.Trim() ?? "";

                        if (!nextType.Equals("Test case", StringComparison.OrdinalIgnoreCase))
                        {
                            stats.UntestedTaskLinks.Add($"https://jira.mos.social/browse/{taskId} передана закрытой");
                        }
                        else
                        {
                            stats.ClosedTasks++;
                        }
                    }
                    else
                    {
                        stats.ClosedTasks++;
                    }
                }

                if (status == "Reopened" || status == "Reopen")
                    stats.ReopenedTasks++;

                if (status == "Resolved" && !string.IsNullOrWhiteSpace(taskId) && testCount == 0)
                    stats.UntestedTaskLinks.Add($"https://jira.mos.social/browse/{taskId} тех задача");

                switch (testStatus)
                {
                    case "Pass":
                    case "Passed":
                        stats.Passed += (int)testCount;
                        break;
                    case "Pass with bug":
                        stats.PassedWithBug += (int)testCount;
                        break;
                    case "Failed":
                    case "Fail":
                        stats.Failed += (int)testCount;
                        break;
                    case "Blocked":
                        stats.Blocked += (int)testCount;
                        break;
                }

                if ((int)testCount > 1) { continue; }
                stats.PlannedTestCases += (int)testCount;
            }

            return stats;
        }

        private SheetStats AnalyzeGridData(DataGridView grid)
        {
            var stats = new SheetStats();

            const int numberIndex = 0;       // №
            const int typeIndex = 1;         // Тип
            const int linkIndex = 2;         // ТК
            const int testCountIndex = 4;    // Кол-во тестов
            const int reviewIndex = 6;       // Ревью ТК
            const int testStatusIndex = 7;   // Статус ТК
            const int statusIndex = 10;      // Статус задачи

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string status =
                    row.Cells[statusIndex]?.Value?.ToString()?.Trim() ?? "";

                string testStatus =
                    row.Cells[testStatusIndex]?.Value?.ToString()?.Trim() ?? "";

                string taskId =
                    row.Cells[linkIndex]?.Value?.ToString()?.Trim() ?? "";

                string reviewStatus =
                    row.Cells[reviewIndex]?.Value?.ToString()?.Trim() ?? "";

                double testCount =
                    double.TryParse(
                        row.Cells[testCountIndex]?.Value?.ToString(),
                        out var val)
                        ? val
                        : 0;

                // Ревью
                if (reviewStatus == "Активный"
                    || reviewStatus == "Требует обновления"
                    || reviewStatus == "Устарело" || reviewStatus == "Active")
                {
                    stats.ReviewedTestCases++;
                }

                // Общее количество задач
                if (int.TryParse(
                    row.Cells[numberIndex]?.Value?.ToString(),
                    out int num))
                {
                    stats.TotalTasks = Math.Max(stats.TotalTasks, num);
                }

                // Закрытые
                if (status == "Waiting for Release"
                    || status == "Documentation"
                    || status == "Closed"
                    || status == "Close")
                {
                    if (!string.IsNullOrWhiteSpace(taskId) && testCount == 0)
                    {
                        string nextType = "";

                        if (row.Index + 1 < grid.Rows.Count)
                        {
                            nextType =
                                grid.Rows[row.Index + 1]
                                    .Cells[typeIndex]
                                    ?.Value
                                    ?.ToString()
                                    ?.Trim() ?? "";
                        }

                        if (!nextType.Equals(
                            "Test case",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            stats.UntestedTaskLinks.Add(
                                $"https://jira.mos.social/browse/{taskId} передана закрытой");
                        }
                        else
                        {
                            stats.ClosedTasks++;
                        }
                    }
                    else
                    {
                        stats.ClosedTasks++;
                    }
                }

                // Переоткрытые
                if (status == "Reopened"
                    || status == "Reopen")
                {
                    stats.ReopenedTasks++;
                }

                // Тех задачи
                if (status == "Resolved"
                    && !string.IsNullOrWhiteSpace(taskId)
                    && testCount == 0)
                {
                    stats.UntestedTaskLinks.Add(
                        $"https://jira.mos.social/browse/{taskId} тех задача");
                }

                // Статистика тестов
                switch (testStatus)
                {
                    case "Pass":
                    case "Passed":
                        stats.Passed += (int)testCount;
                        break;

                    case "Pass with bug":
                        stats.PassedWithBug += (int)testCount;
                        break;

                    case "Fail":
                    case "Failed":
                        stats.Failed += (int)testCount;
                        break;

                    case "Blocked":
                        stats.Blocked += (int)testCount;
                        break;
                }

                // Плановые ТК
                if ((int)testCount <= 1)
                {
                    stats.PlannedTestCases += (int)testCount;
                }
            }

            return stats;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void materialCheckbox1_CheckedChanged(object sender, EventArgs e)
        {
            if (materialCheckbox1.Checked)
            {
                materialLabel3.Text = "Цикл";
            }
            else
            {
                materialLabel3.Text = "Задача";
            }
        }

        
        public async void GenerateAndShowYandexData(List<List<object>> rows, CurlRequest curl, string stylesJson)
        {

            int bundleId = curl.BundleId + 1;
            int sheetId = curl.SheetId;
            int timeline = curl.Timeline;
            var centerCols = new HashSet<int> { 1, 2, 4, 5, 6, 7, 10 };
            var bundle = new List<object>();
            var header = new List<object>
            {
                "№",
                "Тип",
                "ТК",
                "Название",
                "Кол-во тестов",
                "Кол-во шагов",
                "Ревью ТК",
                "Статус ТК",
                "Тестировщик",
                "Комментарий",
                "Статус задачи"
            };
            var columns = new[]
            {
                new { Index = 1, Width = 14.97265625 },
                new { Index = 2, Width = 15.78125 },
                new { Index = 3, Width = 53.1 },
                new { Index = 4, Width = 14.703125 },
                new { Index = 5, Width = 14.1640625 },
                new { Index = 8, Width = 19.15234375 },
                new { Index = 9, Width = 20.23046875 },
                new { Index = 10, Width = 16.86328125 }
            };

            foreach (var col in columns)
            {
                bundle.Add(new
                {
                    t = "ucolp",
                    path = new object[] { sheetId, -1, col.Index, -1, col.Index },
                    props = new object[]
                    {
                        new object[] { "width", col.Width },
                        new object[] { "customWidth", "1" }
                    },
                    uPh = (object)null
                });
            }
            bundle.Add(new
            {
                t = "urowp",
                path = new object[] { sheetId, 0, -1, 0 },
                props = new object[]
                    {
                        new object[] { "ht", 30 },
                        new object[] { "customHeight", "1" }
                    },
                uPh = (object)null
            });
            // вставляем в начало
            //rows.Insert(0, header);
            // =========================
            // 🔥 СТИЛИ
            // =========================
            var styleResult = YandexStyleResolver.Resolve(stylesJson);

            bundle.AddRange(styleResult.Bundle);
            
            int wrapStyle = styleResult.WrapXfId;
            int defaultStyle = styleResult.DefaultStyleId;
            int headerStyle = styleResult.HeaderStyleId;
            int linkStyle = styleResult.LinkStyleId;

            int dvIndex = 0;
            int lastRow = rows.Count;

            string rangeB = $"B2:B{lastRow}";
            string rangeG = $"G2:G{lastRow}";
            string rangeH = $"H2:H{lastRow}";
            string rangeK = $"K2:K{lastRow}";

            // =========================
            // 🔗 hyperlinks init
            // =========================
            bundle.Add(new
            {
                t = "ue",
                path = new object[] { "worksheet", sheetId },
                prop = "hyperlinks",
                v = new { t_ = "hyperlinks", hyperlink = new object[] { } },
                uPh = (object)null
            });

            int hyperlinkIndex = 0;
            var allRows = new List<object>();

            for (int i = 0; i < rows.Count; i++)
            {
                int rowNumber = i + 1;
                var row = rows[i];
                var cells = new List<object>();

                for (int j = 0; j < row.Count; j++)
                {
                    string value = row[j]?.ToString() ?? "";
                    string col = GetExcelColumnName(j + 1);
                    string cellRef = col + rowNumber;

                    if (j == 2 && i != 0)
                    {
                        string url = $"https://jira.mos.social/browse/{value}";

                        bundle.Add(new
                        {
                            t = "ie",
                            path = new object[] { "worksheet", sheetId, "hyperlinks", "hyperlink", hyperlinkIndex++ },
                            content = new
                            {
                                t_ = "hyperlink",
                                @ref = cellRef,
                                v_ = value,
                                target = url
                            }
                        });

                        cells.Add(new
                        {
                            t_ = "c",
                            r = cellRef,
                            s = linkStyle,
                            v = new
                            {
                                content = value,
                                v_ = value
                            }
                        });
                    }
                    else
                    {
                        int styleToUse;

                        if (i == 0)
                        {
                            // header
                            styleToUse = headerStyle;
                        }
                        else if (j == 3 || j == 9)
                        {
                            styleToUse = wrapStyle;
                        }
                        else if (centerCols.Contains(j))
                        {
                            // центрирование
                            styleToUse = styleResult.CenterXfId;
                        }
                        else
                        {
                            // обычный
                            styleToUse = defaultStyle;
                        }

                        cells.Add(new
                        {
                            t_ = "c",
                            r = cellRef,
                            s = styleToUse,
                            v = new { content = value, v_ = value }
                        });
                    }
                }

                allRows.Add(new
                {
                    t_ = "row",
                    r = rowNumber,
                    c = cells
                });
            }
            // =====================
            // Тип (B)
            // =====================
            bundle.Add(new
            {
                t = "ie",
                path = new object[] { "worksheet", sheetId, "dataValidations", "dataValidation", dvIndex++ },
                content = new
                {
                    t_ = "dataValidation",
                    sqref = new[] { rangeB },
                    type = "list",
                    formula1 = new { t_ = "formula1", v_ = "\"Bug,Improvement,Test case,Task,New Feature,Sub-task\"" },
                    viewType = "arrow",
                    colors = new object[]
                    {
                        new { t_ = "colors", key = "Bug", color = "#B10202" },
                        new { t_ = "colors", key = "Improvement", color = "#11734B" },
                        new { t_ = "colors", key = "Task", color = "#C6DBE1" },
                        new { t_ = "colors", key = "Test case", color = "#BFE1F6" },
                        new { t_ = "colors", key = "New Feature", color = "#E6CFF2" },
                        new { t_ = "colors", key = "Sub-task", color = "#C6DBE1" },
                        new { t_ = "colors", key = "Story", color = "#E8EAED" }
                    }
                }
            });

            // =====================
            // Ревью ТК (G)
            // =====================
            bundle.Add(new
            {
                t = "ie",
                path = new object[] { "worksheet", sheetId, "dataValidations", "dataValidation", dvIndex++ },
                content = new
                {
                    t_ = "dataValidation",
                    sqref = new[] { rangeG },
                    type = "list",
                    formula1 = new { t_ = "formula1", v_ = "\"На ревью,Активный,Требует обновления,Устаревший\"" },
                    viewType = "arrow",
                    colors = new object[]
                    {
                        new { t_ = "colors", key = "На ревью", color = "#8EABDB" },
                        new { t_ = "colors", key = "Активный", color = "#A9D08D" },
                        new { t_ = "colors", key = "Требует обновления", color = "#F4AF81" },
                        new { t_ = "colors", key = "Устаревший", color = "#F78B8B" }
                    }
                }
            });

            // =====================
            // Статус ТК (H)
            // =====================
            bundle.Add(new
            {
                t = "ie",
                path = new object[] { "worksheet", sheetId, "dataValidations", "dataValidation", dvIndex++ },
                content = new
                {
                    t_ = "dataValidation",
                    sqref = new[] { rangeH },
                    type = "list",
                    formula1 = new { t_ = "formula1", v_ = "\"В работе,Pass,Pass with bug,Fail,Вопрос,Blocked,N/A\"" },
                    viewType = "arrow",
                    colors = new object[]
                    {
                        new { t_ = "colors", key = "В работе", color = "#FBE4D4" },
                        new { t_ = "colors", key = "Pass", color = "#A9D08D" },
                        new { t_ = "colors", key = "Pass with bug", color = "#449DEB" },
                        new { t_ = "colors", key = "Fail", color = "#FFC7CE" },
                        new { t_ = "colors", key = "Вопрос", color = "#DB93D9" },
                        new { t_ = "colors", key = "Blocked", color = "#3F3F3F" },
                        new { t_ = "colors", key = "N/A", color = "#FEF2CB" }
                    }
                }
            });

            // =====================
            // Статус задачи (K)
            // =====================
            bundle.Add(new
            {
                t = "ie",
                path = new object[] { "worksheet", sheetId, "dataValidations", "dataValidation", dvIndex++ },
                content = new
                {
                    t_ = "dataValidation",
                    sqref = new[] { rangeK },
                    type = "list",
                    formula1 = new { t_ = "formula1", v_ = "\"Resolved,Closed,Reopened,Waiting for Release,Documentation\"" },
                    viewType = "arrow",
                    colors = new object[]
                    {
                        new { t_ = "colors", key = "Documentation", color = "#8EABDB" },
                        new { t_ = "colors", key = "Waiting for Release", color = "#A9D08D" },
                        new { t_ = "colors", key = "Reopened", color = "#C00000" },
                        new { t_ = "colors", key = "Closed", color = "#558035" },
                        new { t_ = "colors", key = "Resolved", color = "#E2EFD8" }
                    }
                }
            });
            bundle.Add(new
            {
                t = "uc",
                path = new object[] { sheetId, 0, 0, rows.Count - 1, rows[0].Count - 1 },
                rows = allRows,
                mergedCellsToInsert = new object[] { },
                mergedCellsToDelete = new object[] { }
            });

            var data = new
            {
                bundleId = bundleId,
                bundle = bundle,
                timeline = timeline
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            try
            {

                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };

                using var client = new HttpClient(handler);

                var request = new HttpRequestMessage(HttpMethod.Post, curl.Url);

                // cookies
                handler.CookieContainer.SetCookies(new Uri(curl.Url), curl.Cookies);

                // headers
                foreach (var h in curl.Headers)
                {
                    if (h.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (h.Key.StartsWith("sec-", StringComparison.OrdinalIgnoreCase))
                        continue;

                    request.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                // body
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);


                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    SaveCurlToFile(request, json, resp);
                    showError.ShowErr(this, "Ошибка отправки", resp, response.StatusCode);
                }
                else
                {
                    MaterialMessageBox.Show(this, "Данные успешно выгружены в ЯД. Обновите страницу, что бы увидеть результат.\n" +
                        "Если есть проблемы с выгрузкой или предложения по улучшению, свяжитесь по Telegram: https://t.me/DI_KEN9", "ЯД выгружено",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SaveCurlToFile(request, json, null);
                }

            }
            catch (Exception ex)
            {
                showError.ShowErr(this, "Ошибка запроса", ex.Message);
            }
        }

        private void SaveCurlToFile(HttpRequestMessage request, string body, string? resp)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"curl '{request.RequestUri}' \\");
            sb.AppendLine($"  -X {request.Method} \\");

            // Headers
            foreach (var header in request.Headers)
            {
                foreach (var value in header.Value)
                {
                    sb.AppendLine($"  -H \"{header.Key}: {value}\" \\");
                }
            }

            // Content headers (например Content-Type)
            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        sb.AppendLine($"  -H \"{header.Key}: {value}\" \\");
                    }
                }
            }

            // Body
            if (!string.IsNullOrEmpty(body))
            {
                // экранируем кавычки
                var safeBody = body;
                sb.AppendLine($"  --data-raw \"{safeBody}\"");
            }

            File.WriteAllText("last_curl.txt", sb.ToString(), Encoding.UTF8);
            if (resp != null)
            {
                File.WriteAllText("last_response.txt", resp);
            }
        }


        private string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0) 
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)(65 + remainder) + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnName;
        }

        private void дляТМToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide(); // скрываем главное окно
            using (AdminForm adminForm = new AdminForm(login, password))
            {
                ThemeManager.ApplyThemeToForm(adminForm);
                adminForm.ShowDialog(this); // модальное окно с указанием владельца
            }
            this.Show(); // после закрытия диалога показываем главное окно
        }

        private void dataGridViewPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteFromClipboard();
            }
        }
        private void PasteFromClipboard()
        {
            string clipboard = Clipboard.GetText();

            if (string.IsNullOrWhiteSpace(clipboard))
                return;

            string[] rows = clipboard.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            int startRow = 0;
            int startCol = 0;

            // Сначала добавляем все строки
            int needRows = startRow + rows.Length;

            while (dataGridViewPreview.Rows.Count < needRows)
            {
                dataGridViewPreview.Rows.Add();
            }

            // Потом заполняем
            for (int i = 0; i < rows.Length; i++)
            {
                string[] cells = rows[i].Split('\t');

                DataGridViewRow row =
                    dataGridViewPreview.Rows[startRow + i];

                for (int j = 0; j < cells.Length; j++)
                {
                    if (startCol + j >= dataGridViewPreview.Columns.Count)
                        break;

                    row.Cells[startCol + j].Value = cells[j];
                }
            }

            // Принудительно обновляем отображение
            dataGridViewPreview.EndEdit();
            dataGridViewPreview.Refresh();
        }

        private void checkBoxPreview_CheckedChanged_1(object sender, EventArgs e)
        {
            if (!toolStripMenuItem2.Enabled)
            {
                if (!checkBoxPreview.Checked)
                {
                    materialButton1.Enabled = true;
                    textBox1.Visible = true;
                    textBox1.Enabled = true;
                    dataGridViewPreview.Visible = false;
                }
                else
                {
                    materialButton1.Enabled = true;
                    textBox1.Visible = false;
                    textBox1.Enabled = false;
                    dataGridViewPreview.Visible = true;
                }
            }
            else 
            {
                if (checkBoxPreview.Checked)
                {
                    materialButton1.Enabled = true;
                    dataGridViewPreview.AllowUserToAddRows = true;
                    dataGridViewPreview.AllowUserToDeleteRows = true;
                    dataGridViewPreview.Enabled = true;
                }
                else
                {
                    materialButton1.Enabled = false;
                    dataGridViewPreview.AllowUserToAddRows = false;
                    dataGridViewPreview.AllowUserToDeleteRows = false;
                    dataGridViewPreview.Enabled = false;
                }
            }
        }

        private void textBoxSheetUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true; // отключаем стандартную вставку

                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    string singleLine = ConvertCurlToSingleLine(clipboardText);
                    InsertText(sender as TextBoxBase, singleLine);
                }
            }
        }
        private string ConvertCurlToSingleLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Разбиваем на строки
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                // Убираем пробелы и табы в конце строки
                lines[i] = lines[i].TrimEnd();
                // Если строка заканчивается на обратный слеш – удаляем его
                if (lines[i].EndsWith("\\"))
                    lines[i] = lines[i].Substring(0, lines[i].Length - 1);
            }

            // Собираем обратно, склеивая через пробел
            return string.Join(" ", lines);
        }
        private void InsertText(TextBoxBase textBox, string text)
        {
            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            // Удаляем выделенный текст (если есть)
            textBox.Text = textBox.Text.Remove(start, length);
            // Вставляем новый текст
            textBox.Text = textBox.Text.Insert(start, text);
            // Устанавливаем курсор после вставленного текста
            textBox.SelectionStart = start + text.Length;
            textBox.SelectionLength = 0;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Загружаем сохранённую тему
            string savedTheme = Properties.Settings.Default.Theme;
            bool isDark = (savedTheme == "Dark");

            // Устанавливаем переключатель (если есть)
            if (themeSwitch != null)
            {
                themeSwitch.Checked = isDark;
                // Но событие CheckedChanged вызовется, нужно отключить его временно
                themeSwitch.CheckedChanged -= themeSwitch_CheckedChanged; // отписываем
                themeSwitch.Checked = isDark;
                themeSwitch.CheckedChanged += themeSwitch_CheckedChanged;
            }
            ThemeMode mode = themeSwitch.Checked ? ThemeMode.Dark : ThemeMode.Light;
            // Применяем тему к самой форме и всем контролам
            ThemeManager.SetTheme(isDark ? ThemeMode.Dark : ThemeMode.Light);
            if (menuStrip != null)
                ThemeManager.ApplyThemeToMenuStrip(menuStrip, mode);
        }

        private void themeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            ThemeMode mode = themeSwitch.Checked ? ThemeMode.Dark : ThemeMode.Light;
            ThemeManager.SetTheme(mode);

            if (menuStrip != null)
                ThemeManager.ApplyThemeToMenuStrip(menuStrip, mode);

            Properties.Settings.Default.Theme = mode.ToString();
            Properties.Settings.Default.Save();
        }
    }
    public class SheetStats
    {
        public int TotalTasks { get; set; }
        public int ClosedTasks { get; set; }
        public int ReopenedTasks { get; set; }
        public List<string> UntestedTaskLinks { get; set; } = new List<string>();
        public int PlannedTestCases { get; set; }
        public int Passed { get; set; }
        public int PassedWithBug { get; set; }
        public int ReviewedTestCases { get; set; }
        public int Failed { get; set; }
        public int Blocked { get; set; }
    }
}
