using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfDateTimePicker.Controls
{
    /// <summary>
    /// DateTimePicker.xaml 的交互逻辑
    /// </summary>
    public partial class DateTimePicker : UserControl
    {
        #region 依赖属性

        /// <summary>
        /// 选中的日期时间（可空）
        /// </summary>
        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register("SelectedDateTime", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, OnSelectedDateTimeChanged));

        /// <summary>
        /// 选中的日期（可空）
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, OnSelectedDateChanged));

        /// <summary>
        /// 选中的时间（可空）
        /// </summary>
        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register("SelectedTime", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, OnSelectedTimeChanged));

        /// <summary>
        /// 当前显示的月份
        /// </summary>
        public static readonly DependencyProperty CurrentMonthProperty =
            DependencyProperty.Register("CurrentMonth", typeof(DateTime), typeof(DateTimePicker),
                new PropertyMetadata(DateTime.Now));

        /// <summary>
        /// 最小日期时间（可空）
        /// </summary>
        public static readonly DependencyProperty MinDateTimeProperty =
            DependencyProperty.Register("MinDateTime", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, OnMinMaxDateTimeChanged));

        /// <summary>
        /// 最大日期时间（可空）
        /// </summary>
        public static readonly DependencyProperty MaxDateTimeProperty =
            DependencyProperty.Register("MaxDateTime", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, OnMinMaxDateTimeChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置选中的日期时间（可空）
        /// </summary>
        public DateTime? SelectedDateTime
        {
            get { return (DateTime?)GetValue(SelectedDateTimeProperty); }
            set { SetValue(SelectedDateTimeProperty, value); }
        }

        /// <summary>
        /// 获取或设置选中的日期（可空）
        /// </summary>
        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        /// <summary>
        /// 获取或设置选中的时间（可空）
        /// </summary>
        public DateTime? SelectedTime
        {
            get { return (DateTime?)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        /// <summary>
        /// 获取或设置当前显示的月份
        /// </summary>
        public DateTime CurrentMonth
        {
            get { return (DateTime)GetValue(CurrentMonthProperty); }
            set { SetValue(CurrentMonthProperty, value); }
        }

        /// <summary>
        /// 获取或设置最小日期时间（可空）
        /// </summary>
        public DateTime? MinDateTime
        {
            get { return (DateTime?)GetValue(MinDateTimeProperty); }
            set { SetValue(MinDateTimeProperty, value); }
        }

        /// <summary>
        /// 获取或设置最大日期时间（可空）
        /// </summary>
        public DateTime? MaxDateTime
        {
            get { return (DateTime?)GetValue(MaxDateTimeProperty); }
            set { SetValue(MaxDateTimeProperty, value); }
        }

        #endregion

        /// <summary>
        /// 日期时间变更事件
        /// </summary>
        public event EventHandler<DateTime?> DateTimeChanged;

        private List<Button> dayButtons = new List<Button>();
        private DateTime tempDateTime; // 临时存储时间选择
        private bool isUpdatingFromProperties = false; // 防止循环更新

        public DateTimePicker()
        {
            InitializeComponent();
            
            // 如果SelectedDateTime为null，设置为当前时间
            if (SelectedDateTime.HasValue)
            {
                CurrentMonth = SelectedDateTime.Value;             
               
            }
            GenerateCalendar();
            InitializeTimeLists();
            InitializeInputValidation();

        }

        #region 私有方法

        private void InitializeTimeLists()
        {
            // 初始化小时列表 (00-23)
            for (int i = 0; i < 24; i++)
            {
                HourListBox.Items.Add(i.ToString("D2"));
            }

            // 初始化分钟列表 (00-59)
            for (int i = 0; i < 60; i++)
            {
                MinuteListBox.Items.Add(i.ToString("D2"));
            }

            // 初始化秒列表 (00-59)
            for (int i = 0; i < 60; i++)
            {
                SecondListBox.Items.Add(i.ToString("D2"));
            }

            // 设置当前选中项
            UpdateTimeSelection();
        }

        private void InitializeInputValidation()
        {
            // 为主显示框添加失去焦点验证
            DisplayText.LostFocus += DisplayText_LostFocus;

            // 为日期输入框添加失去焦点验证
            DateInput.LostFocus += DateInput_LostFocus;

            // 为时间输入框添加失去焦点验证
            if (TimeInput != null)
            {
                TimeInput.LostFocus += TimeInput_LostFocus;
            }
        }

        private void UpdateTimeSelection()
        {
            if (SelectedDateTime.HasValue)
            {
                tempDateTime = SelectedDateTime.Value;
                HourListBox.SelectedIndex = tempDateTime.Hour;
                MinuteListBox.SelectedIndex = tempDateTime.Minute;
                SecondListBox.SelectedIndex = tempDateTime.Second;

                // 滚动到选中项
                if (HourListBox.SelectedItem != null)
                    HourListBox.ScrollIntoView(HourListBox.SelectedItem);
                if (MinuteListBox.SelectedItem != null)
                    MinuteListBox.ScrollIntoView(MinuteListBox.SelectedItem);
                if (SecondListBox.SelectedItem != null)
                    SecondListBox.ScrollIntoView(SecondListBox.SelectedItem);
            }
            else
            {
                // 如果SelectedDateTime为null，使用当前时间作为临时值
                tempDateTime = DateTime.Now;
                HourListBox.SelectedIndex = tempDateTime.Hour;
                MinuteListBox.SelectedIndex = tempDateTime.Minute;
                SecondListBox.SelectedIndex = tempDateTime.Second;
            }
        }

        private void UpdatePropertiesFromSelectedDateTime()
        {
            if (isUpdatingFromProperties) return;

            isUpdatingFromProperties = true;
            try
            {
                if (SelectedDateTime.HasValue)
                {
                    var dateTime = SelectedDateTime.Value;
                    SelectedDate = dateTime.Date;
                    SelectedTime = new DateTime(1900, 1, 1).AddHours(dateTime.Hour).AddMinutes(dateTime.Minute).AddSeconds(dateTime.Second);
                }
                else
                {
                    SelectedDate = null;
                    SelectedTime = null;
                }
            }
            finally
            {
                isUpdatingFromProperties = false;
            }
        }

        private void UpdateSelectedDateTimeFromProperties()
        {
            if (isUpdatingFromProperties) return;

            isUpdatingFromProperties = true;
            try
            {
                if (SelectedDate.HasValue && SelectedTime.HasValue)
                {
                    var date = SelectedDate.Value;
                    var time = SelectedTime.Value;
                    SelectedDateTime = new DateTime(date.Year, date.Month, date.Day, 
                        time.Hour, time.Minute, time.Second);
                }
                else if (SelectedDate.HasValue)
                {
                    var date = SelectedDate.Value;
                    SelectedDateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                }
                else if (SelectedTime.HasValue)
                {
                    var time = SelectedTime.Value;
                    var now = DateTime.Now;
                    SelectedDateTime = new DateTime(now.Year, now.Month, now.Day, 
                        time.Hour, time.Minute, time.Second);
                }
                else
                {
                    SelectedDateTime = null;
                }
            }
            finally
            {
                isUpdatingFromProperties = false;
            }
        }

        #endregion

        #region 范围验证方法

        /// <summary>
        /// 检查日期是否在允许的范围内
        /// </summary>
        private bool IsDateInRange(DateTime date)
        {
            if (MinDateTime.HasValue && date < MinDateTime.Value.Date)
                return false;
            if (MaxDateTime.HasValue && date > MaxDateTime.Value.Date)
                return false;
            return true;
        }

        /// <summary>
        /// 检查日期时间是否在允许的范围内
        /// </summary>
        private bool IsDateTimeInRange(DateTime dateTime)
        {
            if (MinDateTime.HasValue && dateTime < MinDateTime.Value)
                return false;
            if (MaxDateTime.HasValue && dateTime > MaxDateTime.Value)
                return false;
            return true;
        }

        /// <summary>
        /// 将日期时间限制在允许的范围内
        /// </summary>
        private DateTime ClampDateTime(DateTime dateTime)
        {
            if (MinDateTime.HasValue && dateTime < MinDateTime.Value)
                return MinDateTime.Value;
            if (MaxDateTime.HasValue && dateTime > MaxDateTime.Value)
                return MaxDateTime.Value;
            return dateTime;
        }

        #endregion

        #region 依赖属性回调

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DateTimePicker)d;
            var newValue = (DateTime?)e.NewValue;
            
            if (newValue.HasValue)
            {
                picker.CurrentMonth = newValue.Value.Date;
                picker.GenerateCalendar();
            }
            
            picker.UpdatePropertiesFromSelectedDateTime();
            picker.DateTimeChanged?.Invoke(picker, newValue);
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DateTimePicker)d;
            picker.UpdateSelectedDateTimeFromProperties();
        }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DateTimePicker)d;
            picker.UpdateSelectedDateTimeFromProperties();
        }

        private static void OnMinMaxDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (DateTimePicker)d;
            picker.GenerateCalendar(); // 重新生成日历以应用范围
        }

        #endregion

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 关闭时间弹框，打开日期弹框
            TimePopup.IsOpen = false;
            DateTimePopup.IsOpen = !DateTimePopup.IsOpen;
        }

        private void TimeInput_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 保存当前时间到临时变量
            tempDateTime = SelectedDateTime ?? DateTime.Now;

            // 打开时间弹框，但不关闭日期弹框
            TimePopup.IsOpen = !TimePopup.IsOpen;

            if (TimePopup.IsOpen)
            {
                UpdateTimeSelection();
            }

            e.Handled = true; // 防止事件冒泡
        }

        private void HourListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HourListBox.SelectedIndex >= 0)
            {
                tempDateTime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day,
                    HourListBox.SelectedIndex, tempDateTime.Minute, tempDateTime.Second);
            }
        }

        private void MinuteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MinuteListBox.SelectedIndex >= 0)
            {
                tempDateTime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day,
                    tempDateTime.Hour, MinuteListBox.SelectedIndex, tempDateTime.Second);
            }
        }

        private void SecondListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SecondListBox.SelectedIndex >= 0)
            {
                tempDateTime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day,
                    tempDateTime.Hour, tempDateTime.Minute, SecondListBox.SelectedIndex);
            }
        }

        private void TimeCancel_Click(object sender, RoutedEventArgs e)
        {
            // 取消时间选择，不保存更改
            TimePopup.IsOpen = false;
        }

        private void TimeConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 确认时间选择，保存更改
            SelectedDateTime = tempDateTime;
            TimePopup.IsOpen = false;
        }

        private void GenerateCalendar()
        {
            CalendarGrid.Children.Clear();
            dayButtons.Clear();

            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            var today = DateTime.Today;

            // 添加上个月的日期
            var previousMonth = firstDayOfMonth.AddMonths(-1);
            var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            for (int i = firstDayOfWeek - 1; i >= 0; i--)
            {
                var day = daysInPreviousMonth - i;
                var date = new DateTime(previousMonth.Year, previousMonth.Month, day);
                var button = CreateDayButton(day.ToString(), date, true);

                // 检查是否在范围内
                if (!IsDateInRange(date))
                {
                    button.IsEnabled = false;
                    button.Foreground = new SolidColorBrush(Color.FromRgb(192, 196, 204));
                }

                CalendarGrid.Children.Add(button);
                dayButtons.Add(button);
            }

            // 添加当前月的日期
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                var button = CreateDayButton(day.ToString(), date, false);

                // 检查是否在范围内
                if (!IsDateInRange(date))
                {
                    button.IsEnabled = false;
                    button.Foreground = new SolidColorBrush(Color.FromRgb(192, 196, 204));
                }
                else
                {
                    // 设置样式
                    if (SelectedDateTime.HasValue && date.Date == SelectedDateTime.Value.Date)
                    {
                        button.Style = (Style)FindResource("SelectedDateButtonStyle");
                    }
                    else if (date.Date == today)
                    {
                        button.Style = (Style)FindResource("TodayButtonStyle");
                    }
                }

                CalendarGrid.Children.Add(button);
                dayButtons.Add(button);
            }

            // 添加下个月的日期
            var nextMonth = firstDayOfMonth.AddMonths(1);
            var remainingCells = 42 - CalendarGrid.Children.Count;

            for (int day = 1; day <= remainingCells; day++)
            {
                var date = new DateTime(nextMonth.Year, nextMonth.Month, day);
                var button = CreateDayButton(day.ToString(), date, true);

                // 检查是否在范围内
                if (!IsDateInRange(date))
                {
                    button.IsEnabled = false;
                    button.Foreground = new SolidColorBrush(Color.FromRgb(192, 196, 204));
                }

                CalendarGrid.Children.Add(button);
                dayButtons.Add(button);
            }
        }

        private Button CreateDayButton(string content, DateTime date, bool isOtherMonth)
        {
            var button = new Button
            {
                Content = content,
                Style = (Style)FindResource("CalendarButtonStyle"),
                Tag = date
            };

            if (isOtherMonth)
            {
                button.Foreground = new SolidColorBrush(Color.FromRgb(192, 196, 204));
            }

            button.Click += DayButton_Click;
            return button;
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var selectedDate = (DateTime)button.Tag;

            // 检查选择的日期是否在范围内
            if (!IsDateInRange(selectedDate))
            {
                return; // 如果不在范围内，不执行选择操作
            }

            // 保持原有的时间部分
            var currentTime = SelectedDateTime ?? DateTime.Now;
            var newDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day,
                currentTime.Hour, currentTime.Minute, currentTime.Second);

            // 检查完整的日期时间是否在范围内
            if (!IsDateTimeInRange(newDateTime))
            {
                // 如果超出范围，调整到边界值
                newDateTime = ClampDateTime(newDateTime);
            }

            SelectedDateTime = newDateTime;

            // 当点击日期时关闭时间弹框
            TimePopup.IsOpen = false;

            // 如果选择的是其他月份的日期，切换月份
            if (selectedDate.Month != CurrentMonth.Month || selectedDate.Year != CurrentMonth.Year)
            {
                CurrentMonth = selectedDate;
                GenerateCalendar();
            }
            else
            {
                // 更新按钮样式
                foreach (var btn in dayButtons)
                {
                    var btnDate = (DateTime)btn.Tag;
                    if (btnDate.Date == selectedDate.Date)
                    {
                        btn.Style = (Style)FindResource("SelectedDateButtonStyle");
                    }
                    else if (btnDate.Date == DateTime.Today && btnDate.Month == CurrentMonth.Month)
                    {
                        btn.Style = (Style)FindResource("TodayButtonStyle");
                    }
                    else
                    {
                        btn.Style = (Style)FindResource("CalendarButtonStyle");
                        if (btnDate.Month != CurrentMonth.Month)
                        {
                            btn.Foreground = new SolidColorBrush(Color.FromRgb(192, 196, 204));
                        }
                    }
                }
            }
        }

        private void DateInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DateTime.TryParseExact(DateInput.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                var currentTime = SelectedDateTime ?? DateTime.Now;
                SelectedDateTime = new DateTime(date.Year, date.Month, date.Day,
                    currentTime.Hour, currentTime.Minute, currentTime.Second);
                CurrentMonth = date;
                GenerateCalendar();
            }
        }

        private void PreviousYear_Click(object sender, RoutedEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddYears(-1);
            GenerateCalendar();
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            GenerateCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            GenerateCalendar();
        }

        private void NextYear_Click(object sender, RoutedEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddYears(1);
            GenerateCalendar();
        }

        private void Now_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            
            // 检查当前时间是否在范围内
            if (!IsDateTimeInRange(now))
            {
                // 如果超出范围，调整到边界值
                now = ClampDateTime(now);
            }

            SelectedDateTime = now;
            CurrentMonth = now;
            GenerateCalendar();
        }

        private void DateConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 解析输入框中的日期
            if (DateTime.TryParseExact(DateInput.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                var currentTime = SelectedDateTime ?? DateTime.Now;
                SelectedDateTime = new DateTime(date.Year, date.Month, date.Day,
                    currentTime.Hour, currentTime.Minute, currentTime.Second);
            }

            // 只有点击确定按钮才关闭日期弹框
            DateTimePopup.IsOpen = false;
        }

        #region 失去焦点验证方法

        private void DisplayText_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var inputText = textBox.Text.Trim();

            // 如果输入为空，保持当前值
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }

            // 尝试解析输入的日期时间
            if (!DateTime.TryParseExact(inputText, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
            {
                // 解析失败，恢复到原来的有效值
                if (SelectedDateTime.HasValue)
                {
                    textBox.Text = SelectedDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    textBox.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else
            {
                // 检查是否在范围内
                if (!IsDateTimeInRange(parsedDateTime))
                {
                    // 如果超出范围，调整到边界值
                    parsedDateTime = ClampDateTime(parsedDateTime);
                    textBox.Text = parsedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // 更新SelectedDateTime
                SelectedDateTime = parsedDateTime;
            }
        }

        private void DateInput_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var inputText = textBox.Text.Trim();

            // 如果输入为空，保持当前值
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }

            // 尝试解析输入的日期
            if (!DateTime.TryParseExact(inputText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                // 解析失败，恢复到原来的有效值
                if (SelectedDateTime.HasValue)
                {
                    textBox.Text = SelectedDateTime.Value.ToString("yyyy-MM-dd");
                }
                else
                {
                    textBox.Text = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                // 检查日期是否在范围内
                if (!IsDateInRange(parsedDate))
                {
                    // 如果超出范围，调整到边界值
                    if (MinDateTime.HasValue && parsedDate < MinDateTime.Value.Date)
                        parsedDate = MinDateTime.Value.Date;
                    else if (MaxDateTime.HasValue && parsedDate > MaxDateTime.Value.Date)
                        parsedDate = MaxDateTime.Value.Date;
                    textBox.Text = parsedDate.ToString("yyyy-MM-dd");
                }

                // 解析成功，更新SelectedDateTime（保持原有时间部分）
                var currentTime = SelectedDateTime ?? DateTime.Now;
                var newDateTime = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day,
                    currentTime.Hour, currentTime.Minute, currentTime.Second);

                // 检查完整的日期时间是否在范围内
                if (!IsDateTimeInRange(newDateTime))
                {
                    newDateTime = ClampDateTime(newDateTime);
                }

                SelectedDateTime = newDateTime;
            }
        }

        private void TimeInput_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var inputText = textBox.Text.Trim();

            // 如果输入为空，保持当前值
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }

            // 尝试解析输入的时间
            if (!DateTime.TryParseExact(inputText, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
            {
                // 解析失败，恢复到原来的有效值
                if (SelectedDateTime.HasValue)
                {
                    textBox.Text = SelectedDateTime.Value.ToString("HH:mm:ss");
                }
                else
                {
                    textBox.Text = DateTime.Now.ToString("HH:mm:ss");
                }
            }
            else
            {
                // 解析成功，更新SelectedDateTime（保持原有日期部分）
                var currentDate = SelectedDateTime ?? DateTime.Now;
                var newDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day,
                    parsedTime.Hour, parsedTime.Minute, parsedTime.Second);

                // 检查完整的日期时间是否在范围内
                if (!IsDateTimeInRange(newDateTime))
                {
                    // 如果超出范围，调整到边界值
                    newDateTime = ClampDateTime(newDateTime);
                    textBox.Text = newDateTime.ToString("HH:mm:ss");
                }

                SelectedDateTime = newDateTime;
            }
        }

        #endregion
    }
}
