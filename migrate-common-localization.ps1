# ================= START =================
Write-Host "SCRIPT STARTED"

# ================= CONFIG =================
$projectRoot = "C:\Users\ugurk\source\repos\CorrectBonus\CorrectBonus"
$viewsPath   = Join-Path $projectRoot "Views"

Write-Host "Project path: $projectRoot"
Write-Host "Views path  : $viewsPath"
Write-Host ""

if (-not (Test-Path $viewsPath)) {
    Write-Host "ERROR: Views folder not found!"
    Read-Host "Press ENTER to exit"
    exit
}

$commonInject = '@inject IStringLocalizer<CorrectBonus.Localization.Resources.CommonResource> Common'
$filesToProcess = @()

Write-Host "DRY RUN: scanning cshtml files..."
Write-Host ""

# ================= DRY RUN =================
Get-ChildItem -Path $viewsPath -Recurse -Filter *.cshtml | ForEach-Object {

    $filePath = $_.FullName
    Write-Host "Checking: $filePath"

    $content = Get-Content $filePath -Raw

    if ($content -match '@L\["Common\.') {
        Write-Host "MATCH FOUND: $filePath"
        $filesToProcess += $filePath
    }
}

Write-Host ""
Write-Host "------------------------------------"
Write-Host "Total matched files: $($filesToProcess.Count)"
Write-Host "------------------------------------"
Write-Host ""

if ($filesToProcess.Count -eq 0) {
    Write-Host "No files to process."
    Read-Host "Press ENTER to exit"
    exit
}

$confirm = Read-Host "Type YES to apply changes"

if ($confirm -ne "YES") {
    Write-Host "Operation cancelled."
    Read-Host "Press ENTER to exit"
    exit
}

Write-Host ""
Write-Host "APPLYING CHANGES..."

# ================= APPLY =================
foreach ($file in $filesToProcess) {

    Write-Host "Processing: $file"

    $content = Get-Content $file -Raw

    # Backup
    Copy-Item $file "$file.bak" -Force

    # Replace @L["Common.X"] -> @Common["X"]
    $content = $content -replace '@L\["Common\.([A-Za-z0-9_]+)"\]', '@Common["$1"]'

    # Inject Common localizer if missing
    if ($content -notmatch 'IStringLocalizer<.*CommonResource>') {

        if ($content -match '(@inject\s+IViewLocalizer\s+\w+)') {
            $content = $content -replace '(@inject\s+IViewLocalizer\s+\w+)',
                "`$1`r`n$commonInject"
        }
        else {
            $content = "$commonInject`r`n`r`n$content"
        }
    }

    Set-Content $file $content -Encoding UTF8
}

Write-Host ""
Write-Host "DONE. Backup files (*.bak) created."
Read-Host "Press ENTER to exit"
