$projectPath = Join-Path (Get-Location) "Hospital.UI\Hospital.UI.csproj"

if (!(Test-Path $projectPath)) {
    Write-Host "Hospital.UI.csproj not found. Run this script from repository root." -ForegroundColor Red
    exit 1
}

[xml]$xml = Get-Content $projectPath
$project = $xml.Project
$propertyGroup = $project.PropertyGroup | Select-Object -First 1

if ($null -eq $propertyGroup) {
    $propertyGroup = $xml.CreateElement("PropertyGroup")
    $project.AppendChild($propertyGroup) | Out-Null
}

function Set-ProjectProperty($name, $value) {
    $node = $project.SelectSingleNode("//$name")
    if ($null -eq $node) {
        $node = $xml.CreateElement($name)
        $propertyGroup.AppendChild($node) | Out-Null
    }
    $node.InnerText = $value
}

Set-ProjectProperty "BlazorCacheBootResources" "false"
Set-ProjectProperty "DebugType" "embedded"

$xml.Save($projectPath)

Write-Host "Patched Hospital.UI.csproj." -ForegroundColor Green
Write-Host "Removing bin/obj folders..." -ForegroundColor Cyan
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force

Write-Host "Done. Now run restore/build." -ForegroundColor Green
