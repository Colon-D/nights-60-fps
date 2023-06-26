# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/nights.test.60fps/*" -Force -Recurse
dotnet publish "./nights.test.60fps.csproj" -c Release -o "$env:RELOADEDIIMODS/nights.test.60fps" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location
