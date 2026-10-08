<#
Two-way copy between this folder's memory\ snapshot and the local Claude Code memory directory
for the main checkout. Run `.\sync-memory.ps1 pull` at the start of a session on a machine that is
behind, and `.\sync-memory.ps1 push` before committing at the end of a session. Newer files win.

The memory directory is derived from the main checkout path the way Claude Code names project
folders (every character that is not a letter or digit becomes '-'). If that guess does not exist,
pass the directory explicitly: `.\sync-memory.ps1 pull "C:\Users\me\.claude\projects\<name>\memory"`
or set $env:CLAUDE_MEMORY_DIR.
#>
param(
    [Parameter(Mandatory = $true)][ValidateSet('pull', 'push')][string]$Mode,
    [string]$MemoryDir
)
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$common = (git -C $here rev-parse --path-format=absolute --git-common-dir).Trim()
$repo = Split-Path -Parent $common
if (-not $MemoryDir) { $MemoryDir = $env:CLAUDE_MEMORY_DIR }
if (-not $MemoryDir) {
    $name = ($repo -replace '[^A-Za-z0-9]', '-')
    $MemoryDir = Join-Path $HOME ".claude\projects\$name\memory"
}
if ($Mode -eq 'pull' -and -not (Test-Path $MemoryDir)) { New-Item -ItemType Directory -Path $MemoryDir | Out-Null }
if (-not (Test-Path $MemoryDir)) {
    throw "Memory directory not found: $MemoryDir. Look under $HOME\.claude\projects for the folder of this checkout and pass it as -MemoryDir."
}
$snapshot = Join-Path $here 'memory'
if ($Mode -eq 'pull') { $from = $snapshot; $to = $MemoryDir } else { $from = $MemoryDir; $to = $snapshot }
$copied = 0
Get-ChildItem -Path $from -Filter '*.md' -File | ForEach-Object {
    $target = Join-Path $to $_.Name
    if (-not (Test-Path $target) -or $_.LastWriteTimeUtc -gt (Get-Item $target).LastWriteTimeUtc) {
        Copy-Item -Path $_.FullName -Destination $target -Force
        $copied++
    }
}
Write-Host "$Mode`: $copied file(s) copied ($from -> $to)"
