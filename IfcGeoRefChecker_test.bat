REM lese dateien aus einem Ordner aus
REM filter nicht-ifc dateien aus
REM schreibe jeden Pfad der IFC-Dateien in die JSON
REM{
REM    "InputObjects":
REM    [
REM      {
REM        "fileName": "...\\ifc_Quadrat.ifc"
REM      },
REM      {
REM        "fileName": "...\\ifc_Quadrat_L30.ifc"
REM      },
REM    ...
REM    ],
REM    "outputDirectory": "...\\Output",
REM    "outLog": true,
REM    "outJson": false
REM  }

@ECHO OFF

REM Kommandozeilenfenster leeren
CLS
TITLE Write Json
CHCP 1252

SETLOCAL EnableDelayedExpansion

SET "yourExt=*.ifc"

REM schreibe json file
SET json={
SET json=%json%"InputObjects":
SET json=%json%[


REM Finde Pfad der INPUT Ordner
FOR /f "eol=# delims=& tokens=2" %%a in ('findstr /b /l /v "EXE OUTPUT JSONOUTPUT LOGOUTPUT" batch-test_settings.txt') do ( 
    SET jsondir=%%a
    PUSHD %%a
    FOR %%a in (*%yourExt%) do SET json=!json!{"fileName": "!jsondir:\=\\!%%a"},
    POPD
)
SET json=%json%],


REM Finde Pfad zum Output
FOR /f "eol=# delims=& tokens=2" %%a in ('findstr /b /l /v "EXE INPUT JSONOUTPUT LOGOUTPUT " batch-test_settings.txt') do ( 
    SET LogOutputdir="%%a"
)
SET jsonLogOutputdir=%LogOutputdir:\=\\%
SET json=%json%"outputDirectory": %jsonLogOutputdir%,


REM Finde Log-output true/false einstellung
FOR /f "eol=# delims=& tokens=2" %%a in ('findstr /b /l /v "EXE INPUT OUTPUT JSONOUTPUT" batch-test_settings.txt') do ( 
    SET outlog=%%a
)
SET json=%json%"outLog": %outlog%,


REM Finde Json-output true/false einstellung
FOR /f "eol=# delims=& tokens=2" %%a in ('findstr /b /l /v "EXE INPUT OUTPUT LOGOUTPUT" batch-test_settings.txt') do ( 
    SET outjson=%%a
)
SET json=%json%"outJson": %outjson%


SET json=%json%}
SET json=%json:},]=}]%
ECHO %json% > "test.json"
ENDLOCAL

REM Finde Pfad zur Exe
FOR /f "eol=# delims=& tokens=2" %%a in ('findstr /b /l /v "OUTPUT INPUT JSONOUTPUT LOGOUTPUT " batch-test_settings.txt') do ( 
    SET exe="%%a"
)


%exe% "test.json"

REM Es wird der Text "Taste drücken zum Beenden" ausgegeben und gewartet bis der Benutzer eine Taste drückt
ECHO Taste drücken zum Beenden
PAUSE > NUL