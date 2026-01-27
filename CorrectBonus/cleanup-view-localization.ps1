param(
    [string]$ProjectRoot = "C:\Users\ugurk\source\repos\CorrectBonus\CorrectBonus"
)

$viewsResxPath = Join-Path $ProjectRoot "Resources\Views"

$commonKeys = @(
    "Active",
    "Passive",
    "Status",
    "Actions",
    "Search",
    "Filter",
    "Clear",
    "New",
    "Edit",
    "Delete",
    "Create",
    "Back",
    "Select",
    "NoRecord",
    "ConfirmDelete",
    "Activate",
    "Deactivate"
)

Write-Host "Localization Cleanup Tool"
Write-Host "Scanning path: $viewsResxPath"
Write-Host ""

$resxFiles = Get-ChildItem $viewsResxPath -Recurse -Filter "*.resx"

$matches = @()

foreach ($file in $resxFiles) {
    [xml]$xml = Get-Content $file.FullName
    foreach ($data in $xml.root.data) {
        if ($commonKeys -contains $data.name) {
            $matches += [PSCustomObject]@{
                File = $file.FullName
                Key  = $data.name
            }
        }
    }
}

if ($matches.Count -eq 0) {
    Write-Host "No Common keys found in view-level resx files."
    exit
}

Write-Host "FOUND OVERRIDES:"
$matches | Format-Table -AutoSize

Write-Host ""
$confirm = Read-Host "Type YES to remove these keys (backup will be created)"

if ($confirm -ne "YES") {
    Write-Host "Operation cancelled."
    exit
}

foreach ($group in $matches | Group-Object File) {

    $filePath = $group.Name
    $backupPath = "$filePath.bak"

    Copy-Item $filePath $backupPath -Force

    [xml]$xml = Get-Content $filePath
    foreach ($item in $group.Group) {
        $node = $xml.root.data | Where-Object { $_.name -eq $item.Key }
        if ($node) {
            [void]$xml.root.RemoveChild($node)
        }
    }

    $xml.Save($filePath)
    Write-Host "Cleaned: $filePath"
}

Write-Host ""
Write-Host "DONE. Backup files (*.bak) created."
