; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "2chライク掲示板 ブラウザ(仮)"
#define MyAppVerName "2chライク掲示板 ブラウザ(仮)"
#define MyAppExeName "2chBrowser.exe"

[Setup]
BackColorDirection=lefttoright
BackColor2=$00F8A57C
BackColor=$00FC7745
AppName={#MyAppName}
AppId=garakuta_2chlikebbsBrowser
AppVerName={#MyAppVerName}
AppVersion=0.0.1.20160828
DefaultDirName={pf}\2chlikebbsBrowser
DefaultGroupName={#MyAppName}
OutputBaseFilename=2chlikebbsBrowser_setup
SolidCompression=Yes

[Languages]
Name: english; MessagesFile: compiler:Default.isl
Name: japanese; MessagesFile: compiler:Languages\Japanese.isl

[Code]

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}

[Files]
Source: .\bin\Release\*; Excludes:".\*vshost*, *.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: .\ver.txt; DestDir: "{app}";

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {userdesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: desktopicon; WorkingDir: {app}
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}";
