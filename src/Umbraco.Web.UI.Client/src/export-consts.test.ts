import { expect } from '@open-wc/testing';
import { foundConsts } from '../utils/all-umb-consts/index.js';
import { imports } from '../utils/all-umb-consts/imports.js';

describe('Export consts', () => {
	it('all consts are exported', async () => {
		const filteredConsts = foundConsts.filter(
			(foundConst) =>
				foundConst.path.indexOf('@umbraco-cms/backoffice/external') === -1 && foundConst.consts?.length > 0,
		);

		/*console.log(
			'filteredConsts',
			filteredConsts.map((x, i) => 'import * as import' + i + " from '" + x.path + "';"),
		);*/

		// Check if all consts are exported
		//const valid = await validateConstants(filteredConsts[6].consts, imports[6], filteredConsts[6].path);

		/*
		const invalid = (
			await Promise.all(
				imports.map((p: any, i: number) => {
					if (i === filteredConsts.length) {
						console.log('No consts found for', i);
						throw new Error('No consts found for ' + i);
					}
					return validateConstants(filteredConsts[i].consts, p, filteredConsts[i].path);
				}),
			)
		).some((x) => x === false);
		*/

		const invalid = (
			await Promise.all(
				filteredConsts.map(async (entry) => {
					/*
					try {
						if (await validatePackage(foundConst.consts, foundConst.path)) {
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
					*/
					const p = imports.find((x) => x.path === entry.path);
					if (p?.package) {
						return await validateConstants(entry.consts, p.package, entry.path);
					} else {
						throw new Error(`Could not validate consts in ${entry.path}, was unable to load package`);
					}
				}),
			)
		).some((x) => x !== true);

		expect(invalid).to.be.false;
	});
});

async function validatePackage(constants: Array<string>, packagePath: string) {
	const contentOfPackage = await import(packagePath);

	return validateConstants(constants, contentOfPackage, packagePath);
}

async function validateConstants(constants: Array<string>, contentOfPackage: any, packagePath: string) {
	let allValid = true;

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
			console.error(`Error: Constant "${constant}" is not exported of ${packagePath}`);
			allValid = false;
		}
	}

	return allValid;
}
