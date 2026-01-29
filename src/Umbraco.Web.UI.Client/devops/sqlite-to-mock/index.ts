/* eslint-disable local-rules/enforce-umbraco-external-imports */
/**
 * Main orchestrator for SQLite to mock data transformation.
 * Runs all transformation scripts to generate a new mock data set.
 *
 * Usage: npm run sqlite-to-mock -- <db-path> <set-alias>
 *   db-path:   Path to Umbraco SQLite database file
 *   set-alias: Name for the mock data set folder
 */
import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';
import { configure, getDatabase, getOutputDir, closeDatabase } from './db.js';
import { transformDataTypes } from './transform-data-types.js';
import { transformDocumentTypes } from './transform-document-types.js';
import { transformMediaTypes } from './transform-media-types.js';
import { transformDocuments } from './transform-documents.js';
import { transformMedia } from './transform-media.js';
import { transformUsers } from './transform-users.js';
import { transformTemplates } from './transform-templates.js';
import { transformLanguages } from './transform-languages.js';
import { transformDictionary } from './transform-dictionary.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Parse and validate CLI arguments
const args = process.argv.slice(2);
if (args.length < 2) {
	console.error('Usage: npm run sqlite-to-mock -- <db-path> <set-alias>');
	console.error('  db-path:   Path to Umbraco SQLite database file');
	console.error('  set-alias: Name for the mock data set folder');
	process.exit(1);
}

const [dbPath, setAlias] = args;

// Resolve the database path (support relative paths)
const resolvedDbPath = path.resolve(dbPath);

// Validate database file exists
if (!fs.existsSync(resolvedDbPath)) {
	console.error(`Error: Database file not found: ${resolvedDbPath}`);
	process.exit(1);
}

// Configure database module
configure(resolvedDbPath, setAlias);

// Initialize database
await getDatabase();

const OUTPUT_DIR = getOutputDir();

// Ensure output directory exists
if (!fs.existsSync(OUTPUT_DIR)) {
	fs.mkdirSync(OUTPUT_DIR, { recursive: true });
	console.log(`Created output directory: ${OUTPUT_DIR}`);
}

console.log('='.repeat(60));
console.log('SQLite to Mock Data Transformation');
console.log('='.repeat(60));
console.log(`Database: ${resolvedDbPath}`);
console.log(`Output directory: ${OUTPUT_DIR}`);
console.log('');

// Import and run transformations
console.log('Transforming data types...');
transformDataTypes();
console.log('');

console.log('Transforming document types...');
transformDocumentTypes();
console.log('');

console.log('Transforming media types...');
transformMediaTypes();
console.log('');

console.log('Transforming documents...');
transformDocuments();
console.log('');

console.log('Transforming media...');
transformMedia();
console.log('');

console.log('Transforming users and user groups...');
transformUsers();
console.log('');

console.log('Transforming templates...');
transformTemplates();
console.log('');

console.log('Transforming languages...');
transformLanguages();
console.log('');

console.log('Transforming dictionary...');
transformDictionary();
console.log('');

// Close database connection
closeDatabase();

console.log('='.repeat(60));
console.log('Transformation complete!');
console.log('='.repeat(60));
console.log('');

// Run eslint to fix formatting on generated files
console.log('Running eslint to fix formatting...');
try {
	execSync(`npx eslint --fix --no-warn-ignored "src/mocks/data/sets/${setAlias}/*.data.ts"`, {
		stdio: 'inherit',
		cwd: path.resolve(__dirname, '../..'),
	});
	console.log('Eslint formatting complete.');
} catch {
	console.warn('Eslint formatting had some issues, but files were generated.');
}

console.log('');
console.log('Next steps:');
console.log('1. Copy supporting files from default set (culture, etc.)');
console.log('2. Create index.ts for the new data set');
console.log(`3. Update sets/index.ts to include ${setAlias} as an option`);
console.log(`4. Test with VITE_MOCK_SET=${setAlias}`);
