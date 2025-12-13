# BUILD APP

# Navitage to the solution folder:
$scriptFolder = if ($PSScriptRoot -eq "") { "." } else { $PSScriptRoot } # Support PSv5/Core running from script file or copy/pasted into a prompt.
cd "$scriptFolder/.."
Write-Host -F Blue "Changed dir to: '$PWD'"


$publishFolder = "Publish"
$publishFolderApp = "$publishFolder/app"

# CLEANUP:
Write-Host -F Blue "Cleaning up folders: '$publishFolderApp' and '$publishFolderDotnetTool'..."
if (test-path $publishFolderApp) { Remove-Item "$publishFolderApp/*" -Force -Recurse; Write-Host -F Yellow "Removed folder: $publishFolderApp" }
Write-Host -F Blue "Starting compilation..."

# COMPILE AS EXE:
# https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
Write-Host -F Blue "Publishing app..."
dotnet publish "src/Authenticator.UI/Authenticator.UI.csproj" `
--self-contained false `
-o $publishFolderApp `
-c "Release" `
-p:DebugType=None `
-p:EnableCompressionInSingleFile=false `
-p:PublishTrimmed=false `
-p:PublishSingleFile=true 
# -r "win-x64" 
Write-Host -F Green "Publishing app DONE!"


# COMPRESS TO ZIP:
$version = (Get-Item "$publishFolderApp/Authenticator.exe").VersionInfo.FileVersion
$destinationZip = "$publishFolder/Authenticator-v$version.zip"
Write-Host -F Blue "Compressing binaries into: '$destinationZip'..."
# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.archive/compress-archive?view=powershell-7.3
Compress-Archive -Path "$publishFolderApp/*" -DestinationPath $destinationZip -Force


Start $publishFolder
Write-Host -F Green "Finished Build!"
