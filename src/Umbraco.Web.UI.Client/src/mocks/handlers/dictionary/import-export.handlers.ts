const { rest } = window.MockServiceWorker;
//import type { UmbMockDictionaryModel } from '../../data/dictionary/dictionary.data.js';
import { umbDictionaryMockDb } from '../../data/dictionary/dictionary.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

/*
const importResponse: UmbMockDictionaryModel = {
	parent: null,
	name: 'Uploaded dictionary',
	id: 'b7e7d0ab-53ba-485d-dddd-12537f9925cb',
	hasChildren: false,
	translatedIsoCodes: ['en-us', 'da'],
	translations: [
		{
			isoCode: 'en-us',
			translation: 'I am an imported US value',
		},
		{
			isoCode: 'da',
			translation: 'I am an imported Danish value',
		},
	],
};
*/

export const importExportHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}/import`), async (req, res, ctx) => {
		alert('Dictionary import request received. Does not work in mock mode.');
		/*
		const file = req.url.searchParams.get('file');
		if (!file) return;


		const parentId = req.url.searchParams.get('parentId') ?? null;
		importResponse.parent = parentId ? { id: parentId } : null;
		umbDictionaryData.save(importResponse.id, importResponse);

		// build the path to the new item => reflects the expected server response
		const path = ['-1'];
		if (importResponse.parent?.id) {
			path.push(importResponse.parent.id);
		}

		path.push(importResponse.id);

		const contentResult = {
			content: path.join(','),
			statusCode: 200,
		};


		return res(ctx.status(200), ctx.json(contentResult));
		*/
		return res(ctx.status(200));
	}),

	// TODO => handle properly, querystring breaks handler
	rest.get(umbracoPath(`${UMB_SLUG}/:id/export`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const includeChildren = req.url.searchParams.get('includeChildren');
		const item = umbDictionaryMockDb.detail.read(id);

		alert(
			`Downloads file for dictionary "${item?.name}", ${includeChildren === 'true' ? 'with' : 'without'} children.`,
		);
		return res(ctx.status(200));
	}),
];
