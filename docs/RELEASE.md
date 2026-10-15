# IEditor 发布流程

版本唯一来源为 `src/IEditor.App/IEditor.App.csproj`，版本标签必须匹配，如 `0.3.6` 对应 `v0.3.6`。精选发布说明放在 `.github/release-notes/v0.3.6.md`。

## 本机验证

```powershell
dotnet test IEditor.slnx -c Release
pwsh scripts/sync_localization_keys.ps1 -Check
node scripts/prototype/build.cjs --check
dotnet run --project scripts/ui-verification/UiVerification.csproj -c Release -- artifacts/ui-managed
pwsh scripts/publish.ps1 -RuntimeIdentifier win-x64
```

应用发布目录为 `artifacts/publish/win-x64/IEditor.App`。原生界面验证器也必须按 NativeAOT 发布并实际执行；它使用与应用相同的视图、视图模型和图像引擎，隔离图片、偏好和导出目录，不启动安装器。

```powershell
dotnet publish scripts/ui-verification/UiVerification.csproj -c Release -r win-x64 --self-contained true -p:PublishAot=true -p:PublishTrimmed=true -p:DebugType=portable -p:DebugSymbols=true -p:IlcGenerateCompleteTypeMetadata=true -p:IlcTrimMetadata=false -p:IlcSingleThreaded=true -o artifacts/ui-native/win-x64
& ./artifacts/ui-native/win-x64/UiVerification.exe artifacts/ui-evidence/win-x64
```

## 提交与版本标签

直接在 `main` 按变更范围使用简体中文规范提交。完成检查后推送 `main`，用精选发布说明文件创建新附注标签并推送。已发布标签不得移动。

```powershell
git push origin main
git tag -a v0.3.6 -F .github/release-notes/v0.3.6.md
git push origin v0.3.6
```

本地开发可用 `temp_nugets` 目录验证新包，不提交本地源配置或跨仓库项目引用。正式 CI 使用 nuget.org；共享包可用后再触发版本标签。

## CI 与安装包

测试任务检查 Core、T4、单文件原型离线交互及 managed 真实行为。随后五个原生 runner 分别执行仓库发布脚本，再原生发布并实际运行界面验证器，成功后打包。

| RID | Runner | 安装包 |
| --- | --- | --- |
| win-x64 | windows-latest | Inno Setup exe |
| linux-x64 | ubuntu-latest | amd64 deb |
| linux-arm64 | ubuntu-24.04-arm | arm64 deb |
| osx-x64 | macos-15-intel | x64 dmg |
| osx-arm64 | macos-15 | arm64 dmg |

发布资产仅包含上述安装包与 SHA-256 文件；界面证据作为独立 Actions artifact 保留。Release 说明优先精选文件，其次附注标签，禁止以缺失说明静默继续。发布后核对五个平台安装包、校验文件和全部工作流结果。
