const { rest } = window.MockServiceWorker;
import { umbDictionaryData } from '../data/dictionary.data.js';
import {
	ImportDictionaryRequestModel,
	DictionaryOverviewResponseModel,
	DictionaryItemResponseModel,
	EntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

const uploadResponse: ImportDictionaryRequestModel = {
	temporaryFileId: 'c:/path/to/tempfilename.udt',
	parentId: 'b7e7d0ab-53ba-485d-dddd-12537f9925aa',
};

/// TODO: get correct type
const importResponse: DictionaryItemResponseModel & EntityTreeItemResponseModel = {
	parentId: null,
	name: 'Uploaded dictionary',
	id: 'b7e7d0ab-53ba-485d-dddd-12537f9925cb',
	hasChildren: false,
	type: 'dictionary-item',
	isContainer: false,
	translations: [
		{
			isoCode: 'en',
			translation: 'I am an imported US value',
		},
		{
			isoCode: 'fr',
			translation: 'I am an imported French value',
		},
	],
};

// alternate data for dashboard view
const overviewData: Array<DictionaryOverviewResponseModel> = [
	{
		name: 'Hello',
		id: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		translatedIsoCodes: ['en'],
	},
	{
		name: 'Hello again',
		id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		translatedIsoCodes: ['en', 'fr'],
	},
];

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/dictionary/:id', (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const dictionary = umbDictionaryData.getById(id);
		return res(ctx.status(200), ctx.json(dictionary));
	}),

	rest.get('/umbraco/management/api/v1/dictionary', (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const take = req.url.searchParams.get('take');
		if (!skip || !take) return;

		// overview is DictionaryOverview[], umbDictionaryData provides DictionaryDetails[]
		// which are incompatible types to mock, so we can do a filthy replacement here
		//const items = umbDictionaryData.getList(parseInt(skip), parseInt(take));
		const items = overviewData;

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post('/umbraco/management/api/v1/dictionary', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		data.icon = 'umb:book-alt';
		data.hasChildren = false;
		data.type = 'dictionary-item';
		data.translations = [
			{
				isoCode: 'en-US',
				translation: '',
			},
			{
				isoCode: 'fr',
				translation: '',
			},
		];

		const value = umbDictionaryData.save(data.id, data);

		const createdResult = {
			value,
			statusCode: 200,
		};

		return res(ctx.status(200), ctx.json(createdResult));
	}),

	rest.patch('/umbraco/management/api/v1/dictionary/:id', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const id = req.params.id as string;
		if (!id) return;

		const dataToSave = JSON.parse(data[0].value);
		const saved = umbDictionaryData.save(dataToSave.id, dataToSave);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.get('/umbraco/management/api/v1/tree/dictionary/root', (req, res, ctx) => {
		const items = umbDictionaryData.getTreeRoot();
		const response = {
			total: items.length,
			items,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/dictionary/children', (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;

		const items = umbDictionaryData.getTreeItemChildren(parentId);

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/dictionary/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;

		const items = umbDictionaryData.getTreeItem(ids);

		return res(ctx.status(200), ctx.json(items));
	}),

	rest.delete('/umbraco/management/api/v1/dictionary/:id', (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbDictionaryData.delete([id]);

		return res(ctx.status(200));
	}),

	// TODO => handle properly, querystring breaks handler
	rest.get('/umbraco/management/api/v1/dictionary/:id/export', (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const includeChildren = req.url.searchParams.get('includeChildren');
		const item = umbDictionaryData.getById(id);

		alert(
			`Downloads file for dictionary "${item?.name}", ${includeChildren === 'true' ? 'with' : 'without'} children.`,
		);
		return res(ctx.status(200));
	}),

	rest.post('/umbraco/management/api/v1/dictionary/upload', async (req, res, ctx) => {
		if (!req.arrayBuffer()) return;

		return res(ctx.status(200), ctx.json(uploadResponse));
	}),

	rest.post('/umbraco/management/api/v1/dictionary/import', async (req, res, ctx) => {
		const file = req.url.searchParams.get('file');

		if (!file || !importResponse.id) return;

		importResponse.parentId = req.url.searchParams.get('parentId') ?? null;
		umbDictionaryData.save(importResponse.id, importResponse);

		// build the path to the new item => reflects the expected server response
		const path = ['-1'];
		if (importResponse.parentId) path.push(importResponse.parentId);

		path.push(importResponse.id);

		const contentResult = {
			content: path.join(','),
			statusCode: 200,
		};

		return res(ctx.status(200), ctx.json(contentResult));
	}),
];
