# What is this?

This folder contains the schema generator which resides here temporarily until Umbraco provides a proper way to generate schemas in its own repository.

## How do I use it?

1. Run `npm install` in this folder.
2. Update any schema through `api.ts` and then run `npm run generate` to simulate a new schema.
3. You then need to go to the main folder and run `npm run generate:api` to generate the API fetchers and interfaces.
   1. This is the only thing that will remain in the final version.

We are using the excellent tool `@airtasker/spot` to decorate the schema methods. [You can read more about it here](https://github.com/airtasker/spot).
