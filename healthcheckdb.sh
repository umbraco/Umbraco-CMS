postLogoutRedirectUris='["http://localhost:8080/umbraco/oauth_complete","http://localhost:8080/umbraco/logout","http://localhost:44371/umbraco/oauth_complete","http://localhost:44371/umbraco/logout","http://localhost:44372/umbraco/oauth_complete","http://localhost:44372/umbraco/logout"]'
redirectUris='["http://localhost:8080/umbraco/oauth_complete","http://localhost:44371/umbraco/oauth_complete","http://localhost:44372/umbraco/oauth_complete"]'

/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d "umbracoDb" -Q "UPDATE [dbo].[umbracoOpenIddictApplication] SET PostLogoutRedirectUris=${postLogoutRedirectUris} WHERE ClientId ='umbraco-back-office'"
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d "umbracoDb" -Q "UPDATE [dbo].[umbracoOpenIddictApplication] SET RedirectUris=${redirectUris} WHERE ClientId='umbraco-back-office'"

