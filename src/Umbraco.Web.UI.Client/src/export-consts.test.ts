import { expect, fixture, html } from '@open-wc/testing';
import { foundConsts } from '../utils/all-umb-consts/index.js';

import * as import0 from '@umbraco-cms/backoffice/app';
import * as import1 from '@umbraco-cms/backoffice/context-api';
import * as import2 from '@umbraco-cms/backoffice/embedded-media';
import * as import3 from '@umbraco-cms/backoffice/localization-api';
import * as import4 from '@umbraco-cms/backoffice/action';
import * as import5 from '@umbraco-cms/backoffice/auth';
import * as import6 from '@umbraco-cms/backoffice/block-grid';
import * as import7 from '@umbraco-cms/backoffice/block-list';
import * as import8 from '@umbraco-cms/backoffice/block-rte';
import * as import9 from '@umbraco-cms/backoffice/block-type';
import * as import10 from '@umbraco-cms/backoffice/block';
import * as import11 from '@umbraco-cms/backoffice/code-editor';
import * as import12 from '@umbraco-cms/backoffice/collection';
import * as import13 from '@umbraco-cms/backoffice/content-type';
import * as import14 from '@umbraco-cms/backoffice/content';
import * as import15 from '@umbraco-cms/backoffice/culture';
import * as import16 from '@umbraco-cms/backoffice/current-user';
import * as import17 from '@umbraco-cms/backoffice/dashboard';
import * as import18 from '@umbraco-cms/backoffice/data-type';
import * as import19 from '@umbraco-cms/backoffice/debug';
import * as import20 from '@umbraco-cms/backoffice/dictionary';
import * as import21 from '@umbraco-cms/backoffice/document-blueprint';
import * as import22 from '@umbraco-cms/backoffice/document-type';
import * as import23 from '@umbraco-cms/backoffice/document';
import * as import24 from '@umbraco-cms/backoffice/entity-action';
import * as import25 from '@umbraco-cms/backoffice/entity-bulk-action';
import * as import26 from '@umbraco-cms/backoffice/entity-create-option-action';
import * as import27 from '@umbraco-cms/backoffice/entity';
import * as import28 from '@umbraco-cms/backoffice/health-check';
import * as import29 from '@umbraco-cms/backoffice/help';
import * as import30 from '@umbraco-cms/backoffice/icon';
import * as import31 from '@umbraco-cms/backoffice/imaging';
import * as import32 from '@umbraco-cms/backoffice/language';
import * as import33 from '@umbraco-cms/backoffice/log-viewer';
import * as import34 from '@umbraco-cms/backoffice/media-type';
import * as import35 from '@umbraco-cms/backoffice/media';
import * as import36 from '@umbraco-cms/backoffice/member-group';
import * as import37 from '@umbraco-cms/backoffice/member-type';
import * as import38 from '@umbraco-cms/backoffice/member';
import * as import39 from '@umbraco-cms/backoffice/menu';
import * as import40 from '@umbraco-cms/backoffice/modal';
import * as import41 from '@umbraco-cms/backoffice/multi-url-picker';
import * as import42 from '@umbraco-cms/backoffice/notification';
import * as import43 from '@umbraco-cms/backoffice/package';
import * as import44 from '@umbraco-cms/backoffice/partial-view';
import * as import45 from '@umbraco-cms/backoffice/picker';
import * as import46 from '@umbraco-cms/backoffice/property-editor';
import * as import47 from '@umbraco-cms/backoffice/property-type';
import * as import48 from '@umbraco-cms/backoffice/property';
import * as import49 from '@umbraco-cms/backoffice/recycle-bin';
import * as import50 from '@umbraco-cms/backoffice/relation-type';
import * as import51 from '@umbraco-cms/backoffice/relations';
import * as import52 from '@umbraco-cms/backoffice/rte';
import * as import53 from '@umbraco-cms/backoffice/script';
import * as import54 from '@umbraco-cms/backoffice/search';
import * as import55 from '@umbraco-cms/backoffice/section';
import * as import56 from '@umbraco-cms/backoffice/server-file-system';
import * as import57 from '@umbraco-cms/backoffice/settings';
import * as import58 from '@umbraco-cms/backoffice/static-file';
import * as import59 from '@umbraco-cms/backoffice/stylesheet';
import * as import60 from '@umbraco-cms/backoffice/sysinfo';
import * as import61 from '@umbraco-cms/backoffice/tags';
import * as import62 from '@umbraco-cms/backoffice/template';
import * as import63 from '@umbraco-cms/backoffice/themes';
import * as import64 from '@umbraco-cms/backoffice/tiny-mce';
import * as import65 from '@umbraco-cms/backoffice/tiptap';
import * as import66 from '@umbraco-cms/backoffice/translation';
import * as import67 from '@umbraco-cms/backoffice/tree';
import * as import68 from '@umbraco-cms/backoffice/ufm';
import * as import69 from '@umbraco-cms/backoffice/user-change-password';
import * as import70 from '@umbraco-cms/backoffice/user-group';
import * as import71 from '@umbraco-cms/backoffice/user-permission';
import * as import72 from '@umbraco-cms/backoffice/user';
import * as import73 from '@umbraco-cms/backoffice/validation';
import * as import74 from '@umbraco-cms/backoffice/variant';
import * as import75 from '@umbraco-cms/backoffice/webhook';
import * as import76 from '@umbraco-cms/backoffice/workspace';

const imports = [
	import0,
	import1,
	import2,
	import3,
	import4,
	import5,
	import6,
	import7,
	import8,
	import9,
	import10,
	import11,
	import12,
	import13,
	import14,
	import15,
	import16,
	import17,
	import18,
	import19,
	import20,
	import21,
	import22,
	import23,
	import24,
	import25,
	import26,
	import27,
	import28,
	import29,
	import30,
	import31,
	import32,
	import33,
	import34,
	import35,
	import36,
	import37,
	import38,
	import39,
	import40,
	import41,
	import42,
	import43,
	import44,
	import45,
	import46,
	import47,
	import48,
	import49,
	import50,
	import51,
	import52,
	import53,
	import54,
	import55,
	import56,
	import57,
	import58,
	import59,
	import60,
	import61,
	import62,
	import63,
	import64,
	import65,
	import66,
	import67,
	import68,
	import69,
	import70,
	import71,
	import72,
	import73,
	import74,
	import75,
	import76,
];

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

		/*const valid = (
			await Promise.all(
				filteredConsts.map(async (foundConst) => {
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
				}),
			)
		).some((x) => x === false);
		*/

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
			console.error(`Error: Constant "${constant}" is not exported in of ${packagePath}`);
			allValid = false;
		}
	}

	return allValid;
}
