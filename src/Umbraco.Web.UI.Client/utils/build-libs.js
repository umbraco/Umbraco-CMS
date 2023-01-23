import fs from 'fs';
import path, { dirname } from 'path';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';

const currDir = dirname(fileURLToPath(import.meta.url));
const libsDir = path.resolve(currDir, '../libs');

fs.readdirSync(libsDir).forEach((lib) => {

	// Run `npm run build` for each lib
	const libDir = path.resolve(libsDir, lib);
	const packageJsonPath = path.resolve(libDir, 'package.json');

	if (!fs.existsSync(packageJsonPath)) return;
	const packageJson = JSON.parse(fs.readFileSync(packageJsonPath));
	if (packageJson.scripts && packageJson.scripts.build) {
		console.log(`Building ${lib}...`);
		execSync(`cd ${libDir} && npm run build`);
	}
});
