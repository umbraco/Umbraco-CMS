import { readFileSync } from 'fs';
import { globSync } from 'glob';

const packageJsonPath = 'package.json';
const packageJsonData = JSON.parse(readFileSync(packageJsonPath).toString());
const packageJsonExports = packageJsonData.exports;

// Iterate over the exports in package.json
for (const [key, value] of Object.entries(packageJsonExports || {})) {
	if (value) {
		const jsFiles = await globSync(value);

		// Log an error if the export from the package.json does not exist in the build output
		if (jsFiles.length === 0) {
			console.error(`Could not find export: ${key} -> ${value} in the build output.`);
		}
	}
}
