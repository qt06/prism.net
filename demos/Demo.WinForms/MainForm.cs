using System.Runtime.InteropServices;
using Prism;
using Prism.Primitives;
using Prism.Exceptions;

public class MainForm : Form
{
    private readonly Context _context = new();
    private Backend? _backend;

    // Backend panel
    private ListBox _backendListBox = default!;
    private Button _refreshBackendsBtn = default!;
    private Button _createBtn = default!;
    private Button _acquireBtn = default!;
    private Button _createBestBtn = default!;
    private Button _acquireBestBtn = default!;
    private Label _activeBackendLabel = default!;
    private CheckBox _interruptCheckBox = default!;

    // Log panel
    private RichTextBox _logBox = default!;
    private Button _clearLogBtn = default!;

    // Speech panel
    private TextBox _speechText = default!;
    private Button _speakBtn = default!;
    private Button _speakToMemoryBtn = default!;
    private Button _brailleBtn = default!;
    private Button _outputBtn = default!;
    private Button _stopBtn = default!;
    private Button _pauseBtn = default!;
    private Button _resumeBtn = default!;
    private Label _isSpeakingLabel = default!;

    // Parameter panel
    private TrackBar _volumeTrackBar = default!;
    private Label _volumeLabel = default!;
    private TrackBar _rateTrackBar = default!;
    private Label _rateLabel = default!;
    private TrackBar _pitchTrackBar = default!;
    private Label _pitchLabel = default!;

    // Voice panel
    private ListBox _voiceListBox = default!;
    private Button _refreshVoicesBtn = default!;
    private Button _selectVoiceBtn = default!;
    private Label _currentVoiceLabel = default!;

    // Audio info panel
    private Label _channelsLabel = default!;
    private Label _sampleRateLabel = default!;
    private Label _bitDepthLabel = default!;

    // Availability / Logger
    private Button _isAutoPowerBtn = default!;
    private Button _pollPauseBtn = default!;
    private Button _pollResumeBtn = default!;
    private Button _logFlushBtn = default!;
    private Button _logShutdownBtn = default!;
    private ComboBox _logLevelCombo = default!;
    private Button _logLevelSetBtn = default!;

    public MainForm()
    {
        Text = "Prism.Net 测试程序";
        Size = new Size(1000, 750);
        BackColor = Color.White;

        BuildUI();
        LoadBackends();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _backend?.Dispose();
        _context.Dispose();
        base.OnFormClosed(e);
    }

    // ─── UI Construction ────────────────────────────────────────

    private void BuildUI()
    {
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 6,
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        mainPanel.Controls.Add(BuildTopSplitPanel(), 0, 0);
        mainPanel.Controls.Add(BuildSpeechPanel(), 0, 1);
        mainPanel.Controls.Add(BuildParameterPanel(), 0, 2);
        mainPanel.Controls.Add(BuildVoicePanel(), 0, 3);
        mainPanel.Controls.Add(BuildInfoPanel(), 0, 4);
        mainPanel.Controls.Add(BuildLogPanel(), 0, 5);

        Controls.Add(mainPanel);
    }

