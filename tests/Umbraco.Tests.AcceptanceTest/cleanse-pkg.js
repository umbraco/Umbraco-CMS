/**
 * Cleans package.json before npm pack/publish.
 * Removes scripts and devDependencies that are not relevant for consumers.
 * Swaps README.md with the consumer-focused README.npm.md.
 * Follows the same one-way pattern as the backoffice cleanse-pkg.js —
 * git restores the original files after packing.
 */
const fs = require('fs');
const path = require('path');

const directory = __dirname;

// --- Cleanse package.json ---
const packageFile = path.join(directory, 'package.json');
const packageJson = JSON.parse(fs.readFileSync(packageFile, 'utf-8'));

// Remove devDependencies — consumers don't need them
delete packageJson.devDependencies;

// Remove all scripts — none are relevant for consumers
// (build/prepack reference files not included in the tarball)
delete packageJson.scripts;

fs.writeFileSync(packageFile, JSON.stringify(packageJson, null, 2) + '\n');
console.log('[Prepack] Cleansed package.json for publishing.');

// --- Swap README for npm consumers ---
const readmeFile = path.join(directory, 'README.md');
const npmReadmeFile = path.join(directory, 'README.npm.md');

if (fs.existsSync(npmReadmeFile)) {
  fs.copyFileSync(npmReadmeFile, readmeFile);
  fs.unlinkSync(npmReadmeFile);
  console.log('[Prepack] Swapped README.md with README.npm.md for npm.');
}
