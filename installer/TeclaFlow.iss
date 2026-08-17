#define MyAppName "TeclaFlow"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Christopher Hernandez"
#define MyAppExeName "TeclaFlow.exe"

[Setup]
AppId={{D07BA9B1-5B8E-4D66-91F4-8D12DC853390}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/chernandezhg/TeclaFlow
AppSupportURL=https://github.com/chernandezhg/TeclaFlow/issues
AppUpdatesURL=https://github.com/chernandezhg/TeclaFlow/releases
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Instalador de {#MyAppName}
VersionInfoProductName={#MyAppName}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist\installer
OutputBaseFilename=TeclaFlow-Setup-{#MyAppVersion}
SetupIconFile=..\TeclaFlow\Assets\teclaflow.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern
WizardSizePercent=110
WizardImageFile=Assets\wizard-side.bmp
WizardSmallImageFile=Assets\wizard-small.bmp
Compression=lzma2/ultra64
SolidCompression=yes
CloseApplications=yes
RestartApplications=no
UsePreviousAppDir=yes
ShowLanguageDialog=no
LanguageDetectionMethod=uilanguage

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"; Flags: unchecked

[Files]
Source: "..\dist\TeclaFlow\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard;
begin
  WizardForm.Color := $00FBF7F5;
  WizardForm.MainPanel.Color := clWhite;
  WizardForm.PageNameLabel.Font.Color := $00332018;
  WizardForm.PageNameLabel.Font.Style := [fsBold];
  WizardForm.PageDescriptionLabel.Font.Color := $00867068;
  WizardForm.WelcomeLabel1.Font.Color := $00332018;
  WizardForm.WelcomeLabel1.Font.Style := [fsBold];
  WizardForm.WelcomeLabel2.Font.Color := $00867068;
  WizardForm.FinishedHeadingLabel.Font.Color := $00332018;
  WizardForm.FinishedHeadingLabel.Font.Style := [fsBold];
  WizardForm.FinishedLabel.Font.Color := $00867068;
  WizardForm.NextButton.Caption := 'Siguiente  >';
  WizardForm.CancelButton.Caption := 'Cancelar';
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
end;
