# PIDFinder

由有限种子预算扫描 RNG 相关数据，也可在受支持的编辑器模板上应用种子结果。当前属于 raw 种子相关性模式：不初始化完整遭遇，也不承诺生成的实体通过全部 PKHeX 合法性检查。

## 支持范围

| 算法 | 由 RNG 生成 | 模板及限制 |
|---|---|---|
| Method1 | PID、IV；性格由 PID 派生 | 应用结果要求原生 PK3；按物种处理普通特性与性别；不支持隐藏特性或现代迁移格式 |
| Overworld8 | EC、PID、IV、尺寸 | 应用结果要求剑/盾 PK8；保留模板性格、特性及性别，开始前校验规则与模板兼容 |
| Roaming8b | EC、修正后的 PID、IV、普通特性槽、尺寸 | 应用结果要求 BD/SP PB8，形态0的艾姆利多或克雷色利亚及其固定性别；只实现同步性格分支，保留模板性格；跳过种子 `0x7FFFFFFF` |

两个 Gen8 算法的 `flawlessIVCount` 默认3，可设0..6。该值必须来自实际遭遇信息或调用方的明确选择，不能从模板已有31的个体值数量推断。Gen8 raw 结果的 `Nature` 为 `-1`；Overworld raw `Ability` 也为 `-1`，表示此段 RNG 不产生该字段。不要用 `PID % 25` 或 PID 最低位替代缺失的 Gen8 字段。

`Generate`、`RngScanner.Scan` 和 `Search` 返回 raw 数据，没有模板上下文；如果规则要求该算法未生成的性格、特性或性别，raw 入口会明确拒绝，不能据此宣称模板字段已验证。UI 使用 `GenerationContext(template, kind, rules, flawlessIVCount)` 和 `TryGenerate(seed, out result)`，由上下文预检查保留字段并校验最终克隆实体。训练家 ID 来自模板，成功与否均不修改原模板；不兼容的格式、非法参数或不可能的固定模板条件抛出 `ArgumentException`，候选种子不匹配返回 `false`。兼容用的 `TryApplyFromSeed(ref pk, tid, sid, rules, seed)` 保留旧签名，但实际以模板的训练家 ID 为准。

## PKHeX 插件使用

运行 `buildplugin.bat`，将 `publish/plugins/PIDFinder` 复制到 PKHeX 可执行文件旁的 `plugins` 目录；只需要插件和 PIDFinder.Lib 两个 DLL，使用宿主提供的 PKHeX.Core。当前对应 PKHeX 26.10.18 / .NET 10。

从 PKHeX 的工具菜单打开“猎游者”。种类与来源版本取自当前编辑模板，选择兼容算法后输入十六进制种子；Lookup 只在所有条件匹配后写回编辑器。搜索从该种子按算法 Next 推进，受扫描上限约束。规则与模板在开始时复制，取消完成后才能重启，关闭窗口会先取消运行任务。Gen8 保证 V 数需要按实际遭遇选择，BDSP 当前仅支持同步分支。

原生窗口回归工具位于 `tests/PluginSmoke`，需要先构建 PKHeX，使用 `-p:PKHeXDirectory=<PKHeX输出目录>` 指定宿主程序集位置；其运行参数见该工具的 usage 输出。

## 扫描 API

```csharp
using PIDFinder.Lib;
using System.Threading;

var rules = new CheckRule { Ability = -1, Gender = -1 };
using var cancellation = new CancellationTokenSource();
var results = RngScanner.Scan(
    RngKind.Method1, startSeed: 0x10000000, count: 1000000,
    tid: 12345, sid: 54321, rules: rules,
    parallel: true, chunkSize: 4096, maxDegreeOfParallelism: -1,
    cancellationToken: cancellation.Token, maxResults: 100,
    flawlessIVCount: 3);

PkmResult? first = RngScanner.Search(
    RngKind.Method1, startSeed: 0x10000000, count: 1000000,
    tid: 12345, sid: 54321, rules: rules,
    traversal: SeedTraversal.AlgorithmNext,
    cancellationToken: cancellation.Token);
```

`Scan` 保留原有参数前部，始终按数值递增扫描 `count` 个种子，越过 `uint.MaxValue` 后模2^32回绕。先按遍历顺序保留前 `maxResults` 个匹配，返回值再按 `Seed` 数值排序；因此回绕时不能对已排序的全结果直接 `Take` 来复现限额结果。串并行模式保持一致。`maxResults` 默认 `int.MaxValue`，是返回结果的硬上限；0直接返回空。需要控制总结果内存时应显式传较小值。

并行扫描按有限批次处理，`chunkSize` 是块大小上限，内部每块至多4096候选，同时块数不超过逻辑处理器数及请求并行度。除已保留结果外，只保留当前批次的有限临时结果；不会为全范围或全部块分配结果表。`chunkSize` 必须大于0，`maxDegreeOfParallelism` 必须为-1或正数，实际并行度不超过逻辑处理器数。取消抛出 `OperationCanceledException`，不返回部分结果；已取消的 token 即使遇到0预算也会抛出。

`Search` 串行按指定策略检查至多 `count` 个候选并返回首匹配，未找到返回 `null`。`NumericIncrement` 同 `Scan`；`AlgorithmNext` 每次调用所选算法的 `Next(seed)`，与数值递增序列不同。两者都支持取消，不会无限搜索。`count=0` 不生成候选；参数仍会校验。

两个扫描入口都会调用 `rules.Snapshot()`，但快照并不能使跨线程读取 UI 安全。调用方必须在 UI 线程读取并校验控件，传入独立规则对象，并确保复制期间没有并发修改；后台不得读取控件。`GenerationContext` 还会克隆模板，条件更改只应影响下次搜索。

## 构建、测试与工具

需要 .NET 10 SDK 和 PKHeX.Core 26.10.18。Windows UI、插件及完整解决方案应在 Windows 上构建；纯库、测试、Bench 和 GoldenGen 可独立运行。

```bash
dotnet build PIDFinder.sln -c Release
dotnet test PIDFinder.Tests/PIDFinder.Tests.csproj -c Release
# 可选候选预算，默认100000；同时计时串行/并行并核对种子结果
dotnet run -c Release --project PIDFinder.Bench -- 100000
# 默认定位仓库 tests/golden.json；也可传显式输出路径
# 会覆盖目标文件，生成数据必须独立复核后才能接受为黄金基线
dotnet run -c Release --project tests/GoldenGen -- ./candidate-golden.json
```

Windows 可运行 `build.bat` 发布独立 WinForms 应用到 `publish/standalone`，目标框架为 `net10.0-windows`，不清理插件发布产物。Bench 使用 `Stopwatch` 计时，包含生成、过滤、收集与排序；用于简单本机比较，并非统计性能结论。

项目分工：`PIDFinder.Lib` 为纯算法与上下文库，`PIDFinder` 为独立 WinForms UI，`PKHeX_Hunter_Plugin` 为 PKHeX 插件，`PIDFinder.Tests` 为回归与扫描边界测试，`PIDFinder.Bench` 为计时工具，`tests/GoldenGen` 为开发用 raw 样例导出器。

## 许可

MIT。PKHeX.Core 属于 PKHeX 项目（GPL-3.0）。
