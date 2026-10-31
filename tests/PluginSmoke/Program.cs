using System.Diagnostics;
using System.Reflection;
using PKHeX.Core;
using PKHeX.WinForms;
using HostMain = PKHeX.WinForms.Main;

// Real Windows/STA integration test. No plugin ProjectReference and no simulated worker tasks.
internal static class Smoke
{
    private const uint Seed = 0x12345678;
    private const string SearcherName = "PKHeX_Hunter_Plugin.Searcher";
    private static Exception? uiException;
    private static int assertions;

    [STAThread]
    private static int Main(string[] args)
    {
        // Keep a real message loop alive: Nested DoEvents can tear down thread windows; async waits keep
        // continuations on the actual UI message loop.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        _ = HostMain.Settings; // Host static startup configures exception mode before any control exists.
        var handler = typeof(HostMain).Assembly.GetType("PKHeX.WinForms.Program")!.GetMethod("UIThreadException", BindingFlags.Static | BindingFlags.NonPublic)!;
        Application.ThreadException -= (System.Threading.ThreadExceptionEventHandler)handler.CreateDelegate(typeof(System.Threading.ThreadExceptionEventHandler));
        Application.ThreadException += (_, e) => { uiException = e.Exception; Console.Error.WriteLine(e.Exception); };
        int result = 1;
        bool runFinished = false;
        using var loop = new Form { ShowInTaskbar = false, Opacity = 0 };
        loop.Shown += (_, _) => loop.BeginInvoke((Action)(async () =>
        {
            try { result = await Run(args); }
            finally { runFinished = true; loop.Close(); }
        }));
        // PKHeX closes other windows while loading a save; keep the test message loop alive.
        loop.FormClosing += (_, e) => e.Cancel = !runFinished;
        Application.Run(loop);
        return result;
    }

