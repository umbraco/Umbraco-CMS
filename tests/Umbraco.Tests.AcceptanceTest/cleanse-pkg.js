/**
 * Cleans package.json before npm pack/publish.
 * Removes scripts and devDependencies that are not relevant for consumers.
 * Follows the same one-way pattern as the backoffice cleanse-pkg.js —
 * git restores the original file after packing.
 */
const fs = require('fs');
const path = require('path');

const packageFile = path.join(__dirname, 'package.json');
const packageJson = JSON.parse(fs.readFileSync(packageFile, 'utf-8'));

// Remove devDependencies — consumers don't need them
delete packageJson.devDependencies;

// Remove all scripts — none are relevant for consumers
// (build/prepack reference files not included in the tarball)
delete packageJson.scripts;

fs.writeFileSync(packageFile, JSON.stringify(packageJson, null, 2) + '\n');
console.log('[Prepack] Cleansed package.json for publishing.');
