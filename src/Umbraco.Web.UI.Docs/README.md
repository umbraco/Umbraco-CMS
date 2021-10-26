# Umbraco Backoffice UI API Documentation

This project builds the documentation for the UI of the Umbraco backoffice, it is published on Our Umbraco in the "Reference" section of the documentation.

In order to build the documentation, please follow the following two steps:

```
npm ci
npx gulp docs
```

After this, you should have an `api` directory which contains index.html. 

In order to check if the documentation works properly, you would need to run the `api` directory in a webserver. On Windows, this can be accomplished by opening the `api` directory in [Visual Studio Code](https://code.visualstudio.com/) and running it with the [IIS Express plugin](https://marketplace.visualstudio.com/items?itemName=warren-buckley.iis-express).