/**
 * Main orchestrator for SQLite to mock data transformation.
 * Runs all transformation scripts to generate the kenn mock data set.
 */
import { db, OUTPUT_DIR } from './db.js';
import fs from 'fs';
import path from 'path';

// Ensure output directory exists
if (!fs.existsSync(OUTPUT_DIR)) {
	fs.mkdirSync(OUTPUT_DIR, { recursive: true });
	console.log(`Created output directory: ${OUTPUT_DIR}`);
}

console.log('='.repeat(60));
console.log('SQLite to Mock Data Transformation');
console.log('='.repeat(60));
console.log(`Output directory: ${OUTPUT_DIR}`);
console.log('');

// Import and run transformations
console.log('Transforming data types...');
const { transformDataTypes } = await import('./transform-data-types.js');
transformDataTypes();
console.log('');

console.log('Transforming document types...');
const { transformDocumentTypes } = await import('./transform-document-types.js');
transformDocumentTypes();
console.log('');

console.log('Transforming media types...');
const { transformMediaTypes } = await import('./transform-media-types.js');
transformMediaTypes();
console.log('');

console.log('Transforming documents...');
const { transformDocuments } = await import('./transform-documents.js');
transformDocuments();
console.log('');

console.log('Transforming media...');
const { transformMedia } = await import('./transform-media.js');
transformMedia();
console.log('');

console.log('Transforming users and user groups...');
const { transformUsers } = await import('./transform-users.js');
transformUsers();
console.log('');

console.log('Transforming templates...');
const { transformTemplates } = await import('./transform-templates.js');
transformTemplates();
console.log('');

console.log('Transforming languages...');
const { transformLanguages } = await import('./transform-languages.js');
transformLanguages();
console.log('');

console.log('Transforming dictionary...');
const { transformDictionary } = await import('./transform-dictionary.js');
transformDictionary();
console.log('');

// Close database connection
db.close();

console.log('='.repeat(60));
console.log('Transformation complete!');
console.log('='.repeat(60));
console.log('');
console.log('Next steps:');
console.log('1. Copy supporting files from default set (culture, etc.)');
console.log('2. Create index.ts for the kenn data set');
console.log('3. Update sets/index.ts to include kenn as an option');
console.log('4. Test with VITE_MOCK_SET=kenn');
