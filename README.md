<p align="center">
  <img src="logo.svg" width="96" alt="IEditor 标志" />
</p>

# 证件照处理

这是一个基于 Avalonia 的桌面证件照处理软件。

## 功能范围

- 证件照单张处理：1 寸 / 2 寸 / 自定义尺寸（毫米），白色、标准蓝、中国红或自定义背景，实时预览、旋转镜像、参考线与原图对比
- 批量处理：队列 + 拖放导入、统一尺寸/背景、文件名模板、并发导出与 ZIP 打包
- 设置：外观主题色、默认 DPI、参考线开关、处理引擎与默认保存位置
- 更新检查：帮助菜单手动检查，设置页可开启每周自动检查
- 智能抠图：即将上线（界面就绪，算法接入中）

## 界面原型

- `design/index.html`：原型总览与设计规范（深色壁纸 + 圆角自绘窗口呈现）
- 功能原型页：`design/证件照处理.html`、`design/批量处理.html`、`design/设置.html`

## 目录结构

- `src/IEditor.Core`：核心模型与图像处理
- `src/IEditor.App`：界面与视图模型
- `tests/IEditor.Core.Tests`：核心逻辑测试

## 运行

```bash
dotnet run --project src/IEditor.App/IEditor.App.csproj
```

## 发布

标准发布流程与发布说明规范见 [docs/RELEASE.md](docs/RELEASE.md)。
