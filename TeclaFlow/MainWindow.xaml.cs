using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TeclaFlow;

public partial class MainWindow : Window
{
    private const int HotkeyPauseId = 7001;
    private const int HotkeyStopId = 7002;
    private const int WmHotkey = 0x0312;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkF7 = 0x76;
    private const uint VkF8 = 0x77;

    private readonly string[] _tutorialTitles =
    {
        "Bienvenido a TeclaFlow",
        "Tú eliges el destino",
        "Siempre tienes el control"
    };

    private readonly string[] _tutorialBodies =
    {
        "Coloca tu contenido en el editor y selecciona un intervalo fijo entre caracteres.",
        "Al comenzar, TeclaFlow se minimiza y muestra una banda flotante. Durante la cuenta regresiva, haz clic en el lugar exacto donde debe empezar la escritura.",
        "Pulsa F7 para pausar o continuar y F8 para detener inmediatamente desde cualquier ventana."
    };

    private readonly string[] _tutorialIcons = { "⌨", "◎", "■" };
    private CancellationTokenSource? _typingCancellation;
    private HwndSource? _windowSource;
    private bool _isRunning;
    private bool _isPaused;
    private int _tutorialPage;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += MainWindow_SourceInitialized;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        _windowSource = HwndSource.FromHwnd(handle);
        _windowSource?.AddHook(WindowMessageHook);

