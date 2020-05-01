@ECHO OFF
setlocal EnableDelayedExpansion
SET PROTO_PATH=Centrifugo.Client/Protocol/Schemas
SET OUTPUT_PATH=Centrifugo.Client/Protocol/Models

FOR %%i IN (./Centrifugo.Client/Protocol/Schemas/*.proto) DO (
    CALL SET "files=%%files%% %PROTO_PATH%/%%i"
    ECHO "%PROTO_PATH%/%%i"
) 

protoc-3.11.4.0.exe --proto_path=%PROTOPATH% --csharp_out=%OUTPUT_PATH% --csharp_opt=file_extension=.designer.cs %files%

pause 