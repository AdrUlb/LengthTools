@echo off
pushd LengthTools.Compiler
dotnet publish -r win-x64 -c Release
popd

pushd LengthTools.Runtime
dotnet publish -r win-x64 -c Release
popd

rmdir /S /Q bin
mkdir bin
xcopy /e /q /y "LengthTools.Runtime\bin\Release\net5.0\win-x64\publish\*" bin\
xcopy /e /q /y "LengthTools.Compiler\bin\Release\net5.0\win-x64\publish\*" bin\
del bin\*.pdb
del bin\lengthc.deps.json
del bin\lengthc.runtimeconfig.json
del bin\lengthrt.deps.json
del bin\lengthrt.runtimeconfig.json