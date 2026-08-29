<p align="center">
  <img src="logo.svg" width="96" alt="IEditor logo" />
</p>

# 证件照处理

这是一个基于 Avalonia 的桌面证件照处理软件。

## 功能范围

- 1 寸证件照
- 2 寸证件照
- 自定义尺寸输入
- 白色、蓝色或自定义背景
- 预览与导出

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
