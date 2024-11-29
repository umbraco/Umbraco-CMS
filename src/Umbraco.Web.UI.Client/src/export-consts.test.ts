import { expect, fixture, html } from '@open-wc/testing';
import { foundConsts } from '../utils/all-umb-consts/index.js';

describe('Export consts', () => {
	it('all consts are exported', async () => {
		const filteredConsts = foundConsts.filter(
			(foundConst) =>
				foundConst.path.indexOf('@umbraco-cms/backoffice/external') === -1 && foundConst.consts.length > 0,
		);

		// Check if all consts are exported
		const valid = await validateConstants(filteredConsts[2].consts, filteredConsts[2].path);

		/*const valid = (
			await Promise.all(
				filteredConsts.map(async (foundConst) => {
					try {
						if (await validateConstants(foundConst.consts, foundConst.path)) {
							console.log(`All consts is exported from ${foundConst.path}`);
							return true;
						} else {
							console.log(`Missing consts is exported from ${foundConst.path}`);
							return false;
						}
					} catch (e) {
						console.error(`Could not validate consts in ${foundConst.path}`);
						return;
					}
				}),
			)
		).some((x) => x === false);
		*/

		expect(valid).to.be.true;
	});
});

/**
 * Validates if the constants are exported via the package's exports in package.json.
 * @param {string[]} constants List of constants to check.
 * @param {object} packagePath The exports field from package.json.
 * @returns {boolean} Whether all constants are valid.
 */
async function validateConstants(constants: Array<string>, packagePath: string) {
	let allValid = true;

	//const fullPackagePath = path.join(projectRoot, packagePath.split('./').join(''));
	const contentOfPackage = await import(packagePath);

	for (const constant of constants) {
		let isExported = false;

		for (const key in contentOfPackage) {
			if (key === constant) {
				isExported = true;
				break;
			}
			/*
			const value = contentOfPackage[key];
			console.log('value...', value);
			if (typeof value === 'string' && value.includes(constant)) {
				isExported = true;
				break;
			}
			if (typeof value === 'object') {
				console.log('object...', value);
				for (const subKey in value) {
					console.log('subKey', subKey);
					if (subKey === constant) {
						isExported = true;
						break;
					}
				}
			}
				*/
		}

		if (!isExported) {
			console.error(`Error: Constant "${constant}" is not exported in of ${packagePath}`);
			allValid = false;
		}
	}

	return allValid;
}
