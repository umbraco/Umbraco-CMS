const { http, HttpResponse } = window.MockServiceWorker;
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
	http.post(umbracoPath(`${UMB_SLUG}/import`), async () => {
		alert('Dictionary import request received. Does not work in mock mode.');
		/*
		const searchParams = new URL(request.url).searchParams;
		const file = searchParams.get('file');
		if (!file) return;


		const parentId = searchParams.get('parentId') ?? null;
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


		return HttpResponse.json(contentResult);
		*/
		return new HttpResponse(null, { status: 200 });
	}),

	// TODO => handle properly, querystring breaks handler
	http.get(umbracoPath(`${UMB_SLUG}/:id/export`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return;

		const searchParams = new URL(request.url).searchParams;
		const includeChildren = searchParams.get('includeChildren');
		const item = umbDictionaryMockDb.detail.read(id);

		alert(
			`Downloads file for dictionary "${item?.name}", ${includeChildren === 'true' ? 'with' : 'without'} children.`,
		);
		return new HttpResponse(null, { status: 200 });
	}),
];
