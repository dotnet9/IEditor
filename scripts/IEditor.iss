; IEditor Windows installer.
; Build from the repository root with Inno Setup 6 and pass /DAppVersion=x.y.z.

#ifndef AppVersion
#define AppVersion "0.0.0"
#endif

#ifndef SourceDir
#define SourceDir "..\artifacts\publish\win-x64\IEditor"
#endif

#ifndef OutputDir
#define OutputDir "..\artifacts\release"
#endif

[Setup]
AppId={{E7A1C2D4-3B5F-4A6E-9C8D-0B1A2C3D4E5F}}
AppName=IEditor
AppVersion={#AppVersion}
AppPublisher=Dotnet9
AppPublisherURL=https://github.com/dotnet9/IEditor
AppSupportURL=https://github.com/dotnet9/IEditor/issues
DefaultDirName={autopf}\IEditor
DefaultGroupName=IEditor
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=IEditor-v{#AppVersion}-win-x64-setup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\IEditor"; Filename: "{app}\IEditor.App.exe"
Name: "{autodesktop}\IEditor"; Filename: "{app}\IEditor.App.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Run]
Filename: "{app}\IEditor.App.exe"; Description: "{cm:LaunchProgram,IEditor}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
