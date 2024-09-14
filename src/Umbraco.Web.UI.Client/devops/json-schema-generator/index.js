import { writeFileSync } from 'fs';
import { format, resolveConfig } from 'prettier';
import { createImportMap } from '../importmap/index.js';

const tsPath = './src/all-packages.ts';

const importmap = createImportMap({
	rootDir: './src',
	replaceModuleExtensions: true,
});

const paths = Object.keys(importmap.imports);

const content = `
${
	paths.map(path => `import '${path}';`).join('\n')
}
`;

//const config = await resolveConfig('./.prettierrc.json');
//const formattedContent = await format(content, { ...config, parser: 'json' });

writeFileSync(tsPath, content);