        RegisterHotKey(handle, HotkeyPauseId, ModNoRepeat, VkF7);
        RegisterHotKey(handle, HotkeyStopId, ModNoRepeat, VkF8);
    }

    private IntPtr WindowMessageHook(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WmHotkey)
            return IntPtr.Zero;

        var id = wParam.ToInt32();
        if (id == HotkeyPauseId && _isRunning)
        {
            _isPaused = !_isPaused;
            StatusText.Text = _isPaused ? "Escritura pausada · F7 para continuar" : "Escribiendo… · F7 pausa · F8 detiene";
            handled = true;
        }
        else if (id == HotkeyStopId && _isRunning)
        {
            _typingCancellation?.Cancel();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateSpeedAndEstimate();
        ShowTutorial();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _typingCancellation?.Cancel();
        var handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero)
        {
            UnregisterHotKey(handle, HotkeyPauseId);
            UnregisterHotKey(handle, HotkeyStopId);
        }
        _windowSource?.RemoveHook(WindowMessageHook);
    }

    private void InputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var count = InputTextBox.Text.Length;
        CharacterCountText.Text = count == 1 ? "1 carácter" : $"{count:N0} caracteres";
        StartButton.IsEnabled = count > 0 && !_isRunning;
        UpdateSpeedAndEstimate();
    }

    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
            return;
        UpdateSpeedAndEstimate();
    }

    private void CountdownSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
            return;
        CountdownValueText.Text = $"{(int)CountdownSlider.Value} segundos";
    }

    private void UpdateSpeedAndEstimate()
    {
        if (SpeedValueText is null || InputTextBox is null)
            return;

        var delay = (int)SpeedSlider.Value;
        SpeedValueText.Text = $"{delay} ms";
        var duration = TimeSpan.FromMilliseconds(InputTextBox.Text.Length * (long)delay);
        EstimateText.Text = InputTextBox.Text.Length == 0
            ? "Duración estimada: —"
            : $"Duración estimada: {FormatDuration(duration)}";
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours} h {duration.Minutes} min";
        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes} min {duration.Seconds} s";
        return $"{Math.Max(1, (int)Math.Ceiling(duration.TotalSeconds))} s";
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning || string.IsNullOrEmpty(InputTextBox.Text))
            return;

        var text = InputTextBox.Text.Replace("\r\n", "\n").Replace('\r', '\n');
        var delayMilliseconds = (int)SpeedSlider.Value;
        var countdownSeconds = (int)CountdownSlider.Value;

        _typingCancellation = new CancellationTokenSource();
        var cancellationToken = _typingCancellation.Token;
        _isRunning = true;
        _isPaused = false;
        SetEditorEnabled(false);
        TypingProgress.Value = 0;
        ProgressText.Text = "0%";
        CountdownOverlayWindow? countdownOverlay = null;

        try
        {
            countdownOverlay = new CountdownOverlayWindow();
            countdownOverlay.Show();
            WindowState = WindowState.Minimized;
            for (var remaining = countdownSeconds; remaining > 0; remaining--)
            {
                countdownOverlay.UpdateCountdown(remaining);
                StatusText.Text = $"Comenzando en {remaining}… elige la ventana destino";
                await Task.Delay(1000, cancellationToken);
            }

            countdownOverlay.Close();
            countdownOverlay = null;
            StatusText.Text = "Escribiendo… · F7 pausa · F8 detiene";
            for (var index = 0; index < text.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                while (_isPaused)
                {
                    await Task.Delay(80, cancellationToken);
                }

                SendCharacter(text[index]);
                var completed = index + 1;
                var percent = completed * 100d / text.Length;
                TypingProgress.Value = percent;
                ProgressText.Text = $"{percent:0}%";
                await Task.Delay(delayMilliseconds, cancellationToken);
            }

            StatusText.Text = "Escritura completada correctamente";
            TypingProgress.Value = 100;
            ProgressText.Text = "100%";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Proceso detenido por el usuario";
            WindowState = WindowState.Normal;
            Activate();
        }
        catch (Exception exception)
        {
            StatusText.Text = "No se pudo continuar";
            WindowState = WindowState.Normal;
            Activate();
            MessageBox.Show(this,
                $"TeclaFlow no pudo enviar la entrada de teclado.\n\n{exception.Message}\n\nSi la aplicación destino se ejecuta como administrador, abre TeclaFlow con el mismo nivel de permisos.",
                "No se pudo escribir", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            countdownOverlay?.Close();
            _isRunning = false;
            _isPaused = false;
            _typingCancellation.Dispose();
            _typingCancellation = null;
            SetEditorEnabled(true);
        }
    }

    private void SetEditorEnabled(bool enabled)
    {
        InputTextBox.IsEnabled = enabled;
        SpeedSlider.IsEnabled = enabled;
        CountdownSlider.IsEnabled = enabled;
        StartButton.IsEnabled = enabled && InputTextBox.Text.Length > 0;
    }

    private static void SendCharacter(char character)
    {
        if (character == '\n')
        {
            SendVirtualKey(0x0D);
            return;
        }

        if (character == '\t')
        {
            SendVirtualKey(0x09);
            return;
        }

        var inputs = new[]
        {
            CreateUnicodeInput(character, keyUp: false),
            CreateUnicodeInput(character, keyUp: true)
        };

        if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) != inputs.Length)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private static void SendVirtualKey(ushort virtualKey)
    {
        var inputs = new[]
        {
            CreateVirtualKeyInput(virtualKey, keyUp: false),
            CreateVirtualKeyInput(virtualKey, keyUp: true)
        };

        if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) != inputs.Length)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private static INPUT CreateUnicodeInput(char character, bool keyUp) => new()
    {
        Type = 1,
        Data = new InputUnion
        {
            Keyboard = new KEYBDINPUT
            {
                Scan = character,
                Flags = 0x0004u | (keyUp ? 0x0002u : 0u)
            }
        }
    };

    private static INPUT CreateVirtualKeyInput(ushort virtualKey, bool keyUp) => new()
    {
        Type = 1,
        Data = new InputUnion
        {
            Keyboard = new KEYBDINPUT
            {
                VirtualKey = virtualKey,
                Flags = keyUp ? 0x0002u : 0u
            }
        }
    };

    private void WriteNavButton_Click(object sender, RoutedEventArgs e) => ShowView(WriteView, WriteNavButton);
    private void GuideNavButton_Click(object sender, RoutedEventArgs e) => ShowView(GuideView, GuideNavButton);
    private void AboutNavButton_Click(object sender, RoutedEventArgs e) => ShowView(AboutView, AboutNavButton);

    private void ShowView(UIElement selectedView, System.Windows.Controls.Button selectedButton)
    {
        WriteView.Visibility = Visibility.Collapsed;
        GuideView.Visibility = Visibility.Collapsed;
        AboutView.Visibility = Visibility.Collapsed;
        WriteNavButton.Tag = null;
        GuideNavButton.Tag = null;
        AboutNavButton.Tag = null;

        selectedView.Opacity = 0;
        selectedView.Visibility = Visibility.Visible;
        selectedButton.Tag = "Selected";
        selectedView.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
    }

    private void ShowTutorial_Click(object sender, RoutedEventArgs e) => ShowTutorial();

    private void ShowTutorial()
    {
        _tutorialPage = 0;
        UpdateTutorialPage();
        TutorialOverlay.Visibility = Visibility.Visible;
        TutorialOverlay.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)));
    }

    private void CloseTutorial_Click(object sender, RoutedEventArgs e) => HideTutorial();

    private void HideTutorial()
    {
        var animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
        animation.Completed += (_, _) => TutorialOverlay.Visibility = Visibility.Collapsed;
        TutorialOverlay.BeginAnimation(OpacityProperty, animation);
    }

    private void TutorialNext_Click(object sender, RoutedEventArgs e)
    {
        if (_tutorialPage == _tutorialTitles.Length - 1)
        {
            HideTutorial();
            return;
        }

        _tutorialPage++;
        UpdateTutorialPage();
    }

    private void TutorialBack_Click(object sender, RoutedEventArgs e)
    {
        if (_tutorialPage <= 0)
            return;
        _tutorialPage--;
        UpdateTutorialPage();
    }

    private void UpdateTutorialPage()
    {
        TutorialTitle.Text = _tutorialTitles[_tutorialPage];
        TutorialBody.Text = _tutorialBodies[_tutorialPage];
        TutorialIcon.Text = _tutorialIcons[_tutorialPage];
        TutorialBackButton.Visibility = _tutorialPage == 0 ? Visibility.Hidden : Visibility.Visible;
        TutorialNextButton.Content = _tutorialPage == _tutorialTitles.Length - 1 ? "Empezar  ✓" : "Siguiente  →";

        var dots = new Ellipse[] { Dot0, Dot1, Dot2 };
        var activeBrush = (Brush)FindResource("PrimaryBrush");
        var inactiveBrush = new SolidColorBrush(Color.FromRgb(215, 218, 229));
        for (var i = 0; i < dots.Length; i++)
            dots[i].Fill = i == _tutorialPage ? activeBrush : inactiveBrush;

        TutorialTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
        TutorialBody.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT Keyboard;
        [FieldOffset(0)] public MOUSEINPUT Mouse;
        [FieldOffset(0)] public HARDWAREINPUT Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort VirtualKey;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint Message;
        public ushort ParameterLow;
        public ushort ParameterHigh;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, INPUT[] inputs, int inputSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr windowHandle, int id);
}
