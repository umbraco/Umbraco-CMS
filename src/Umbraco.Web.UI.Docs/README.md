# Umbraco Backoffice UI API Documentation

This project builds the documentation for the UI of the Umbraco backoffice, it is published on the Umbraco Docs in the "Reference" section of the documentation.

All versions can be accessed through the https://apidocs.umbraco.com/vXX/ui/ url where XX is the major version number of Umbraco.

In order to build the documentation, please follow the following two steps:

```
npm ci
npm start
```

After this, you should have an `api` directory which contains index.html. 

In order to check if the documentation works properly, you would need to run the `api` directory in a webserver. On Windows, this can be accomplished by opening the `api` directory in [Visual Studio Code](https://code.visualstudio.com/) and running it with the [IIS Express plugin](https://marketplace.visualstudio.com/items?itemName=warren-buckley.iis-express).
