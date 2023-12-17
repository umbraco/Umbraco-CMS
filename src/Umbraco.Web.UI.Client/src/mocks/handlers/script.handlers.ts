const { rest } = window.MockServiceWorker;
import { RestHandler, MockedRequest, DefaultBodyType } from 'msw';
import { umbScriptMockDb } from '../data/script/script.db.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import {
	CreatePathFolderRequestModel,
	CreateScriptRequestModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

const treeHandlers = [
	rest.get(umbracoPath('/tree/script/root'), (req, res, ctx) => {
		const response = umbScriptMockDb.tree.getRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/script/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbScriptMockDb.tree.getChildrenOf(path);
		return res(ctx.status(200), ctx.json(response));
	}),
];

const detailHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/script'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbScriptMockDb.file.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/script'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateScriptRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbScriptMockDb.file.create(requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/script'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		umbScriptMockDb.file.delete(path);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath('/script'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as UpdateScriptRequestModel;
		debugger;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbScriptMockDb.file.update(requestBody);
		return res(ctx.status(200));
	}),
];

const itemHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/script/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		debugger;
		if (!paths) return res(ctx.status(400, 'no body found'));
		const response = umbScriptMockDb.getItems(paths);
		return res(ctx.status(200), ctx.json(response));
	}),
];

const folderHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/script/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbScriptMockDb.folder.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/script/folder'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreatePathFolderRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbScriptMockDb.folder.create(requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/script/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		umbScriptMockDb.folder.delete(path);
		return res(ctx.status(200));
	}),
];

export const handlers = [...treeHandlers, ...detailHandlers, ...itemHandlers, ...folderHandlers];
