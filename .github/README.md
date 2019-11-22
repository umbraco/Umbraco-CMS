# Restore database
1. Copy `_db/Umbraco.sdf` to `src/Umbraco.Web.UI/App_Data`
2. Update `src/Umbraco.Web.UI/Web.config`:
    * Update app setting `Umbraco.Core.ConfigurationStatus` to `"8.6.0"`
    * Replace configuration/connectionStrings with this:
```
<connectionStrings>
    <remove name="umbracoDbDSN"/>
    <add name="umbracoDbDSN" connectionString="Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;" providerName="System.Data.SqlServerCe.4.0"/>
</connectionStrings>
```

# Umbraco Login
```
username: segments@umbraco
password: segments
```