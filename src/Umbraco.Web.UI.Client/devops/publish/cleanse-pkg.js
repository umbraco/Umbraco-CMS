import { readFileSync, writeFileSync } from 'fs';

const packageFile = './package.json';
const packageJson = JSON.parse(readFileSync(packageFile, 'utf8'));

/**
 * Here we will modify the package.json to remove dependencies that are not needed in the CMS or does not work on npm.
 */
delete packageJson.dependencies['router-slot'];

// Write the package.json back to disk
writeFileSync(packageFile, JSON.stringify(packageJson, null, 2), 'utf8');
