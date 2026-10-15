# IEditor 交互原型

唯一入口为 [index.html](index.html)。该文件包含证件照、批量处理、设置、关于及更新状态；CSS、JavaScript、图标和两种语言资源均已内嵌。可单独复制到空目录，断网后直接打开。

通过「工作区」菜单切换页面，也可使用 `#photo`、`#batch`、`#settings` 路由。标题栏提供浅色、深色、跟随系统及八套主题色；中文和英文可即时切换。导出对话框、质量滑杆、参考线、背景颜色及设置控件可交互。更新流程是受控状态演示，不会下载或安装程序。

## 维护

可编辑源位于 `scripts/prototype/source/`，不直接编辑生成的 `design/index.html`。语言资源源自宿主 `src/IEditor.App/I18n/*.json`，C# 强类型由 `Language.tt` 生成。

```powershell
pwsh scripts/sync_localization_keys.ps1
node scripts/prototype/build.cjs
node scripts/prototype/build.cjs --check
npm ci --prefix scripts/prototype/tests
cd scripts/prototype/tests
npx playwright install chromium
npm test
```

自动审计将单文件复制到隔离目录，以断网浏览器遍历 3 个工作区 × 2 种语言 × 8 套配色 × 2 种明暗状态，检查页面内容、外部请求、JavaScript 错误、翻译残留及关键交互。实际应用的图像处理、持久化和更新状态另由 `scripts/ui-verification` 验证。

原型批量队列使用固定样例，应用启动时为空队列。浏览器与原生控件的字体抗锯齿、原生窗口按钮和滚动条存在平台差异；不将交互审计通过当作像素一致结论。逐页复核范围见 [界面验证记录](../docs/UI-VERIFICATION.md)。

标题栏使用 Ursa 风格三态按钮、八个配色圆点和真实语言菜单，设置页保留完整外观表单；审计检查三态循环、语言菜单开闭及即时切换，不重复放置标题栏下拉框。
