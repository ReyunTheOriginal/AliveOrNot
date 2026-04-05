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
        Name  = $file.Name
        Lines = $lineCount
    }
}

$averageLines = [math]::Round($totalLines / $totalFiles, 2)

$top5Largest = $fileStats | Sort-Object Lines -Descending | Select-Object -First 5

# Build markdown
$statsBlock = @"
## Code Stats

- Total Scripts: $totalFiles
- Total Lines: $totalLines
- Non-Empty Lines: $totalNonEmptyLines
- Average Lines per Script: $averageLines

### Top 5 Largest Scripts
$(
    $top5Largest | ForEach-Object {
        "- $($_.Name): $($_.Lines) lines"
    } | Out-String
)
"@

# README path
$readmePath = "..\README.md"

if (!(Test-Path $readmePath)) {
    # Create new README if it doesn't exist
    $statsBlock | Out-File $readmePath
    Write-Host "README.md created with stats."
    return
}

# Read existing README
$readmeContent = Get-Content $readmePath -Raw

# Replace existing section or append
if ($readmeContent -match "## Code Stats") {
    $updatedContent = [regex]::Replace(
        $readmeContent,
        "## Code Stats[\s\S]*?(?=##|$)",
        $statsBlock
    )
} else {
    $updatedContent = $readmeContent + "`n`n" + $statsBlock
}

# Write back
$updatedContent | Out-File $readmePath

Write-Host "README.md updated with latest stats."