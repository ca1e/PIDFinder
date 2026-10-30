# PIDFinder

宝可梦伪随机数（RNG）操弄计算器：由种子直接推导宝可梦的 PID、个体值、闪光与性格。

## 支持场景

- **Gen3**：Method 1（GBA 世代 PID/个体生成）
- **Gen8 剑盾**：旷野可见宝可梦生成
- **Gen8 BDSP**：游走宝可梦生成

## 项目结构（2026-10-30 重构后）

| 项目 | 说明 |
|---|---|
| `PIDFinder.Lib` | 算法库（net10.0，无 UI 依赖）：Method1RNG / Overworld8RNG / Roaming8bRNG 的 `Generate`，CheckRule 规则集，RngScanner 并行扫描器 |
| `PIDFinder` | Windows WinForms 图形界面（net10.0-windows） |
| `PIDFinder.Tests` | xunit 回归测试（黄金样例 + 并行/串行一致性） |
| `PIDFinder.Bench` | 扫描性能基准工具 |

## 命令行快速上手

```bash
# 构建与测试（Linux / Windows 均可）
dotnet build PIDFinder.sln -c Release
dotnet test PIDFinder.Tests/PIDFinder.Tests.csproj -c Release

# 批量扫描（示例：Gen3 Method 1，起点 0x10000000 扫 100 万个种子）
dotnet run --project PIDFinder.Bench
```

`RngScanner.Scan(kind, startSeed, count, trainer, rules, parallel: true)` 支持三种生成方式的批量扫描，
`parallel: false` 可退化为单线程（低核数机器上通常更快，见 REFACTOR_PLAN.md M3 实测数据）。

## 依赖

- .NET 10 SDK
- PKHeX.Core 26.10.18

## 许可

MIT。PKHeX.Core 属于 PKHeX 项目（GPL-3.0）。
