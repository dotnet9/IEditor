# IEditor 发布流程

版本唯一来源为 `src/IEditor.App/IEditor.App.csproj`，版本标签必须匹配，如 `0.3.9` 对应 `v0.3.9`。精选发布说明放在 `.github/release-notes/v0.3.9.md`。

## 本机验证

```powershell
dotnet test IEditor.slnx -c Release
pwsh scripts/sync_localization_keys.ps1 -Check
node scripts/prototype/build.cjs --check
dotnet run --project scripts/ui-verification/UiVerification.csproj -c Release -- artifacts/ui-managed
pwsh scripts/publish.ps1 -RuntimeIdentifier win-x64
$process = Start-Process ./artifacts/publish/win-x64/IEditor.App/IEditor.App.exe -ArgumentList '--verify-runtime', 'artifacts/runtime-verification/win-x64' -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'NativeAOT runtime verification failed.' }
```

应用发布目录为 `artifacts/publish/win-x64/IEditor.App`。原生界面验证器也必须按 NativeAOT 发布并实际执行；它使用与应用相同的视图、视图模型和图像引擎，隔离图片、偏好和导出目录，不启动安装器。

`--verify-runtime <独立目录>` 直接执行正式原生 EXE，检查发布目录的两份翻译及语言切换，并在指定目录使用实际配置服务完成偏好写入和回读。它不初始化窗口、日志和用户偏好；不替代下方的真实视图验收。五平台 CI 均执行此项。

```powershell
dotnet publish scripts/ui-verification/UiVerification.csproj -c Release -r win-x64 --self-contained true -m:1 -p:PublishAot=true -p:PublishTrimmed=true -p:DebugType=portable -p:DebugSymbols=true -p:IlcGenerateCompleteTypeMetadata=true -p:IlcTrimMetadata=false -p:IlcSingleThreaded=true -o artifacts/ui-native/win-x64
& ./artifacts/ui-native/win-x64/UiVerification.exe artifacts/ui-evidence/win-x64
```

## 提交与版本标签

直接在 `main` 按变更范围使用简体中文规范提交。完成检查后推送 `main`，用精选发布说明文件创建新附注标签并推送。已发布标签不得移动。

```powershell
git push origin main
git tag -a v0.3.9 -F .github/release-notes/v0.3.9.md
git push origin v0.3.9
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

发布前 `verify_release_installers.ps1` 严格检查五个平台各有一个非空、版本匹配的安装包及对应真实 SHA-256，拒绝 ZIP 和任何额外文件。PDB、DBG、dSYM 不进入安装包。macOS 翻译位于 `Contents/Resources/I18n`，默认 ad-hoc 签名或使用配置的 Developer ID；签名或严格验证失败会终止打包，ad-hoc 签名不等于 Apple 公证。

Linux deb 提供命令行启动器、桌面菜单和图标，安装容量按剔除调试符号后的真实文件计算。本机验证了 staging 结构、启动参数与剔符号行为；Windows 不执行真实 dpkg 或 macOS 签名，实际打包结果以对应原生 runner 为准。
