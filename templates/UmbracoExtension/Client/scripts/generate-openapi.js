import fetch from 'node-fetch';
import chalk from 'chalk';
import { createClient, defaultPlugins } from '@hey-api/openapi-ts';

// Start notifying user we are generating the TypeScript client
console.log(chalk.green("Generating OpenAPI client..."));

const openApiUrl = process.argv[2];
if (openApiUrl === undefined) {
  console.error(chalk.red(`ERROR: Missing URL to OpenAPI spec`));
  console.error(`Please provide the URL to the OpenAPI spec as the first argument found in ${chalk.yellow('package.json')}`);
  console.error(`Example: node generate-openapi.js ${chalk.yellow('https://localhost:44339/umbraco/openapi/REPLACE_ME.json')}`);
  process.exit();
}

// Needed to ignore self-signed certificates from running Umbraco on https on localhost
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

// Start checking to see if we can connect to the OpenAPI spec
console.log("Ensure your Umbraco instance is running");
console.log(`Fetching OpenAPI definition from ${chalk.yellow(openApiUrl)}`);

fetch(openApiUrl).then(async (response) => {
  if (!response.ok) {
    console.error(chalk.red(`ERROR: OpenAPI spec returned with a non OK (200) response: ${response.status} ${response.statusText}`));
    console.error(`The URL to your Umbraco instance may be wrong or the instance is not running`);
    console.error(`Please verify or change the URL in the ${chalk.yellow('package.json')} for the script ${chalk.yellow('generate-openapi')}`);
    return;
  }

  console.log(`OpenAPI spec fetched successfully`);
  console.log(`Calling ${chalk.yellow('hey-api')} to generate TypeScript client`);

  await createClient({
    input: openApiUrl,
    output: 'src/api',
    plugins: [
      // Spread defaults so future @hey-api/openapi-ts additions come along automatically,
      // but filter out @hey-api/sdk because we override its responseStyle below.
      ...defaultPlugins.filter((plugin) => (typeof plugin === 'string' ? plugin : plugin.name) !== '@hey-api/sdk'),
      {
        name: '@hey-api/sdk',
        responseStyle: 'fields',
      }
    ],
  });

})
  .catch(error => {
    console.error(`ERROR: Failed to connect to the OpenAPI spec: ${chalk.red(error.message)}`);
    console.error(`The URL to your Umbraco instance may be wrong or the instance is not running`);
    console.error(`Please verify or change the URL in the ${chalk.yellow('package.json')} for the script ${chalk.yellow('generate-openapi')}`);
  });