    private Control BuildTopSplitPanel()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 380,
        };

        // Left: Backend panel
        var backendPanel = new GroupBox { Text = "后端 (Backends)", Dock = DockStyle.Fill };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        _backendListBox = new ListBox { Dock = DockStyle.Fill };
        tbl.Controls.Add(_backendListBox, 0, 0);
        tbl.SetRowSpan(_backendListBox, 4);

        var btnPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
        _refreshBackendsBtn = new Button { Text = "刷新后端", Dock = DockStyle.Fill, Height = 28 };
        _refreshBackendsBtn.Click += (_, _) => LoadBackends();
        btnPanel.Controls.Add(_refreshBackendsBtn, 0, 0);

        _createBtn = new Button { Text = "创建(Create)", Dock = DockStyle.Fill, Height = 28 };
        _createBtn.Click += (_, _) => CreateBackend(false);
        btnPanel.Controls.Add(_createBtn, 0, 1);

        _acquireBtn = new Button { Text = "获取(Acquire)", Dock = DockStyle.Fill, Height = 28 };
        _acquireBtn.Click += (_, _) => CreateBackend(true);
        btnPanel.Controls.Add(_acquireBtn, 0, 2);

        _createBestBtn = new Button { Text = "CreateBest", Dock = DockStyle.Fill, Height = 28 };
        _createBestBtn.Click += (_, _) => CreateBestBackend(false);
        btnPanel.Controls.Add(_createBestBtn, 0, 3);

        _acquireBestBtn = new Button { Text = "AcquireBest", Dock = DockStyle.Fill, Height = 28 };
        _acquireBestBtn.Click += (_, _) => CreateBestBackend(true);
        btnPanel.Controls.Add(_acquireBestBtn, 0, 4);

        _interruptCheckBox = new CheckBox { Text = "中断(Interrupt)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        btnPanel.Controls.Add(_interruptCheckBox, 0, 5);

        _activeBackendLabel = new Label { Text = "未选择后端", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Gray };
        btnPanel.Controls.Add(_activeBackendLabel, 0, 6);

        tbl.Controls.Add(btnPanel, 1, 0);
        tbl.SetRowSpan(btnPanel, 4);
        backendPanel.Controls.Add(tbl);

        // Right: Log panel
        var logPanel = new GroupBox { Text = "操作日志 (Log)", Dock = DockStyle.Fill };
        var logTbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        logTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        logTbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        _logBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.Black, ForeColor = Color.Lime, Font = new Font("Consolas", 9) };
        logTbl.Controls.Add(_logBox, 0, 0);

        _clearLogBtn = new Button { Text = "清除日志", Dock = DockStyle.Fill };
        _clearLogBtn.Click += (_, _) => _logBox.Clear();
        logTbl.Controls.Add(_clearLogBtn, 0, 1);

        logPanel.Controls.Add(logTbl);
        split.Panel1.Controls.Add(backendPanel);
        split.Panel2.Controls.Add(logPanel);
        return split;
    }

    private Control BuildSpeechPanel()
    {
        var panel = new GroupBox { Text = "语音控制 (Speech)", Dock = DockStyle.Fill };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 2 };
        for (int i = 0; i < 7; i++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.29f));

        _speechText = new TextBox { Dock = DockStyle.Fill, Text = "Hello, this is a Prism test." };
        tbl.Controls.Add(_speechText, 0, 0);
        tbl.SetColumnSpan(_speechText, 7);

        _speakBtn = new Button { Text = "Speak", Dock = DockStyle.Fill, Height = 30 };
        _speakBtn.Click += (_, _) => SafeOp("Speak", () => _backend!.Speak(_speechText.Text, _interruptCheckBox.Checked));
        tbl.Controls.Add(_speakBtn, 0, 1);

        _speakToMemoryBtn = new Button { Text = "SpeakToMem", Dock = DockStyle.Fill, Height = 30 };
        _speakToMemoryBtn.Click += SpeakToMemory_Click;
        tbl.Controls.Add(_speakToMemoryBtn, 1, 1);

        _brailleBtn = new Button { Text = "Braille", Dock = DockStyle.Fill, Height = 30 };
        _brailleBtn.Click += (_, _) => SafeOp("Braille", () => _backend!.Braille(_speechText.Text));
        tbl.Controls.Add(_brailleBtn, 2, 1);

        _outputBtn = new Button { Text = "Output", Dock = DockStyle.Fill, Height = 30 };
        _outputBtn.Click += (_, _) => SafeOp("Output", () => _backend!.Output(_speechText.Text, _interruptCheckBox.Checked));
        tbl.Controls.Add(_outputBtn, 3, 1);

        _stopBtn = new Button { Text = "Stop", Dock = DockStyle.Fill, Height = 30 };
        _stopBtn.Click += (_, _) => SafeOp("Stop", () => _backend!.Stop());
        tbl.Controls.Add(_stopBtn, 4, 1);

        _pauseBtn = new Button { Text = "Pause", Dock = DockStyle.Fill, Height = 30 };
        _pauseBtn.Click += (_, _) => SafeOp("Pause", () => _backend!.Pause());
        tbl.Controls.Add(_pauseBtn, 5, 1);

        _resumeBtn = new Button { Text = "Resume", Dock = DockStyle.Fill, Height = 30 };
        _resumeBtn.Click += (_, _) => SafeOp("Resume", () => _backend!.Resume());
        tbl.Controls.Add(_resumeBtn, 6, 1);

        panel.Controls.Add(tbl);
        return panel;
    }

    private Control BuildParameterPanel()
    {
        var panel = new GroupBox { Text = "参数 (Parameters)", Dock = DockStyle.Fill };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));

        _volumeLabel = new Label { Text = "0.5", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
        _volumeTrackBar = CreateTrackBar(0, (_, e) => SetParam("Volume", v => _backend!.Volume = v, _volumeLabel, _volumeTrackBar));
        tbl.Controls.Add(new Label { Text = "Volume:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
        tbl.Controls.Add(_volumeTrackBar, 1, 0);
        tbl.Controls.Add(_volumeLabel, 2, 0);

        _rateLabel = new Label { Text = "0.5", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
        _rateTrackBar = CreateTrackBar(50, (_, e) => SetParam("Rate", v => _backend!.Rate = v, _rateLabel, _rateTrackBar));
        tbl.Controls.Add(new Label { Text = "Rate:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
        tbl.Controls.Add(_rateTrackBar, 1, 1);
        tbl.Controls.Add(_rateLabel, 2, 1);

        _pitchLabel = new Label { Text = "0.5", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
        _pitchTrackBar = CreateTrackBar(50, (_, e) => SetParam("Pitch", v => _backend!.Pitch = v, _pitchLabel, _pitchTrackBar));
        tbl.Controls.Add(new Label { Text = "Pitch:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
        tbl.Controls.Add(_pitchTrackBar, 1, 2);
        tbl.Controls.Add(_pitchLabel, 2, 2);

        panel.Controls.Add(tbl);
        return panel;
    }

    private Control BuildVoicePanel()
    {
        var panel = new GroupBox { Text = "语音 (Voices)", Dock = DockStyle.Fill };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 3 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

        _voiceListBox = new ListBox { Dock = DockStyle.Fill };
        tbl.Controls.Add(_voiceListBox, 0, 0);
        tbl.SetRowSpan(_voiceListBox, 3);

        _refreshVoicesBtn = new Button { Text = "刷新语音", Dock = DockStyle.Fill, Height = 30 };
        _refreshVoicesBtn.Click += (_, _) => LoadVoices();
        tbl.Controls.Add(_refreshVoicesBtn, 1, 0);

        _selectVoiceBtn = new Button { Text = "选择语音", Dock = DockStyle.Fill, Height = 30 };
        _selectVoiceBtn.Click += SelectVoice_Click;
        tbl.Controls.Add(_selectVoiceBtn, 2, 0);

        _isSpeakingLabel = new Label { Text = "IsSpeaking: ?", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        tbl.Controls.Add(_isSpeakingLabel, 4, 0);

        _currentVoiceLabel = new Label { Text = "当前语音: (无)", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        tbl.Controls.Add(_currentVoiceLabel, 1, 1);
        tbl.SetColumnSpan(_currentVoiceLabel, 4);

        var refreshIsSpeaking = new Button { Text = "刷新状态", Dock = DockStyle.Fill, Height = 30 };
        refreshIsSpeaking.Click += (_, _) => RefreshIsSpeaking();
        tbl.Controls.Add(refreshIsSpeaking, 4, 2);

        panel.Controls.Add(tbl);
        return panel;
    }

    private Control BuildInfoPanel()
    {
        var panel = new GroupBox { Text = "信息 & 工具 (Info & Tools)", Dock = DockStyle.Fill };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 8, RowCount = 1 };
        for (int i = 0; i < 8; i++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));

        _channelsLabel = new Label { Text = "Ch: ?", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
        tbl.Controls.Add(_channelsLabel, 0, 0);

        _sampleRateLabel = new Label { Text = "SR: ?", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
        tbl.Controls.Add(_sampleRateLabel, 1, 0);

        _bitDepthLabel = new Label { Text = "Bits: ?", TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
        tbl.Controls.Add(_bitDepthLabel, 2, 0);

        _isAutoPowerBtn = new Button { Text = "IsAutoPower", Dock = DockStyle.Fill, Height = 28 };
        _isAutoPowerBtn.Click += (_, _) => Log("IsAutoPowerSupported: " + Context.IsAutoPowerSupported());
        tbl.Controls.Add(_isAutoPowerBtn, 3, 0);

        _pollPauseBtn = new Button { Text = "PollPause", Dock = DockStyle.Fill, Height = 28 };
        _pollPauseBtn.Click += (_, _) => SafeCtxOp("PollPause", ctx => ctx.AvailabilityPollPause());
        tbl.Controls.Add(_pollPauseBtn, 4, 0);

        _pollResumeBtn = new Button { Text = "PollResume", Dock = DockStyle.Fill, Height = 28 };
        _pollResumeBtn.Click += (_, _) => SafeCtxOp("PollResume", ctx => ctx.AvailabilityPollResume());
        tbl.Controls.Add(_pollResumeBtn, 5, 0);

        _logFlushBtn = new Button { Text = "LogFlush", Dock = DockStyle.Fill, Height = 28 };
        _logFlushBtn.Click += (_, _) => { Logger.Flush(); Log("Logger flushed"); };
        tbl.Controls.Add(_logFlushBtn, 6, 0);

        _logShutdownBtn = new Button { Text = "LogShutdown", Dock = DockStyle.Fill, Height = 28 };
        _logShutdownBtn.Click += (_, _) => { Logger.Shutdown(); Log("Logger shut down"); };
        tbl.Controls.Add(_logShutdownBtn, 7, 0);

        panel.Controls.Add(tbl);
        return panel;
    }

    private Control BuildLogPanel()
    {
        var panel = new GroupBox { Text = "日志控制 (Logger)", Dock = DockStyle.Fill };
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

        tbl.Controls.Add(new Label { Text = "LogLevel:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);

        _logLevelCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _logLevelCombo.Items.AddRange(["Trace", "Debug", "Info", "Warn", "Error", "None"]);
        _logLevelCombo.SelectedIndex = 2;
        tbl.Controls.Add(_logLevelCombo, 1, 0);

        _logLevelSetBtn = new Button { Text = "SetLevel", Dock = DockStyle.Fill, Height = 28 };
        _logLevelSetBtn.Click += (_, _) =>
        {
            var level = (PrismLogLevel)_logLevelCombo.SelectedIndex;
            var prev = Logger.SetLevel(level);
            Log($"LogLevel set to {level} (previous: {(PrismLogLevel)prev})");
        };
        tbl.Controls.Add(_logLevelSetBtn, 2, 0);

        var refreshBtn = new Button { Text = "刷新音频信息", Dock = DockStyle.Fill, Height = 28 };
        refreshBtn.Click += (_, _) => RefreshAudioInfo();
        tbl.Controls.Add(refreshBtn, 3, 0);

        panel.Controls.Add(tbl);
        return panel;
    }

    // ─── Helpers ───────────────────────────────────────────────

    private static TrackBar CreateTrackBar(int defaultValue, EventHandler onScroll)
    {
        var tb = new TrackBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = defaultValue,
            TickFrequency = 10,
            Dock = DockStyle.Fill,
        };
        tb.Scroll += onScroll;
        return tb;
    }

    private void Log(string message)
    {
        var text = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        if (_logBox.IsHandleCreated)
        {
            _logBox.BeginInvoke(() =>
            {
                _logBox.AppendText(text);
                _logBox.ScrollToCaret();
            });
        }
        else
        {
            _logBox.AppendText(text);
        }
    }

    private static string OpResult(string name, Exception? ex = null)
    {
        if (ex == null) return $"{name} 成功";
        if (ex is PrismNotImplementedException) return $"{name}: 此操作不支持 [{ex.GetType().Name}]";
        return $"{name} 失败: [{ex.GetType().Name}] {ex.Message}";
    }

    private void SafeOp(string name, Action action)
    {
        if (_backend == null) { Log("错误: 请先选择一个后端并点击 [创建] 或 [Acquire]"); return; }
        try { action(); Log(OpResult(name)); }
        catch (Exception ex) { Log(OpResult(name, ex)); }
    }

    private void SafeCtxOp(string name, Action<Context> action)
    {
        try { action(_context); Log(OpResult(name)); }
        catch (Exception ex) { Log(OpResult(name, ex)); }
    }

    // ─── Backend Operations ─────────────────────────────────────

    private void LoadBackends()
    {
        try
        {
            _backendListBox.Items.Clear();
            var count = _context.BackendsCount;
            for (int i = 0; i < count; i++)
            {
                var id = _context.IdAt(i);
                var name = _context.NameOf(id);
                var priority = _context.PriorityOf(id);
                _backendListBox.Items.Add(new BackendItem(id, name, priority));
            }
            Log($"加载了 {count} 个后端");
        }
        catch (Exception ex)
        {
            Log($"加载后端失败: [{ex.GetType().Name}] {ex.Message}");
        }
    }

    private void CreateBackend(bool acquire)
    {
        if (_backendListBox.SelectedItem is not BackendItem item) { Log("请先选择一个后端"); return; }
        try
        {
            _backend?.Dispose();
            _backend = acquire ? _context.Acquire(item.Id) : _context.Create(item.Id);
            _activeBackendLabel.Text = $"当前: {item.Name} [{(acquire ? "Acquired" : "Created")}]";
            _activeBackendLabel.ForeColor = Color.DarkGreen;
            Log($"{(acquire ? "Acquire" : "Create")} 后端: {item.Name} (ID: 0x{(ulong)item.Id:X16})");
            RefreshAudioInfo();
            LoadVoices();
            RefreshIsSpeaking();
        }
        catch (Exception ex)
        {
            Log($"{(acquire ? "Acquire" : "Create")} 失败: [{ex.GetType().Name}] {ex.Message}");
        }
    }

    private void CreateBestBackend(bool acquire)
    {
        try
        {
            _backend?.Dispose();
            _backend = acquire ? _context.AcquireBest() : _context.CreateBest();
            _activeBackendLabel.Text = $"当前: {_backend.Name} [{(acquire ? "AcquiredBest" : "CreatedBest")}]";
            _activeBackendLabel.ForeColor = Color.DarkGreen;
            Log($"{(acquire ? "AcquireBest" : "CreateBest")}: {_backend.Name}");
            RefreshAudioInfo();
            LoadVoices();
            RefreshIsSpeaking();
        }
        catch (Exception ex)
        {
            Log($"{(acquire ? "AcquireBest" : "CreateBest")} 失败: [{ex.GetType().Name}] {ex.Message}");
        }
    }

    // ─── Speech Operations ──────────────────────────────────────

    private void SpeakToMemory_Click(object? sender, EventArgs e)
    {
        if (_backend == null) { Log("请先选择后端"); return; }
        try
        {
            _backend.SpeakToMemory(_speechText.Text, (data, ch, sr) =>
            {
                Log($"[SpeakToMemory 回调] 数据长度={data.Length} floats, 声道={ch}, 采样率={sr}");
            });
            Log("SpeakToMemory 请求已发送");
        }
        catch (Exception ex)
        {
            Log($"SpeakToMemory 失败: [{ex.GetType().Name}] {ex.Message}");
        }
    }

    // ─── Parameter Operations ───────────────────────────────────

    private void SetParam(string name, Action<float> setter, Label label, TrackBar trackBar)
    {
        if (_backend == null) return;
        var val = trackBar.Value / 100f;
        label.Text = val.ToString("F2");
        try { setter(val); }
        catch (Exception ex) { Log($"{name} 设置失败: [{ex.GetType().Name}] {ex.Message}"); }
    }

    // ─── Voice Operations ───────────────────────────────────────

    private void LoadVoices()
    {
        if (_backend == null) return;
        _voiceListBox.Items.Clear();
        try
        {
            _backend.RefreshVoices();
            var count = _backend.VoicesCount;
            for (int i = 0; i < count; i++)
            {
                var name = _backend.GetVoiceName(i);
                var lang = _backend.GetVoiceLanguage(i);
                _voiceListBox.Items.Add($"#{i}: {name} ({lang})");
            }
            Log($"加载了 {count} 个语音");
            RefreshCurrentVoice();
        }
        catch (Exception ex)
        {
            Log(OpResult("加载语音", ex));
        }
    }

    private void SelectVoice_Click(object? sender, EventArgs e)
    {
        if (_backend == null) return;
        if (_voiceListBox.SelectedItem is not string sel || !sel.StartsWith("#")) return;
        var id = int.Parse(sel.Split(':')[0].TrimStart('#'));
        try
        {
            _backend.Voice = id;
            Log($"选择语音: {sel}");
            RefreshCurrentVoice();
        }
        catch (Exception ex)
        {
            Log($"选择语音失败: [{ex.GetType().Name}] {ex.Message}");
        }
    }

    private void RefreshCurrentVoice()
    {
        if (_backend == null) return;
        try
        {
            var id = _backend.Voice;
            var name = _backend.GetVoiceName(id);
            var lang = _backend.GetVoiceLanguage(id);
            _currentVoiceLabel.Text = $"当前语音: #{id} {name} ({lang})";
        }
        catch { _currentVoiceLabel.Text = "当前语音: (获取失败)"; }
    }

    // ─── Metadata ───────────────────────────────────────────────

    private void RefreshAudioInfo()
    {
        if (_backend == null) return;
        try
        {
            _channelsLabel.Text = $"Ch: {_backend.Channels}";
            _sampleRateLabel.Text = $"SR: {_backend.SampleRate}";
            _bitDepthLabel.Text = $"Bits: {_backend.BitDepth}";
        }
        catch { }
    }

    private void RefreshIsSpeaking()
    {
        if (_backend == null) return;
        try
        {
            _isSpeakingLabel.Text = "IsSpeaking: " + _backend.IsSpeaking;
        }
        catch { _isSpeakingLabel.Text = "IsSpeaking: ?"; }
    }

    // ─── Helper Types ──────────────────────────────────────────

    private record BackendItem(BackendId Id, string Name, int Priority)
    {
        public override string ToString() => $"[{Priority}] {Name} (0x{(ulong)Id:X16})";
    }
}