    private static async Task<int> Run(string[] args)
    {
        if (args.Length is < 1 or > 3)
        {
            Console.Error.WriteLine("Usage: PluginSmoke <plugin-directory> [host-directory] [screenshot.png]");
            return 2;
        }
        // Also bounds a modal dialog or a hung UI call, which a UI-thread stopwatch cannot bound.
        using var watchdog = new System.Threading.Timer(_ =>
        {
            Console.Error.WriteLine("FAIL: whole smoke run exceeded 120 seconds (possibly blocked UI).");
            Environment.Exit(124);
        }, null, TimeSpan.FromSeconds(120), Timeout.InfiniteTimeSpan);
        try
        {
            Control.CheckForIllegalCrossThreadCalls = true;
            Application.ThreadException += (_, e) => uiException = e.Exception;
            string pluginDirectory = Path.GetFullPath(args[0]);
            string hostDirectory = Path.GetFullPath(args.Length > 1 ? args[1] :
                "D:/github/kwsch_PKHeX/PKHeX.WinForms/bin/Release/net10.0-windows/win-x64");
            Require(Directory.Exists(pluginDirectory), "plugin directory exists");
            Require(File.Exists(Path.Combine(hostDirectory, "PKHeX.dll")), "actual host exists");
            var discovered = new List<IPlugin>();
            var loaded = PluginLoader.LoadPlugins(pluginDirectory, discovered, false);
            Require(discovered.Count(p => p.Name == "猎游者") == 1, "PluginLoader discovers exactly one hunter");
            Console.WriteLine($"Host={typeof(HostMain).Assembly.Location}; Core={typeof(PKM).Assembly.Location}");

            HostMain.Settings.LocalResources.SetLocalPath(hostDirectory);
            HostMain.Settings.LocalResources.PluginPath = pluginDirectory;
            StartupUtil.ReloadSettings(HostMain.Settings);
            using var main = new HostMain();
            main.AttachPlugins();
            main.LoadInitialFiles(StartupUtil.GetStartup([], HostMain.Settings));
            main.Show();
            await PumpUntil(() => main.Visible, "actual Main.Show");
            var strip = Get<MenuStrip>(main, "menuStrip1");
            var tools = (ToolStripMenuItem)strip.Items.Find("Menu_Tools", false).Single();
            var hunter = tools.DropDownItems.OfType<ToolStripMenuItem>().Single(x => x.Text == "猎游者");
            Require(hunter.Enabled, "actual hunter menu enabled");
            hunter.PerformClick();
            await PumpUntil(() => OpenSearchers().Length == 1, "actual host menu opens Searcher");
            var hostSearcher = OpenSearchers().Single();
            Require(hostSearcher.Visible, "host Searcher visible");
            hunter.PerformClick();
            await Task.Delay(1);
            ThrowUIException();
            Require(OpenSearchers().Length == 1 && ReferenceEquals(hostSearcher, OpenSearchers()[0]),
                "repeated host menu clicks reuse one window");
            Require(hostSearcher.Controls.Find("BTN_Encounter", true).Length == 0, "placeholder encounter control absent");
            var searcherType = hostSearcher.GetType();
            var saveProvider = AllControls(main).OfType<ISaveFileProvider>().Single();
            hostSearcher.Close();
            await PumpUntil(() => hostSearcher.IsDisposed, "host menu window closes");
            hunter.PerformClick();
            await PumpUntil(() => OpenSearchers().Length == 1, "menu reopens after close");
            var reopened = OpenSearchers().Single();
            Require(!ReferenceEquals(hostSearcher, reopened), "closed menu window replaced");
            reopened.Close();
            await PumpUntil(() => reopened.IsDisposed, "reopened host window closes");

            foreach (string algorithm in new[] { "Method1", "Overworld8", "Roaming8b" })
                await RunScenario(searcherType, saveProvider, algorithm, args.Length > 2 ? args[2] : null);
            main.Close();
            GC.KeepAlive(loaded);
            ThrowUIException();
            Console.WriteLine($"SMOKE PASS: {assertions} assertions; actual host/menu, three algorithms, validation, worker completion/cancellation/restart/error/close.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"SMOKE FAIL after {assertions} assertions: {ex}");
            return 1;
        }
    }

    private static async Task RunScenario(Type type, ISaveFileProvider sav, string algorithm, string? screenshot)
    {
        PKM template = algorithm switch
        {
            "Method1" => new PK3 { Species = 1, Version = GameVersion.R, TID16 = 12345, SID16 = 54321 },
            "Overworld8" => new PK8 { Species = 1, Version = GameVersion.SW, Nature = (Nature)3,
                Gender = 0, Form = 0, AbilityNumber = 1, TID16 = 12345, SID16 = 54321 },
            "Roaming8b" => new PB8 { Species = 481, Version = GameVersion.BD, Nature = (Nature)3,
                Gender = 2, Form = 0, AbilityNumber = 1, TID16 = 12345, SID16 = 54321 },
            _ => throw new ArgumentException(algorithm),
        };
        template.RefreshAbility(0);
        var editor = new StubView(template);
        using var form = (Form)Activator.CreateInstance(type, [sav, editor])!;
        form.Show();
        await PumpUntil(() => form.Visible, $"{algorithm} native Searcher shown");
        Select(form, "CB_Method", algorithm);
        Reset(form);
        var expected = Expected(template, algorithm, Seed, 3);
        int before = editor.Writes;
        Get<Button>(form, "checkBTN").PerformClick();
        Require(editor.Writes == before + 1, $"{algorithm} Lookup writes once");
        EqualEntity(expected, editor.Data, $"{algorithm} Lookup");
        Console.WriteLine($"Lookup={algorithm}; Seed={Seed:X8}; PID={editor.Data.PID:X8}; IVs={string.Join(',', IVs(editor.Data))}");

        // Opposite gender and ability rules must never write the editor.
        Select(form, "CB_Gender", expected.Gender == 0 ? "Female" : "Male");
        await Reject(form, editor, $"{algorithm} opposite gender");
        Select(form, "CB_Gender", "Any");
        Select(form, "CB_Ability", expected.AbilityNumber == 1 ? "Ability 2" : "Ability 1");
        await Reject(form, editor, $"{algorithm} opposite ability");
        Select(form, "CB_Ability", "Any");

        foreach (var invalid in new[] { "32", "-1", "xx" })
        {
            Get<TextBox>(form, "HPMin").Text = invalid;
            await Reject(form, editor, $"{algorithm} invalid IV {invalid}", bothButtons: true);
        }
        Get<TextBox>(form, "HPMin").Text = "31";
        Get<TextBox>(form, "HPMax").Text = "30";
        await Reject(form, editor, $"{algorithm} min greater than max", bothButtons: true);
        Reset(form);
        Get<TextBox>(form, "seedBox").Text = "NOTHEX";
        await Reject(form, editor, $"{algorithm} nonhex seed", bothButtons: true);
        Reset(form);

        // Real Task.Run success, with immediate checks before pumping its UI continuation.
        before = editor.Writes;
        Get<NumericUpDown>(form, "NUD_Budget").Value = 1;
        Start(form, algorithm);
        await WaitIdle(form, $"{algorithm} background success");
        Require(editor.Writes == before + 1, $"{algorithm} background search writes once");
        EqualEntity(expected, editor.Data, $"{algorithm} background");
        Require(Status(form).Contains("已应用"), $"{algorithm} success status");

        // Reject the first seed with an independently known IV and scan exactly one seed.
        Get<NumericUpDown>(form, "NUD_Flawless").Value = 0;
        var zero = Expected(editor.Data, algorithm, Seed, 0);
        string excluded = zero.IV_HP == 0 ? "1" : "0";
        Get<TextBox>(form, "HPMin").Text = excluded;
        Get<TextBox>(form, "HPMax").Text = excluded;
        before = editor.Writes;
        var snapshot = editor.Data.Data.ToArray();
        Start(form, algorithm);
        await WaitIdle(form, $"{algorithm} exhausted search");
        Require(Status(form).Contains("未找到"), $"{algorithm} exhaustion status");
        Unchanged(editor, before, snapshot, $"{algorithm} exhaustion");

        // Highly restrictive but valid rules force real work. Cancellation is requested in the
        // same UI turn, so no successful worker continuation can write before cancellation.
        Reset(form);
        SetRareRules(form);
        Get<NumericUpDown>(form, "NUD_Budget").Value = 100000000;
        before = editor.Writes;
        snapshot = editor.Data.Data.ToArray();
        Start(form, algorithm);
        var cancelledTask = ObserveTask(form);
        Get<Button>(form, "cancelBTN").PerformClick();
        await WaitIdle(form, $"{algorithm} quick cancel", 5000);
        Require(cancelledTask.IsCompleted, $"{algorithm} cancelled real task completed");
        Require(Status(form).Contains("已取消"), $"{algorithm} cancelled status");
        Unchanged(editor, before, snapshot, $"{algorithm} cancel");
        Reset(form);
        Get<NumericUpDown>(form, "NUD_Budget").Value = 1;
        Start(form, algorithm);
        await WaitIdle(form, $"{algorithm} restart after cancellation");
        Require(editor.Writes == before + 1 && Status(form).Contains("已应用"), $"{algorithm} restart succeeds");

        editor.ThrowNextPopulate = true;
        before = editor.Writes;
        snapshot = editor.Data.Data.ToArray();
        Start(form, algorithm);
        await WaitIdle(form, $"{algorithm} editor error");
        Require(Status(form).Contains(StubView.InjectedError), $"{algorithm} reports editor error");
        Require(!editor.ThrowNextPopulate, $"{algorithm} injected error was exercised");
        Unchanged(editor, before, snapshot, $"{algorithm} failed populate");
        Start(form, algorithm);
        await WaitIdle(form, $"{algorithm} recovery after editor error");
        Require(editor.Writes == before + 1 && Status(form).Contains("已应用"), $"{algorithm} editor error recovery");

        if (screenshot != null && algorithm == "Method1")
        {
            string path = Path.GetFullPath(screenshot);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var image = new System.Drawing.Bitmap(form.Width, form.Height);
            form.DrawToBitmap(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height));
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            Require(File.Exists(path) && new FileInfo(path).Length > 0, "screenshot saved");
            Console.WriteLine($"Screenshot={path}");
        }

        SetRareRules(form);
        Get<NumericUpDown>(form, "NUD_Budget").Value = 100000000;
        before = editor.Writes;
        snapshot = editor.Data.Data.ToArray();
        Start(form, algorithm);
        var closingTask = ObserveTask(form);
        form.Close();
        Require(!form.IsDisposed, $"{algorithm} running close deferred");
        await PumpUntil(() => form.IsDisposed, $"{algorithm} close waits for real task", 5000);
        Require(closingTask.IsCompleted, $"{algorithm} closed only after task completed");
        Unchanged(editor, before, snapshot, $"{algorithm} closing cancellation");
        Console.WriteLine($"Scenario PASS: {algorithm}");
    }

