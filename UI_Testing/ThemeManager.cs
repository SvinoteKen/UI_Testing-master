using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace UI_Testing
{
    public enum ThemeMode { Light, Dark }

    public static class ThemeManager
    {
        private static ThemeMode _currentMode = ThemeMode.Light;
        private static MaterialSkinManager _skinManager => MaterialSkinManager.Instance;

        public static ThemeMode CurrentMode => _currentMode;

        // ----- Глобальное применение ко всем формам -----
        public static void SetTheme(ThemeMode mode)
        {
            _currentMode = mode;
            UpdateMaterialSkin(mode);
            foreach (Form form in Application.OpenForms)
            {
                ApplyThemeToForm(form, mode);
            }
        }

        // ----- Применение темы к конкретной форме (рекурсивно все контролы) -----
        public static void ApplyThemeToForm(Form form, ThemeMode? mode = null)
        {
            if (mode == null) mode = _currentMode;
            UpdateMaterialSkin(mode.Value);
            ApplyThemeToControls(form, mode.Value);
            form.Refresh();
        }

        // ----- Точечное применение к одному контролу (и его детям) -----
        public static void ApplyThemeToControl(Control control, ThemeMode? mode = null)
        {
            if (mode == null) mode = _currentMode;
            ApplyThemeToControlInternal(control, mode.Value);
        }

        // ----- Внутренние методы -----

        private static void UpdateMaterialSkin(ThemeMode mode)
        {
            if (mode == ThemeMode.Dark)
            {
                _skinManager.Theme = MaterialSkinManager.Themes.DARK;
                _skinManager.ColorScheme = new ColorScheme(
                    Primary.Blue700, Primary.Blue800,
                    Primary.Blue300, Accent.LightBlue200,
                    TextShade.WHITE
                );
            }
            else
            {
                _skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                _skinManager.ColorScheme = new ColorScheme(
                    Primary.Blue600, Primary.Blue700,
                    Primary.Blue200, Accent.LightBlue200,
                    TextShade.WHITE
                );
            }
        }

        private static void ApplyThemeToControls(Control parent, ThemeMode mode)
        {
            foreach (Control ctrl in parent.Controls)
            {
                ApplyThemeToControlInternal(ctrl, mode);
            }
        }

        private static void ApplyThemeToControlInternal(Control ctrl, ThemeMode mode)
        {
            // --- Пропускаем MaterialSkin-контролы (они управляются MaterialSkinManager) ---
            if (ctrl is MaterialButton ||
                ctrl is MaterialTextBox ||
                ctrl is MaterialSwitch ||
                ctrl is MaterialLabel ||
                ctrl is MaterialCheckbox ||
                ctrl is MaterialComboBox ||
                ctrl is MaterialListView)
            {
                // Рекурсивно обрабатываем вложенные контролы (если есть)
                foreach (Control child in ctrl.Controls)
                    ApplyThemeToControlInternal(child, mode);
                return;
            }

            // --- Применяем стили для стандартных контролов ---
            if (mode == ThemeMode.Dark)
            {
                ctrl.BackColor = Color.FromArgb(45, 45, 48);
                ctrl.ForeColor = Color.FromArgb(241, 241, 241);

                if (ctrl is TextBox txt)
                {
                    txt.BackColor = Color.FromArgb(30, 30, 30);
                    txt.ForeColor = Color.WhiteSmoke;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is DataGridView dgv)
                {
                    ApplyDarkThemeToDataGridView(dgv);
                }
                else if (ctrl is ListBox lb)
                {
                    lb.BackColor = Color.FromArgb(30, 30, 30);
                    lb.ForeColor = Color.WhiteSmoke;
                }
                else if (ctrl is ComboBox cb)
                {
                    cb.BackColor = Color.FromArgb(30, 30, 30);
                    cb.ForeColor = Color.WhiteSmoke;
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is DateTimePicker dtp)
                {
                    dtp.BackColor = Color.FromArgb(30, 30, 30);
                    dtp.ForeColor = Color.WhiteSmoke;
                    dtp.CalendarMonthBackground = Color.FromArgb(30, 30, 30);
                    dtp.CalendarForeColor = Color.WhiteSmoke;
                    dtp.CalendarTitleBackColor = Color.FromArgb(60, 60, 60);
                    dtp.CalendarTitleForeColor = Color.WhiteSmoke;
                    dtp.CalendarTrailingForeColor = Color.DimGray;
                    dtp.CalendarFont = new Font(dtp.Font, FontStyle.Regular);
                }
                else if (ctrl is TabControl tab)
                {
                    tab.BackColor = Color.FromArgb(45, 45, 48);
                    tab.ForeColor = Color.WhiteSmoke;
                    foreach (TabPage page in tab.TabPages)
                    {
                        page.BackColor = Color.FromArgb(45, 45, 48);
                        page.ForeColor = Color.WhiteSmoke;
                    }
                }
                else if (ctrl is GroupBox gb)
                {
                    gb.BackColor = Color.FromArgb(45, 45, 48);
                    gb.ForeColor = Color.WhiteSmoke;
                }
                // Можно добавить другие типы (Button, Label, Panel и т.д.)
            }
            else // Light
            {
                ctrl.BackColor = SystemColors.Control;
                ctrl.ForeColor = SystemColors.ControlText;

                if (ctrl is TextBox txt)
                {
                    txt.BackColor = SystemColors.Window;
                    txt.ForeColor = SystemColors.WindowText;
                    txt.BorderStyle = BorderStyle.Fixed3D;
                }
                else if (ctrl is DataGridView dgv)
                {
                    ApplyLightThemeToDataGridView(dgv);
                }
                else if (ctrl is ListBox lb)
                {
                    lb.BackColor = SystemColors.Window;
                    lb.ForeColor = SystemColors.WindowText;
                }
                else if (ctrl is ComboBox cb)
                {
                    cb.BackColor = SystemColors.Window;
                    cb.ForeColor = SystemColors.WindowText;
                    cb.FlatStyle = FlatStyle.Standard;
                }
                else if (ctrl is DateTimePicker dtp)
                {
                    dtp.BackColor = SystemColors.Window;
                    dtp.ForeColor = SystemColors.WindowText;
                    // Для светлой можно сбросить календарные цвета к системным
                    dtp.CalendarMonthBackground = SystemColors.Window;
                    dtp.CalendarForeColor = SystemColors.WindowText;
                    dtp.CalendarTitleBackColor = SystemColors.Control;
                    dtp.CalendarTitleForeColor = SystemColors.WindowText;
                    dtp.CalendarTrailingForeColor = SystemColors.GrayText;
                }
                else if (ctrl is TabControl tab)
                {
                    tab.BackColor = SystemColors.Control;
                    tab.ForeColor = SystemColors.ControlText;
                    foreach (TabPage page in tab.TabPages)
                    {
                        page.BackColor = SystemColors.Control;
                        page.ForeColor = SystemColors.ControlText;
                    }
                }
                // сброс для остальных
            }

            // Рекурсивно обрабатываем вложенные контролы
            foreach (Control child in ctrl.Controls)
                ApplyThemeToControlInternal(child, mode);
        }

        public static void ApplyThemeToMenuStrip(MenuStrip menuStrip, ThemeMode mode)
        {
            if (mode == ThemeMode.Dark)
            {
                menuStrip.BackColor = Color.FromArgb(45, 45, 48);
                menuStrip.ForeColor = Color.WhiteSmoke;
                menuStrip.Renderer = new CustomMenuRenderer(new DarkColorTable());
            }
            else
            {
                menuStrip.BackColor = SystemColors.MenuBar;
                menuStrip.ForeColor = SystemColors.MenuText;
                menuStrip.Renderer = new CustomMenuRenderer(new LightColorTable());
            }

            // Применяем цвета ко всем элементам меню
            foreach (ToolStripItem item in menuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.BackColor = menuStrip.BackColor;
                    menuItem.ForeColor = menuStrip.ForeColor;
                }
            }
        }

        // Кастомный рендерер, принимающий цветовую таблицу в конструкторе
        private class CustomMenuRenderer : ToolStripProfessionalRenderer
        {
            public CustomMenuRenderer(ProfessionalColorTable colorTable) : base(colorTable)
            {
            }
        }

        // Цветовая таблица для тёмной темы
        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(70, 70, 80);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 80);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 80);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(50, 50, 60);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(50, 50, 60);
            public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);
            public override Color MenuBorder => Color.FromArgb(80, 80, 90);
            public override Color MenuItemBorder => Color.FromArgb(80, 80, 90);
        }

        // Цветовая таблица для светлой темы (можно оставить пустой, чтобы использовать системные)
        private class LightColorTable : ProfessionalColorTable
        {
            // Оставляем пустым — тогда будут использоваться стандартные системные цвета
        }

        // Классы цветовых таблиц для меню
        private class CustomColorTableDark : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(70, 70, 80);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 80);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 80);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(50, 50, 60);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(50, 50, 60);
            public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);
            public override Color MenuBorder => Color.FromArgb(80, 80, 90);
            public override Color MenuItemBorder => Color.FromArgb(80, 80, 90);
        }

        private class CustomColorTableLight : ProfessionalColorTable
        {
            // Можно оставить системные или настроить свои пастельные тона
        }
        // ---- Настройки DataGridView ----
        private static void ApplyDarkThemeToDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.FromArgb(45, 45, 48);
            dgv.GridColor = Color.FromArgb(68, 68, 68);
            dgv.ForeColor = Color.WhiteSmoke;
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(90, 122, 154);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgv.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
            dgv.RowHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgv.EnableHeadersVisualStyles = false;
        }

        private static void ApplyLightThemeToDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = SystemColors.Window;
            dgv.GridColor = SystemColors.ControlDark;
            dgv.ForeColor = SystemColors.WindowText;
            dgv.DefaultCellStyle.BackColor = SystemColors.Window;
            dgv.DefaultCellStyle.ForeColor = SystemColors.WindowText;
            dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            dgv.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;
            dgv.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            dgv.RowHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;
            dgv.EnableHeadersVisualStyles = true;
        }
    }

}
