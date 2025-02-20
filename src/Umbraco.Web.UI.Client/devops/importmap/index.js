import { packageJsonExports, packageJsonName } from '../package/index.js';

export const createImportMap = (args) => {
	const imports = {
		...args.additionalImports,
	};

	// Iterate over the exports in package.json
	for (const [key, value] of Object.entries(packageJsonExports || {})) {
		// remove leading ./
		if (value && value.endsWith('.js')) {
			const moduleName = key.replace(/^\.\//, '');

			// replace ./dist-cms with src and remove /index.js
			let modulePath = value;
			if (typeof args.rootDir !== 'undefined') modulePath = modulePath.replace(/^\.\/dist-cms/, args.rootDir);
			if (args.replaceModuleExtensions) modulePath = modulePath.replace('.js', '.ts');
			const importAlias = `${packageJsonName}/${moduleName}`;

			imports[importAlias] = modulePath;
		}
	}

	return {
		imports,
	};
};
