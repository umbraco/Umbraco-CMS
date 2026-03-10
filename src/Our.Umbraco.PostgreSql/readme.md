# PostgreSQL Provider for Umbraco

```
ATTENTION: This is a community driven project and is not officially supported by Umbraco HQ.

Currently it is still in early development and should be used for testing and evaluation 
purposes only. And there are several issues in Umbraco's core that prevent full functionality 
when using PostgreSQL as the database. These are already addressed and will hopefully be 
fixed in future releases of Umbraco.
```


### Comments and contributions are very welcome!

If you are curios about the progress, please have a look into my fork of Umbraco on GitHub:

[Our.Umbraco.PostgreSql](https://github.com/idseefeld/Umbraco-CMS/tree/v173/postgreSqlProvider)



## Install PostgreSQL Database
1. Download and install PostgreSQL (version 16, 17 or 18) from the [official website](https://www.postgresql.org/download/) or especially for [Windwos](https://www.postgresql.org/download/windows/).
1. Create a new database for Umbraco using the PostgreSQL command line or a GUI tool like pgAdmin. Follow my [tutorial video](https://youtu.be/6ruTSbTdzSk).
1. Start debugging this solution [F5]
1. If not trusting databse certificate "SSL Mode" is set to `VerifyCA` during installation. But you can change this later on in the connection string in `appsettings.json` file. Read details: https://www.npgsql.org/doc/security.html?tabs=tabid-1

<!--
## appSettings
I have created a pull request for NPoco project. This will allow case insensitivity for PostgreSQL. But it is not merged yet.
When this is done, you can add the following setting from your `appsettings.json` for your Umbraco instance.
```
  "PostgreSqlOptions": {
    "EscapeTableColumAliasNames": false
  }
```
-->
