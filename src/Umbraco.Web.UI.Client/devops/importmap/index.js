import { packageJsonExports, packageJsonName } from '../package/index.js';

export const createImportMap = (args) => {
	const imports = {
		...args.additionalImports,
	};

	// Iterate over the exports in package.json
	for (const [key, value] of Object.entries(packageJsonExports || {})) {
		// remove leading ./
		if (value) {
			const moduleName = key.replace(/^\.\//, '');

			// replace ./dist-cms with src and remove /index.js
			const modulePath = value.replace(/^\.\/dist-cms/, args.rootDir).replace('.js', '.ts');
			console.log('replacing', value, 'with', modulePath)
			const importAlias = `${packageJsonName}/${moduleName}`;

			imports[importAlias] = modulePath;
		}
	}

	return {
		imports,
	};
};
