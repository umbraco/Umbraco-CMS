/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { readFileSync } from 'fs';
import { resolve } from 'path';
import semver from 'semver';

// The Umbraco Extension template generates its API client with @hey-api/openapi-ts, and the backoffice
// publishes the version it uses as a peer dependency. A template outside the backoffice's range fails
// "npm install" with ERESOLVE, and a client from another generator version may not match the types of
// umbHttpClient, so the template must stay within the backoffice's range.
const packageName = '@hey-api/openapi-ts';
const clientProjectRoot = resolve(import.meta.dirname, '../../');
const templatePackageFile = 'templates/UmbracoExtension/Client/package.json';

const readJson = (path) => JSON.parse(readFileSync(resolve(clientProjectRoot, path), 'utf8'));

const backofficeRange =
	readJson('package.json').dependencies?.[packageName] ??
	readJson('src/packages/core/package.json').dependencies?.[packageName];
const templateRange = readJson(`../../${templatePackageFile}`).devDependencies?.[packageName];

if (!backofficeRange || !templateRange) {
	console.error(`--- Could not find ${packageName} in the backoffice or in ${templatePackageFile} ---`);
	process.exit(1);
}

if (!semver.subset(templateRange, backofficeRange)) {
	console.error(`--- ${packageName} in the Umbraco Extension template is outside the backoffice's range ---

    template:   ${templateRange}
    backoffice: ${backofficeRange}

  Set the range in ${templatePackageFile} to the backoffice's, and regenerate the template's client
  in templates/UmbracoExtension/Client/src/api with that version.
`);
	process.exit(1);
}

console.log(`--- ${packageName} version validated successfully. ---`);
