# Set environment variables for artillery to use:

$Env:U_BASE_URL = "http://localhost:8111/"
$Env:U_USERNAME = "test@example.com"
$Env:U_PASS = "testtesttest"

# Execute artillery
& artillery run .\general-coverage.yml
