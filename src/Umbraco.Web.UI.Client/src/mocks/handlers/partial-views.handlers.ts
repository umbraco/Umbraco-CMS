const { rest } = window.MockServiceWorker;
import { RestHandler, MockedRequest, DefaultBodyType } from 'msw';
import { umbPartialViewMockDB } from '../data/partial-view/partial-view.db.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { CreatePartialViewRequestModel, UpdatePartialViewRequestModel } from '@umbraco-cms/backoffice/backend-api';

const treeHandlers = [
	rest.get(umbracoPath('/tree/partial-view/root'), (req, res, ctx) => {
		const response = umbPartialViewMockDB.tree.getRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/partial-view/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewMockDB.tree.getChildrenOf(path);
		return res(ctx.status(200), ctx.json(response));
	}),
];

const detailHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/script'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewMockDB.file.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/script'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreatePartialViewRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const path = umbPartialViewMockDB.file.create(requestBody);
		return res(
			ctx.status(200),
			ctx.set({
				Location: path,
			}),
		);
	}),

	rest.delete(umbracoPath('/script'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		umbPartialViewMockDB.file.delete(path);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath('/script'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as UpdatePartialViewRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbPartialViewMockDB.file.update(requestBody);
		return res(ctx.status(200));
	}),
];

const itemHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/script/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return res(ctx.status(400, 'no body found'));
		const response = umbPartialViewMockDB.item.getItems(paths);
		return res(ctx.status(200), ctx.json(response));
	}),
];

const folderHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/script/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewMockDB.folder.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/script/folder'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreatePartialViewRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbPartialViewMockDB.folder.create(requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/script/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		umbPartialViewMockDB.folder.delete(path);
		return res(ctx.status(200));
	}),
];

export const handlers = [...treeHandlers, ...detailHandlers, ...itemHandlers, ...folderHandlers];
