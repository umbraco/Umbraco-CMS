import { packageJsonExports } from './devops/package/meta.js';

const excludePaths = ['./src/external', '.src/apps', './src/packages/extension-types/index.d.ts'];
const entryPoints = [];

for (const [key, value] of Object.entries(packageJsonExports || {})) {
	if (value) {
		let path = value.replace(/^\.\/dist-cms/, './src');
		path = path.replace('.js', '.ts');

		if (excludePaths.some((excludePath) => path.startsWith(excludePath))) {
			console.log('excluding', path);
		} else {
			entryPoints.push(path);
		}
	}
}

export default {
	entryPoints,
	out: 'ui-api',
};
