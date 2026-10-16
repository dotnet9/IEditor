# 0.3.13 本地候选版本媒体复核

2026-10-16，Windows x64。PNG 从独立 NativeAOT UI 验证器捕获逐字节复制，绘制实际应用控件；全部图片、导出、偏好或工作流输入均在临时目录隔离，未执行安装器。

生产及验证源码在验证时尚未提交，冻结指纹 `3d3d8f5414d3a812589ca497f29bce298a83511f1c9df8375b271103a6c4faca`（66 个文件）。指纹规则为排序后 `路径 + NUL + 文件小写 SHA-256 + LF` 的 UTF-8 SHA-256，清单为 `E:/github/temp/recheck25-ieditor-source-manifest.json`。旧标签和历史日志不替代当前候选字节的验证。

验证完成后，生产修复提交为 `b547973de9cdc315aa2ab958de6ec6ef2ba7348d`，验证与原型提交为 `5a0aee39bb3a9677da04398fe2cae6b946cdba4c`。提交后的工作区冻结文件逐字节复核一致；此指纹不是 Git 树哈希。本文收口时未推送、打标签或公开发布。

| 媒体文件 | SHA-256 |
| --- | --- |
| `docs/images/photo-v0.3.13-light.png` | `19f36151d8c6cbef7737e060957579a46fbc6a1e8bd590a010d1e0f1c69b009f` |
| `docs/images/batch-v0.3.13-dark.png` | `f61be7223d35566b16ebd275d172934e1ec8f2f61a47042111837d01701c912e` |
| `docs/images/about-v0.3.13-zh-CN.png` | `2821086ebb64c385887c10da92482a3962d728aa3ca7df6f9a778cbfa6c4bd91` |

正式产品由仓库 `scripts/publish.ps1` 构建；实际 EXE 的隔离 `--verify-runtime` 已通过，版本 `0.3.13.0`。独立验证器实际执行：完整原生 UI 384/384 条断言通过，输出 `NativeAOT=True`。覆盖 关于更新与导出保存的真实主按钮，十六主题与 normal/hover/pressed/disabled/reenabled 五种状态；实际图像处理、尺寸输入、单张导出、批处理、语言和更新行为。本轮状态取色 160 条，启用关键状态最低对比度 4.8498；禁用态单独检查弱化反馈。

托管相同状态断言通过；测试 8/8、离线原型 96 套组合和定位检查通过。原型/原生公共区域的坐标对照见 `E:/github/temp/recheck25-ieditor-final-layout.log`，仅证明列出区域的 2 DIP 容差，不等于整页 95% 像素还原。

原 `.iss` 样式保留，基于逐文件核验且剔除 PDB/DBG 的临时 staging 编译 Windows 安装器，未执行。精确产品、验证器、安装包、包与日志哈希见 `E:/github/temp/recheck25-ieditor-final-receipt.json`。第三方裁剪/AOT 告警原样保留。尚未人工验证安装/升级/卸载、系统文件选择器、IME、多 DPI、跨屏或其他系统桌面操作。