    private static void Start(Form form, string label)
    {
        Console.WriteLine($"Start {label}: thread={Environment.CurrentManagedThreadId}, context={SynchronizationContext.Current?.GetType().Name ?? "null"}");
        Get<Button>(form, "BTN_Search").PerformClick();
        // A one-seed Task.Run can finish before its first await yields. The long cancel/close
        // scenarios separately demand an observable live task via ObserveTask.
        var task = form.GetType().GetField("activeTask", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form) as Task;
        if (task == null)
        {
            Require(Get<Button>(form, "BTN_Search").Enabled && !Get<Button>(form, "cancelBTN").Visible,
                $"{label} synchronous worker completion leaves UI idle");
            return;
        }
        Require(task != null, $"{label} has real active task");
        foreach (string name in new[] { "BTN_Search", "checkBTN", "CB_Method", "conditionPKM1", "seedBox", "NUD_Flawless", "NUD_Budget" })
            Require(!Get<Control>(form, name).Enabled, $"{label} disables {name} while running");
        Require(!Get<ComboBox>(form, "CB_Gender").Enabled && !Get<ComboBox>(form, "CB_Ability").Enabled,
            $"{label} nested rules disabled");
        Require(Get<Button>(form, "cancelBTN").Visible && Get<Button>(form, "cancelBTN").Enabled,
            $"{label} cancellation available");
    }

