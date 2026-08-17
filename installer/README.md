# Instalador de TeclaFlow

El instalador se genera con Inno Setup 6 y empaqueta la publicación autónoma de Windows x64.

```powershell
dotnet publish .\TeclaFlow\TeclaFlow.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false --output .\dist\TeclaFlow
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" .\installer\TeclaFlow.iss
```

El resultado se guarda en `dist\installer\TeclaFlow-Setup-1.0.0.exe`.
