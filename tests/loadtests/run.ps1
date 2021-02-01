# Set environment variables for artillery to use:

$Env:U_BASE_URL = "http://localhost:8111/"
$Env:U_USERNAME = "sdeminick2@gmail.com"
$Env:U_PASS = "testtesttest"

# Execute artillery
& artillery run "$PSScriptRoot\general-coverage.yml" --output "$PSScriptRoot\report.json"
& artillery report --output "$PSScriptRoot\report.html" "$PSScriptRoot\report.json"