    // Reflection is observation only: never replace activeTask/cancellation or invoke the worker.
    private static Task ObserveTask(Form form) =>
        (Task)(form.GetType().GetField("activeTask", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(form)
            ?? throw new InvalidOperationException("Expected a live Searcher task."));

    private static async Task WaitIdle(Form form, string reason, int milliseconds = 10000)
    {
        await PumpUntil(() => Get<Button>(form, "BTN_Search").Enabled, reason, milliseconds);
        Require(!Get<Button>(form, "cancelBTN").Visible, $"{reason}: cancel hidden");
        foreach (string name in new[] { "checkBTN", "CB_Method", "conditionPKM1", "seedBox", "NUD_Flawless", "NUD_Budget" })
            Require(Get<Control>(form, name).Enabled, $"{reason}: restores {name}");
    }

    private static async Task Reject(Form form, StubView editor, string reason, bool bothButtons = false)
    {
        foreach (string button in bothButtons ? new[] { "checkBTN", "BTN_Search" } : new[] { "checkBTN" })
        {
            int before = editor.Writes;
            var snapshot = editor.Data.Data.ToArray();
            Get<Label>(form, "L_Status").Text = "sentinel";
            Get<Button>(form, button).PerformClick();
            await Task.Delay(1);
            ThrowUIException();
            Unchanged(editor, before, snapshot, $"{reason} / {button}");
            Require(Status(form) != "sentinel" && !string.IsNullOrWhiteSpace(Status(form)), $"{reason}: visible diagnostic");
            Require(Get<Button>(form, "BTN_Search").Enabled && !Get<Button>(form, "cancelBTN").Visible,
                $"{reason}: validation leaves UI idle");
        }
    }

    private static void Reset(Form form)
    {
        foreach (string stat in new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" })
        {
            Get<TextBox>(form, stat + "Min").Text = "0";
            Get<TextBox>(form, stat + "Max").Text = "31";
        }
        Select(form, "CB_Gender", "Any"); Select(form, "CB_Ability", "Any"); Select(form, "CB_Shiny", "Any");
        Get<TextBox>(form, "seedBox").Text = $"{Seed:X8}";
        Get<NumericUpDown>(form, "NUD_Flawless").Value = 3;
    }

    private static void SetRareRules(Form form)
    {
        foreach (string stat in new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" })
        {
            Get<TextBox>(form, stat + "Min").Text = "31";
            Get<TextBox>(form, stat + "Max").Text = "31";
        }
        Select(form, "CB_Shiny", "Square");
    }

    private static PKM Expected(PKM template, string algorithm, uint seed, int flawless)
    {
        var pk = template.Clone();
        if (algorithm == "Method1")
        {
            uint lo = LCRNG.Next(seed) >> 16;
            pk.PID = (LCRNG.Advance(seed, 2) & 0xFFFF0000) | lo;
            uint a = LCRNG.Advance(seed, 3) >> 16, b = LCRNG.Advance(seed, 4) >> 16;
            pk.IV_HP = (int)(a & 31); pk.IV_ATK = (int)((a >> 5) & 31); pk.IV_DEF = (int)((a >> 10) & 31);
            pk.IV_SPE = (int)(b & 31); pk.IV_SPA = (int)((b >> 5) & 31); pk.IV_SPD = (int)((b >> 10) & 31);
            pk.RefreshAbility((int)(pk.PID & 1));
        }
        else if (algorithm == "Overworld8")
        {
            var method = typeof(PKHeX.Core.Overworld8RNG).GetMethod("TryApplyFromSeed", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new MissingMethodException("Actual Core Overworld8RNG.TryApplyFromSeed not found.");
            Require((bool)method.Invoke(null, [pk, EncounterCriteria.Unrestricted, Shiny.Random, flawless, seed])!, "Core Overworld reference generated");
        }
        else
            Require(PKHeX.Core.Roaming8bRNG.TryApplyFromSeed((PB8)pk, EncounterCriteria.Unrestricted, Shiny.Random, flawless, seed), "Core Roaming reference generated");
        pk.RefreshChecksum();
        return pk;
    }

    private static void EqualEntity(PKM expected, PKM actual, string reason)
    {
        Require(actual.GetType() == expected.GetType(), $"{reason}: native format preserved");
        Require(actual.PID == expected.PID, $"{reason}: PID expected {expected.PID:X8}, actual {actual.PID:X8}");
        Require(actual.EncryptionConstant == expected.EncryptionConstant, $"{reason}: EC");
        Require(IVs(actual).SequenceEqual(IVs(expected)), $"{reason}: all six IVs");
        Require(actual.TID16 == expected.TID16 && actual.SID16 == expected.SID16, $"{reason}: trainer IDs preserved");
        Require(actual.Nature == expected.Nature && actual.Gender == expected.Gender, $"{reason}: nature and gender");
        Require(actual.Ability == expected.Ability && actual.AbilityNumber == expected.AbilityNumber, $"{reason}: ability");
        Require(actual.Species == expected.Species && actual.Form == expected.Form && actual.Version == expected.Version, $"{reason}: template context");
        Require(actual.ShinyXor == expected.ShinyXor && actual.ChecksumValid, $"{reason}: shiny xor and checksum");
        if (actual is IScaledSize a && expected is IScaledSize e)
            Require(a.HeightScalar == e.HeightScalar && a.WeightScalar == e.WeightScalar, $"{reason}: size scalars");
    }

    private static int[] IVs(PKM pk) => [pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE];
    private static void Unchanged(StubView editor, int writes, byte[] data, string reason)
    {
        Require(editor.Writes == writes, $"{reason}: no editor writes");
        Require(editor.Data.Data.SequenceEqual(data), $"{reason}: editor bytes unchanged");
    }
    private static string Status(Form form) => Get<Label>(form, "L_Status").Text;
    private static Form[] OpenSearchers() => Application.OpenForms.Cast<Form>().Where(f => f.GetType().FullName == SearcherName).ToArray();
    private static IEnumerable<Control> AllControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var descendant in AllControls(child)) yield return descendant;
        }
    }
    private static T Get<T>(Control parent, string name) where T : Control =>
        (T)parent.Controls.Find(name, true).Single();
    private static void Select(Control parent, string name, string value)
    {
        var combo = Get<ComboBox>(parent, name);
        int index = combo.Items.Cast<object>().Select((item, i) => (item, i)).Single(x => x.item.ToString() == value).i;
        combo.SelectedIndex = index;
        Require(combo.SelectedItem?.ToString() == value, $"select {name} by name: {value}");
    }
    private static async Task PumpUntil(Func<bool> condition, string reason, int milliseconds = 10000)
    {
        var clock = Stopwatch.StartNew();
        while (!condition())
        {
            ThrowUIException();
            if (clock.ElapsedMilliseconds >= milliseconds)
                throw new TimeoutException($"FAIL: {reason} exceeded {milliseconds} ms.");
            await Task.Delay(1);
        }
        ThrowUIException();
    }
    private static void ThrowUIException()
    {
        if (uiException != null) throw new InvalidOperationException("Unhandled UI exception", uiException);
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"ASSERT FAILED: {message}");
        assertions++;
    }

    private sealed class StubView(PKM initial) : IPKMView
    {
        private readonly int uiThread = Environment.CurrentManagedThreadId;
        private PKM data = initial.Clone();
        public const string InjectedError = "injected PopulateFields failure";
        public int Writes { get; private set; }
        public bool ThrowNextPopulate { get; set; }
        public PKM Data { get { CheckThread(); return data; } }
        public bool Unicode => true;
        public bool HaX => false;
        public bool ChangingFields { get; set; }
        public bool EditsComplete => true;
        public PKM PreparePKM(bool click = true) => Data;
        public void NotifyWasExported(PKM pk) => CheckThread();
        public void PopulateFields(PKM pk, bool focus = true, bool skipConversionCheck = false)
        {
            CheckThread();
            if (ThrowNextPopulate)
            {
                ThrowNextPopulate = false;
                throw new InvalidOperationException(InjectedError);
            }
            data = pk.Clone();
            Writes++;
        }
        private void CheckThread() => Require(Environment.CurrentManagedThreadId == uiThread, "editor access stays on UI thread");
    }
}
