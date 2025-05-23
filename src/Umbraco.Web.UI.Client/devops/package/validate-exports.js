import { globSync } from 'glob';
import { packageJsonExports } from './meta.js';

const validateExports = async () => {
	const errors = [];

	// Iterate over the exports in package.json
	for (const [key, value] of Object.entries(packageJsonExports || {})) {
		if (value) {
			const jsFiles = await globSync(value);

			// Log an error if the export from the package.json does not exist in the build output
			if (jsFiles.length === 0) {
				errors.push(`Could not find export: ${key} -> ${value} in the build output.`);
			}
		}
	}

	if (errors.length > 0) {
		throw new Error(errors.join('\n'));
	} else {
		console.log('--- Exports validated successfully. ---');
	}
};

validateExports();
