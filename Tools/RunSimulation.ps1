# Runs balance-simulation games in parallel using the Windows build's hidden -simulate mode,
# merges the results and prints a summary.
#
#   powershell -ExecutionPolicy Bypass -File Tools\RunSimulation.ps1                 # 5 games, 5 at once
#   powershell -ExecutionPolicy Bypass -File Tools\RunSimulation.ps1 -Games 40 -Processes 8
#
# Build first (Build > Windows in the editor, or BuildScript.BuildWindows). Each game uses a fresh
# in-memory profile (Soldier, no shop upgrades) and never touches your saved progress.
param(
    [int]$Games = 5,
    [int]$Processes = 5,
    [int]$FirstSeed = 1,
    [float]$MaxTime = 720,
    [string]$Exe = (Join-Path $PSScriptRoot "..\Builds\Windows\ZombieSurvival.exe")
)

$ErrorActionPreference = "Stop"
$Exe = (Resolve-Path $Exe).Path
$outDir = Join-Path (Split-Path $Exe) "Simulation"
New-Item -ItemType Directory -Force $outDir | Out-Null
Get-ChildItem $outDir -Filter "part*.jsonl" | Remove-Item

$perProcess = [math]::Ceiling($Games / $Processes)
$lastSeed = $FirstSeed + $Games - 1
$players = @()
$seed = $FirstSeed
$index = 0
$started = Get-Date
while ($seed -le $lastSeed) {
    $end = [math]::Min($seed + $perProcess - 1, $lastSeed)
    $part = Join-Path $outDir "part$index.jsonl"
    $log = Join-Path $outDir "log$index.txt"
    $arguments = @("-batchmode", "-nographics", "-simulate", "-seeds", "$seed-$end",
                   "-out", "`"$part`"", "-maxTime", "$MaxTime", "-logFile", "`"$log`"")
    $players += Start-Process -FilePath $Exe -ArgumentList $arguments -PassThru -WindowStyle Hidden
    Write-Host "Started games $seed-$end"
    $seed = $end + 1
    $index++
}
$players | Wait-Process
$elapsed = (Get-Date) - $started

$results = Join-Path $outDir "results.jsonl"
Get-Content (Join-Path $outDir "part*.jsonl") | Where-Object { $_.Trim() } | Set-Content $results
$runs = @(Get-Content $results | ForEach-Object { $_ | ConvertFrom-Json } | Sort-Object seed)
if ($runs.Count -eq 0) { throw "No results; check $outDir\log*.txt" }

function Format-Time([double]$seconds) { "{0}:{1:00}" -f [math]::Floor($seconds / 60), [math]::Floor($seconds % 60) }
$times = @($runs | ForEach-Object { $_.time } | Sort-Object)
$median = $times[[math]::Floor($times.Count / 2)]

Write-Host ""
Write-Host ("{0} games in {1:0}s" -f $runs.Count, $elapsed.TotalSeconds)
Write-Host ("Survival median {0} (min {1}, max {2})" -f (Format-Time $median), (Format-Time $times[0]), (Format-Time $times[-1]))
Write-Host ("Reached the {0} cap: {1}/{2}" -f (Format-Time $MaxTime), @($runs | Where-Object { -not $_.died }).Count, $runs.Count)
Write-Host ("Died before 2:00: {0}/{1}" -f @($runs | Where-Object { $_.died -and $_.time -lt 120 }).Count, $runs.Count)
Write-Host ("First boss killed: {0}/{1}" -f @($runs | Where-Object { $_.bossesKilled -ge 1 }).Count, $runs.Count)
$runs | Format-Table seed, @{n = "time"; e = { Format-Time $_.time } }, died, kills, level, bossesKilled, bossesSpawned, weapons -AutoSize
Write-Host "Results: $results"
