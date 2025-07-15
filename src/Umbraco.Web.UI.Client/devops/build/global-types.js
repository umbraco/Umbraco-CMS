import { createImportMap } from '../importmap/index.js';
import { writeFileWithDir } from '../utils/index.js';

const tsPath = './dist-cms/packages/extension-types/index.d.ts';

const importmap = createImportMap({
	rootDir: './src',
	replaceModuleExtensions: true,
});

const paths = Object.keys(importmap.imports);

const content = `
${paths.map((path) => `import '${path}';`).join('\n')}
`;

writeFileWithDir(tsPath, content, (err) => {
	if (err) {
		// eslint-disable-next-line no-undef
		console.log(err);
	}

	// eslint-disable-next-line no-undef
	console.log(`global-types file generated`);
});
