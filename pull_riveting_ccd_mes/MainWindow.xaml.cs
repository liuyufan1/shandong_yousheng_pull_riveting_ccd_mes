using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using pull_riveting_ccd_mes.programUtil.log;

namespace pull_riveting_ccd_mes;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // 日志最大行数
    private const int MaxLogLines = 500;
    // mainWindow 实例
    public static MainWindow? Install;
    
    public MainWindow()
    {
        InitializeComponent();
        Install = this;
        LogUtil.ShowInMainPgae += AppendLog;
    }
    
    // 追加日志的 UI 操作（UI 线程）
    private void AppendLog(string message)
    {
        Dispatcher.Invoke(() =>
        {
            string timeStampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            LogTextBox.AppendText(timeStampedMessage + Environment.NewLine);

            // 限制最大行数
            if (LogTextBox.LineCount > MaxLogLines)
            {
                int removeLinesCount = LogTextBox.LineCount - MaxLogLines;
                int start = LogTextBox.GetCharacterIndexFromLineIndex(0);
                int end = LogTextBox.GetCharacterIndexFromLineIndex(removeLinesCount);
            
                LogTextBox.Select(start, end);        // 选中最早的多余行
                LogTextBox.SelectedText = "";         // 删除选中内容，不会改变后续光标
            }

            LogTextBox.CaretIndex = LogTextBox.Text.Length; // 光标移动到末尾
            LogTextBox.ScrollToEnd();                         // 滚动到底部
        });
    }


}