import fs from 'fs';
import path from 'path';
import { createImportMap } from '../importmap/index.js';


const excludeTheseMaps = [
	'@umbraco-cms/backoffice/models',
	'@umbraco-cms/backoffice/markdown-editor',
	'@umbraco-cms/backoffice/external/',
]


/**
 * Recursively fetch all TypeScript files in the given directory.
 * @param {string} dir Directory path to scan.
 * @returns {string[]} List of file paths.
 */
function getTsFiles(dir) {
  let files = [];
  for (const file of fs.readdirSync(dir)) {
    const fullPath = path.join(dir, file);
    if (fs.statSync(fullPath).isDirectory()) {
      files = files.concat(getTsFiles(fullPath));
    } else if (file.endsWith('.ts')) {
      files.push(fullPath);
    }
  }
  return files;
}

/**
 * Extract constants starting with "UMB_" from a file.
 * @param {string} filePath Path to the file to analyze.
 * @returns {string[]} List of exported UMB_ constants.
 */
function findUmbConstants(filePath) {
  const content = fs.readFileSync(filePath, 'utf-8');
  const lines = content.split('\n');
  const umbConstants = [];

  for (const line of lines) {
    // Match export const UMB_* syntax
    const match = line.match(/export\s+const\s+([A-Za-z_][A-Za-z0-9_]*)/);
    if (match && match[1].startsWith('UMB_')) {
      umbConstants.push(match[1]);
    }
  }

  return umbConstants;
}

/**
 * Main function to find UMB_ constants from package.json exports.
 * @param {string} projectRoot Root directory of the project (defaults to `process.cwd()`).
 * @returns {Promise<boolean>} Resolves to true if all constants are valid; false otherwise.
 */
export async function findUmbConstExports() {

	const __dirname = import.meta.dirname;
	const projectRoot = path.join(__dirname, '../../');
  const packageJsonPath = path.join(projectRoot, 'tsconfig.json');

  // Step 1: Validate package.json existence and read exports field
  if (!fs.existsSync(packageJsonPath)) {
    throw new Error('Error: package.json not found in the project root.');
  }

	const packageSource = fs.readFileSync(packageJsonPath, 'utf-8');
	const packageSourceWithoutComment = packageSource.slice(packageSource.indexOf('*/') + 2);
  const packageJson = JSON.parse(packageSourceWithoutComment);
  const exportsField = packageJson.compilerOptions.paths;

  if (!exportsField) {
    throw new Error('Error: No "exports" field found in package.json.');
  }


	const foundConsts = Object.entries(exportsField).map(([key, value]) => {
		const path = value[0];
		if(path && excludeTheseMaps.some(x => path.indexOf(x) === 0) === false) {
			const found = checkPackageExport(projectRoot, path);

			return `{
				path: '${key}',
				consts: ${JSON.stringify(found)}
			}`;
		}
		return true;
	}).filter(x => typeof(x) === 'string' && x != '');


	const content = `export const foundConsts = [${foundConsts.join(',\n')}];`;

	const outputPath = path.join(projectRoot, './utils/all-umb-consts/index.ts');
	fs.writeFileSync(outputPath, content);

	generatetestImportFile(projectRoot);

}

function checkPackageExport(projectRoot, packagePath) {
	 // Step 2: Scan JavaScript files for exported "UMB_" constants
	 //console.log('Scanning for exported "UMB_" constants...');gener
	 // remove file from path:
	 const packageFolder = packagePath.replace(/\/[^/]+$/, '');
	 const jsFiles = getTsFiles(packageFolder);

	 const umbConstants = [];

	 for (const filePath of jsFiles) {
		 const constants = findUmbConstants(filePath);
		 if (constants.length > 0) {
			 umbConstants.push(...constants);
		 }
	 }

	 return umbConstants;


}



function generatetestImportFile(projectRoot) {

	const importmap = createImportMap({
		rootDir: './src',
		replaceModuleExtensions: true,
	});

	const paths = Object.keys(importmap.imports).filter((path) => excludeTheseMaps.some(x => path.indexOf(x) === 0) === false);

	const importEnties = [];
	const dictionaryEntries = [];

	paths.forEach((path, i) => {
		importEnties.push(`import * as import${i.toString()} from '${path}';`);
		dictionaryEntries.push(`{
			path: '${path}',
			package: import${i.toString()}
		}`);
	});

	const content = `
		${importEnties.join('\n')}

		export const imports = [
			${dictionaryEntries.join(',\n')}
		];
	`

	const outputPath = path.join(projectRoot, './utils/all-umb-consts/imports.ts');
	fs.writeFileSync(outputPath, content);
}




// run it self:
(async () => {
  try {
    await findUmbConstExports();
  } catch (error) {
    console.error(error.message);
    process.exit(1);
  }
})();
