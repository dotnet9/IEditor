# 架构说明

`IEditor` 采用简洁的分层结构：

- `IEditor.Core` 负责证件照尺寸模型与 Magick.NET 图像处理。
- `IEditor.App` 负责 Avalonia 界面与视图模型。
- `IEditor.Core.Tests` 负责尺寸换算与处理流程验证。

首个功能切片聚焦证件照处理：

1. 载入源图片
2. 选择 1 寸、2 寸或自定义尺寸
3. 选择白色、蓝色或自定义背景
4. 预览处理结果
5. 导出文件
