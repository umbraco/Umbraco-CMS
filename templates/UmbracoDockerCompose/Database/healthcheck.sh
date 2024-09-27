value="$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d master -Q "SELECT state_desc FROM sys.databases WHERE name = 'umbracoDb'" | awk 'NR==3')"

# This checks for any non-zero length string, and $value will be empty when the database does not exist.
if [ -n "$value" ]
then
  echo "ONLINE"
  return 0 # With docker 0 = success
else
  echo "OFFLINE"
  return 1 # And 1 = unhealthy
fi

# This is useful for debugging
# echo "Value is:"
# echo "$value"
