
namespace UI_Testing
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.оценкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.дляТМToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.textBoxSheetUrl = new MaterialSkin.Controls.MaterialTextBox();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.textBoxStyleSheetUrl = new MaterialSkin.Controls.MaterialTextBox();
            this.textBoxVersion = new MaterialSkin.Controls.MaterialTextBox();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.dataGridViewPreview = new System.Windows.Forms.DataGridView();
            this.materialButton1 = new MaterialSkin.Controls.MaterialButton();
            this.materialButton2 = new MaterialSkin.Controls.MaterialButton();
            this.blocksTC = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox = new MaterialSkin.Controls.MaterialCheckbox();
            this.checkBoxIteration = new MaterialSkin.Controls.MaterialCheckbox();
            this.materialCheckbox1 = new MaterialSkin.Controls.MaterialCheckbox();
            this.checkBoxPreview = new MaterialSkin.Controls.MaterialCheckbox();
            this.checkBoxNoStyle = new MaterialSkin.Controls.MaterialCheckbox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.materialLabel4 = new MaterialSkin.Controls.MaterialLabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPreview)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5,
            this.toolStripMenuItem2,
            this.toolStripMenuItem1,
            this.toolStripMenuItem3,
            this.оценкаToolStripMenuItem,
            this.toolStripMenuItem4,
            this.дляТМToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(4, 79);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1193, 36);
            this.menuStrip.TabIndex = 10;
            this.menuStrip.Text = "menuStrip";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(52, 32);
            this.toolStripMenuItem5.Text = "ЯД";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(78, 32);
            this.toolStripMenuItem2.Text = "Отчет";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(95, 32);
            this.toolStripMenuItem1.Text = "ГД (old)";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(83, 32);
            this.toolStripMenuItem3.Text = "Поиск";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // оценкаToolStripMenuItem
            // 
            this.оценкаToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.оценкаToolStripMenuItem.Name = "оценкаToolStripMenuItem";
            this.оценкаToolStripMenuItem.Size = new System.Drawing.Size(95, 32);
            this.оценкаToolStripMenuItem.Text = "Оценка";
            this.оценкаToolStripMenuItem.Click += new System.EventHandler(this.оценкаToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(151, 32);
            this.toolStripMenuItem4.Text = "Верификация";
            this.toolStripMenuItem4.Visible = false;
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // дляТМToolStripMenuItem
            // 
            this.дляТМToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.дляТМToolStripMenuItem.Name = "дляТМToolStripMenuItem";
            this.дляТМToolStripMenuItem.Size = new System.Drawing.Size(94, 32);
            this.дляТМToolStripMenuItem.Text = "Для ТМ";
            this.дляТМToolStripMenuItem.Click += new System.EventHandler(this.дляТМToolStripMenuItem_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Enabled = false;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(889, 323);
            this.textBox1.TabIndex = 24;
            this.textBox1.Visible = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.83532F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.16468F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 198F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.materialLabel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxSheetUrl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.materialLabel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxStyleSheetUrl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBoxVersion, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.materialLabel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewPreview, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxPreview, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxNoStyle, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxIteration, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.materialCheckbox1, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.materialCheckbox, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.blocksTC, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.materialButton2, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.materialButton1, 3, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 118);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1189, 656);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // materialLabel1
            // 
            this.materialLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(4, 41);
            this.materialLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(105, 19);
            this.materialLabel1.TabIndex = 11;
            this.materialLabel1.Text = "Ссылка на ГД";
            this.materialLabel1.Visible = false;
            // 
            // textBoxSheetUrl
            // 
            this.textBoxSheetUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSheetUrl.AnimateReadOnly = false;
            this.textBoxSheetUrl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxSheetUrl.Depth = 0;
            this.textBoxSheetUrl.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxSheetUrl.LeadingIcon = null;
            this.textBoxSheetUrl.Location = new System.Drawing.Point(4, 64);
            this.textBoxSheetUrl.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSheetUrl.MaxLength = 500;
            this.textBoxSheetUrl.MouseState = MaterialSkin.MouseState.OUT;
            this.textBoxSheetUrl.Multiline = false;
            this.textBoxSheetUrl.Name = "textBoxSheetUrl";
            this.textBoxSheetUrl.Size = new System.Drawing.Size(420, 50);
            this.textBoxSheetUrl.TabIndex = 0;
            this.textBoxSheetUrl.Text = "";
            this.textBoxSheetUrl.TrailingIcon = null;
            this.textBoxSheetUrl.Visible = false;
            // 
            // materialLabel3
            // 
            this.materialLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel3.Location = new System.Drawing.Point(4, 156);
            this.materialLabel3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(420, 19);
            this.materialLabel3.TabIndex = 13;
            this.materialLabel3.Text = "Скоуп";
            // 
            // textBoxStyleSheetUrl
            // 
            this.textBoxStyleSheetUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStyleSheetUrl.AnimateReadOnly = false;
            this.textBoxStyleSheetUrl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxStyleSheetUrl.Depth = 0;
            this.textBoxStyleSheetUrl.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxStyleSheetUrl.LeadingIcon = null;
            this.textBoxStyleSheetUrl.Location = new System.Drawing.Point(432, 64);
            this.textBoxStyleSheetUrl.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxStyleSheetUrl.MaxLength = 200;
            this.textBoxStyleSheetUrl.MouseState = MaterialSkin.MouseState.OUT;
            this.textBoxStyleSheetUrl.Multiline = false;
            this.textBoxStyleSheetUrl.Name = "textBoxStyleSheetUrl";
            this.textBoxStyleSheetUrl.Size = new System.Drawing.Size(405, 50);
            this.textBoxStyleSheetUrl.TabIndex = 1;
            this.textBoxStyleSheetUrl.Text = "";
            this.textBoxStyleSheetUrl.TrailingIcon = null;
            this.textBoxStyleSheetUrl.Visible = false;
            // 
            // textBoxVersion
            // 
            this.textBoxVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxVersion.AnimateReadOnly = false;
            this.textBoxVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxVersion.Depth = 0;
            this.textBoxVersion.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textBoxVersion.LeadingIcon = null;
            this.textBoxVersion.Location = new System.Drawing.Point(4, 179);
            this.textBoxVersion.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxVersion.MaxLength = 999999;
            this.textBoxVersion.MouseState = MaterialSkin.MouseState.OUT;
            this.textBoxVersion.Multiline = false;
            this.textBoxVersion.Name = "textBoxVersion";
            this.textBoxVersion.Size = new System.Drawing.Size(420, 50);
            this.textBoxVersion.TabIndex = 2;
            this.textBoxVersion.Text = "";
            this.textBoxVersion.TrailingIcon = null;
            // 
            // materialLabel2
            // 
            this.materialLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel2.Location = new System.Drawing.Point(432, 41);
            this.materialLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(194, 19);
            this.materialLabel2.TabIndex = 12;
            this.materialLabel2.Text = "Ссылка на ГД со стилями";
            this.materialLabel2.Visible = false;
            // 
            // dataGridViewPreview
            // 
            this.dataGridViewPreview.AllowUserToOrderColumns = true;
            this.dataGridViewPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.dataGridViewPreview, 4);
            this.dataGridViewPreview.Location = new System.Drawing.Point(4, 239);
            this.dataGridViewPreview.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridViewPreview.Name = "dataGridViewPreview";
            this.dataGridViewPreview.RowHeadersWidth = 51;
            this.dataGridViewPreview.Size = new System.Drawing.Size(1181, 413);
            this.dataGridViewPreview.TabIndex = 8;
            // 
            // materialButton1
            // 
            this.materialButton1.AutoSize = false;
            this.materialButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton1.Depth = 0;
            this.materialButton1.HighEmphasis = true;
            this.materialButton1.Icon = null;
            this.materialButton1.Location = new System.Drawing.Point(995, 7);
            this.materialButton1.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton1.Name = "materialButton1";
            this.materialButton1.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton1.Size = new System.Drawing.Size(185, 44);
            this.materialButton1.TabIndex = 7;
            this.materialButton1.Text = "Предпросмотр";
            this.materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton1.UseAccentColor = false;
            this.materialButton1.UseVisualStyleBackColor = true;
            this.materialButton1.Click += new System.EventHandler(this.materialButton1_Click);
            // 
            // materialButton2
            // 
            this.materialButton2.AutoSize = false;
            this.materialButton2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton2.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.materialButton2.Depth = 0;
            this.materialButton2.HighEmphasis = true;
            this.materialButton2.Icon = null;
            this.materialButton2.Location = new System.Drawing.Point(995, 67);
            this.materialButton2.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.materialButton2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton2.Name = "materialButton2";
            this.materialButton2.NoAccentTextColor = System.Drawing.Color.Empty;
            this.materialButton2.Size = new System.Drawing.Size(185, 41);
            this.materialButton2.TabIndex = 15;
            this.materialButton2.Text = "Выгрузка ЯД";
            this.materialButton2.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton2.UseAccentColor = false;
            this.materialButton2.UseVisualStyleBackColor = true;
            this.materialButton2.Click += new System.EventHandler(this.materialButton2_Click);
            // 
            // blocksTC
            // 
            this.blocksTC.AutoSize = true;
            this.blocksTC.Depth = 0;
            this.blocksTC.Location = new System.Drawing.Point(990, 120);
            this.blocksTC.Margin = new System.Windows.Forms.Padding(0);
            this.blocksTC.MouseLocation = new System.Drawing.Point(-1, -1);
            this.blocksTC.MouseState = MaterialSkin.MouseState.HOVER;
            this.blocksTC.Name = "blocksTC";
            this.blocksTC.ReadOnly = false;
            this.blocksTC.Ripple = true;
            this.blocksTC.Size = new System.Drawing.Size(105, 37);
            this.blocksTC.TabIndex = 17;
            this.blocksTC.Text = "blocks ТК";
            this.blocksTC.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox
            // 
            this.materialCheckbox.AutoSize = true;
            this.materialCheckbox.Depth = 0;
            this.materialCheckbox.Location = new System.Drawing.Point(990, 175);
            this.materialCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox.Name = "materialCheckbox";
            this.materialCheckbox.ReadOnly = false;
            this.materialCheckbox.Ripple = true;
            this.materialCheckbox.Size = new System.Drawing.Size(148, 37);
            this.materialCheckbox.TabIndex = 9;
            this.materialCheckbox.Text = "С приоритетом";
            this.materialCheckbox.UseVisualStyleBackColor = true;
            this.materialCheckbox.Visible = false;
            // 
            // checkBoxIteration
            // 
            this.checkBoxIteration.AutoSize = true;
            this.checkBoxIteration.Depth = 0;
            this.checkBoxIteration.Location = new System.Drawing.Point(841, 120);
            this.checkBoxIteration.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxIteration.MouseLocation = new System.Drawing.Point(-1, -1);
            this.checkBoxIteration.MouseState = MaterialSkin.MouseState.HOVER;
            this.checkBoxIteration.Name = "checkBoxIteration";
            this.checkBoxIteration.ReadOnly = false;
            this.checkBoxIteration.Ripple = true;
            this.checkBoxIteration.Size = new System.Drawing.Size(107, 37);
            this.checkBoxIteration.TabIndex = 4;
            this.checkBoxIteration.Text = "Итерация";
            this.checkBoxIteration.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox1
            // 
            this.materialCheckbox1.AutoSize = true;
            this.materialCheckbox1.Depth = 0;
            this.materialCheckbox1.Location = new System.Drawing.Point(841, 175);
            this.materialCheckbox1.Margin = new System.Windows.Forms.Padding(0);
            this.materialCheckbox1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialCheckbox1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCheckbox1.Name = "materialCheckbox1";
            this.materialCheckbox1.ReadOnly = false;
            this.materialCheckbox1.Ripple = true;
            this.materialCheckbox1.Size = new System.Drawing.Size(101, 37);
            this.materialCheckbox1.TabIndex = 16;
            this.materialCheckbox1.Text = "из цикла";
            this.materialCheckbox1.UseVisualStyleBackColor = true;
            // 
            // checkBoxPreview
            // 
            this.checkBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxPreview.AutoSize = true;
            this.checkBoxPreview.Checked = true;
            this.checkBoxPreview.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPreview.Depth = 0;
            this.checkBoxPreview.Location = new System.Drawing.Point(695, 120);
            this.checkBoxPreview.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxPreview.MouseLocation = new System.Drawing.Point(-1, -1);
            this.checkBoxPreview.MouseState = MaterialSkin.MouseState.HOVER;
            this.checkBoxPreview.Name = "checkBoxPreview";
            this.checkBoxPreview.ReadOnly = false;
            this.checkBoxPreview.Ripple = true;
            this.checkBoxPreview.Size = new System.Drawing.Size(146, 37);
            this.checkBoxPreview.TabIndex = 5;
            this.checkBoxPreview.Text = "Предпросмотр";
            this.checkBoxPreview.UseVisualStyleBackColor = true;
            // 
            // checkBoxNoStyle
            // 
            this.checkBoxNoStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxNoStyle.AutoSize = true;
            this.checkBoxNoStyle.Depth = 0;
            this.checkBoxNoStyle.Location = new System.Drawing.Point(733, 175);
            this.checkBoxNoStyle.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxNoStyle.MouseLocation = new System.Drawing.Point(-1, -1);
            this.checkBoxNoStyle.MouseState = MaterialSkin.MouseState.HOVER;
            this.checkBoxNoStyle.Name = "checkBoxNoStyle";
            this.checkBoxNoStyle.ReadOnly = false;
            this.checkBoxNoStyle.Ripple = true;
            this.checkBoxNoStyle.Size = new System.Drawing.Size(108, 37);
            this.checkBoxNoStyle.TabIndex = 3;
            this.checkBoxNoStyle.Text = "Без стиля";
            this.checkBoxNoStyle.UseVisualStyleBackColor = true;
            this.checkBoxNoStyle.Visible = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.materialLabel4, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(7, 118);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1190, 657);
            this.tableLayoutPanel2.TabIndex = 21;
            this.tableLayoutPanel2.Visible = false;
            // 
            // materialLabel4
            // 
            this.materialLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.materialLabel4.AutoSize = true;
            this.materialLabel4.Depth = 0;
            this.materialLabel4.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel4.Location = new System.Drawing.Point(3, 319);
            this.materialLabel4.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel4.Name = "materialLabel4";
            this.materialLabel4.Size = new System.Drawing.Size(1184, 19);
            this.materialLabel4.TabIndex = 14;
            this.materialLabel4.Text = "Спасибо что поинтересовались, функционал \"Поиск\" еще не разработан :(";
            this.materialLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 290);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1186, 484);
            this.tableLayoutPanel3.TabIndex = 25;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1201, 782);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1201, 782);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(4, 79, 4, 4);
            this.Text = "ЯД";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPreview)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem оценкаToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialTextBox textBoxSheetUrl;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialTextBox textBoxStyleSheetUrl;
        private MaterialSkin.Controls.MaterialTextBox textBoxVersion;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private System.Windows.Forms.DataGridView dataGridViewPreview;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        private MaterialSkin.Controls.MaterialButton materialButton2;
        private MaterialSkin.Controls.MaterialLabel materialLabel4;
        private MaterialSkin.Controls.MaterialCheckbox blocksTC;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox;
        private MaterialSkin.Controls.MaterialCheckbox checkBoxIteration;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox1;
        private MaterialSkin.Controls.MaterialCheckbox checkBoxPreview;
        private MaterialSkin.Controls.MaterialCheckbox checkBoxNoStyle;
        private System.Windows.Forms.ToolStripMenuItem дляТМToolStripMenuItem;
    }
}