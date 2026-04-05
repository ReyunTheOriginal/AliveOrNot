# Auto-detect Assets folder
if (Test-Path ".\Assets") {
    $assetsPath = ".\Assets"
} else {
    $assetsPath = "."
}

# Get all C# files
$files = Get-ChildItem -Path $assetsPath -Recurse -Filter *.cs -ErrorAction SilentlyContinue

$totalFiles = $files.Count

if ($totalFiles -eq 0) {
    Write-Host "No C# files found. Check your path." -ForegroundColor Red
    return
}

$totalLines = 0
$totalNonEmptyLines = 0

$fileStats = @()

foreach ($file in $files) {
    $content = Get-Content $file.FullName

    $lineCount = $content.Count
    $nonEmptyCount = ($content | Where-Object { $_.Trim() -ne "" }).Count

    $totalLines += $lineCount
    $totalNonEmptyLines += $nonEmptyCount

    $fileStats += [PSCustomObject]@{
        Name          = $file.Name
        Lines         = $lineCount
        NonEmptyLines = $nonEmptyCount
        Path          = $file.FullName
    }
}

# Calculations
$averageLines = [math]::Round($totalLines / $totalFiles, 2)
$averageNonEmpty = [math]::Round($totalNonEmptyLines / $totalFiles, 2)

$largest = $fileStats | Sort-Object Lines -Descending | Select-Object -First 1
$smallest = $fileStats | Sort-Object Lines | Select-Object -First 1

$top5Largest = $fileStats | Sort-Object Lines -Descending | Select-Object -First 5
$top5Smallest = $fileStats | Sort-Object Lines | Select-Object -First 5

# Output
Write-Host ""
Write-Host "===== Code Stats =====" -ForegroundColor Cyan
Write-Host "Total Scripts: $totalFiles"
Write-Host "Total Lines (raw): $totalLines"
Write-Host "Total Lines (non-empty): $totalNonEmptyLines"
Write-Host "Average Lines per Script: $averageLines"
Write-Host "Average Non-Empty Lines: $averageNonEmpty"

Write-Host ""
Write-Host "Largest Script:"
Write-Host "$($largest.Name) - $($largest.Lines) lines"

Write-Host ""
Write-Host "Smallest Script:"
Write-Host "$($smallest.Name) - $($smallest.Lines) lines"

Write-Host ""
Write-Host "Top 5 Largest Scripts:"
$top5Largest | ForEach-Object {
    Write-Host "$($_.Name) - $($_.Lines) lines"
}

Write-Host ""
Write-Host "Top 5 Smallest Scripts:"
$top5Smallest | ForEach-Object {
    Write-Host "$($_.Name) - $($_.Lines) lines"
}

# Optional: Log progress over time
$logFile = "codestats_log.csv"

if (!(Test-Path $logFile)) {
    "Date,TotalScripts,TotalLines,NonEmptyLines,AverageLines" | Out-File $logFile
}

"$((Get-Date).ToString('yyyy-MM-dd')),$totalFiles,$totalLines,$totalNonEmptyLines,$averageLines" | Out-File $logFile -Append

Write-Host ""
Write-Host "Stats logged to $logFile"