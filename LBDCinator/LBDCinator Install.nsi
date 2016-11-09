; LBDCinator Install.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,

;--------------------------------

; The name of the installer
Name "LBDCinator Installer"

; The file to write
OutFile "LBDCinatorInstall.exe"

; The default installation directory
InstallDir C:\ProgramData\Autodesk\Revit\Addins\2015

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\Revit Plugins - LBDCinator" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------

; Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

; The stuff to install
Section "LBDCinator"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File "bin\Debug\LBDCinator.addin"
  File "bin\Debug\LBDCinator.dll"
  
  
SectionEnd


;--------------------------------

