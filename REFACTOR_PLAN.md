# PIDFinder 重构计划（2026-10-30 定）

目标：完成 ①合并重复实现 ②跨平台+CLI ③并行性能 ④UI 现代化，算法行为保持不变。

## 原则
- 行为不变：每阶段用"黄金样例"回归兜底（固定 seed → 期望 PID/IV/闪光判定）
- 小步提交，每阶段独立可交付；技术栈维持 C#/.NET，不换语言
- PKHeX_Hunter_Plugin 最后再适配，前四个阶段不动它

## M1 解耦与去重（约 1 天）
- [x] 删除 PIDFinder/RNG、PIDFinder/Algs 与 PIDFinder.Lib 的重复实现，统一进 Lib
- [x] CheckRule 的 9 连 if 收敛为规则集合（谓词集合），UI/CLI 共用
- [x] Form1 只依赖 Lib 公共 API
- [x] 建黄金样例测试（Gen3 M1、Gen8 Overworld、BDSP Roaming 各取已知样本）
验收：测试全绿，RNG 实现全局唯一。

## M2 跨平台（2026-10-30 完成；主人取消 CLI）
- 2026-10-30 追加：PKHeX.Core 22.3.18 → 26.10.18（新版仅 net10.0），全解决方案 TFM 升至 net10/net10.0-windows；
  适配点：RNG.LCRNG→LCRNG、ITrainerID(TID/SID)→ITrainerID16(TID16/SID16 ushort)、PersonalTable 转静态类、
  SearchSettings.GetVersions 返回 ReadOnlyMemory、IEncounterInfo.EggEncounter→IsEgg；
  EncounterUtil（PKHeX 库搜索辅助，核心流程未使用）随 API 变化退役待 M5 重估。
- 黄金样例在 PKHeX 26 下回归通过，RNG 行为与 22.3.18 完全一致。
- [x] Lib 升级 net8.0→net10.0（跨平台；已加 EnableWindowsTargeting 兼容 Linux CI）
- [x] ~~CLI~~（2026-10-30 主人取消；后续如需批处理可在 CLI 基础上加，接口已就绪 RngScanner.Scan）
验收：Lib 在 Linux net8.0 构建通过，测试全绿。

## M3 性能并行化（2026-10-30 完成）
- [x] RngScanner.Scan：种子分块 + Parallel.For，分片本地 List 收集（无锁），结果按种子排序
- [x] Method1 热路径：闪光/性格/特性等廉价检查前置，失败提前跳过 IV 计算与对象分配
- [x] Stopwatch 基准（2 逻辑核，Gen3 扫 2^24 种子）：单线程 5.36M seeds/s（3.1s）＞并行 2.14M seeds/s（7.9s）
- 实测结论：2 核机器上 GC/调度开销大于并行收益，单线程已是最快路径（~13 分钟可扫完 2^32 全空间）；
  RngScanner 保留供多核机器使用（≥4 物理核时收益为正），结果与串行一致（ScanTests 验证）。
验收：并行与串行结果一致；基准数据如上，按机器核数选用。

## M4 UI 现代化（约 2-3 天）
- [ ] 方案 A：Avalonia 跨平台桌面（复用 Lib）
- [ ] 方案 B：ASP.NET Core 最小 API + 单页前端（浏览器访问，与 CLI 同核）
- [ ] WinForms 退役
验收：Linux 下完成完整扫描流程。

## M5 收尾（2026-10-31 完成）
- [x] GitHub Actions：linux+windows 双平台 build+test
- [x] README 重写
- [x] PKHeX_Hunter_Plugin 适配 PKHeX 26：
  - GameInfo.Species/Version/NatureDataSource → GameInfo.Sources.*；
  - SaveFile.Game → SaveFile.Version；SaveFile 直接以 TID16/SID16 传入 Lib（ITrainerIds 适配接口保留）；
  - EncounterUtil.SearchDatabase/GetCriteria 已随 PKHeX 26 退役：Searcher 改为直接以当前编辑数据为模板搜索种子（BTN_Encounter 提示废弃）；
  - WFO1000 分析器错误经 NoWarn 压制；Plugin 重新加入 sln，构建通过
  错误映射清单：GameInfo.SpeciesDataSource/VersionDataSource/NatureDataSource 已在新版移除（反射确认 GameInfo 无 DataSource 属性，需迁移到新数据源 API）；
  SaveFile.Game → SaveFile.Version（GameVersion 类型，需显式转换）；
  EncounterUtil.SearchDatabase/GetCriteria 已在 PKHeX 26 适配中退役，Plugin Searcher.cs 依赖它们，需用新 Encounter API 重写或恢复该方法；
  TryApplyFromSeed/Next 的传参需 SaveFile 实现 PIDFinder.Lib.ITrainerIds（PK8 体系 TID16/SID16，SaveFile 大概率已满足，编译期验证）；
  Controls 系 WFO1000 分析器错误已在 csproj 加 NoWarn 压制

## 环境准备
- cachyos：dotnet-sdk 8.x、xunit、BenchmarkDotNet
- 黄金样例：PKHeX.Core 已知结果 + 实机数据对照

## 分工建议
- M1/M2 喵月先行、主人 review；M3 并行推进；M4 的 UI 形态主人拍板后再动工
