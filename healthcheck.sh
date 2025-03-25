value="$(curl -fs http://localhost:8080/umbraco/management/api/v1/server/status | jq -e '.serverStatus == "Run"' || exit 1)"

# This checks for any non-zero length string, and $value will be empty when the database is not installed.
if [ "$value" ]; then
  echo "INSTALLED"
  exit 0 # In shell scripts, exit 0 means success
else
  echo "INSTALLING"
  exit 1 # And exit 1 means unhealthy
fi
