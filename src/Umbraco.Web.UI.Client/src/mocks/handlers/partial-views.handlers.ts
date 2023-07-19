const { rest } = window.MockServiceWorker;
import { RestHandler, MockedRequest, DefaultBodyType } from 'msw';
import { umbPartialViewsData, umbPartialViewsTreeData } from '../data/partial-views.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { CreateTextFileViewModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

const treeHandlers = [
	rest.get(umbracoPath('/tree/partial-view/root'), (req, res, ctx) => {
		const response = umbPartialViewsTreeData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/partial-view/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbPartialViewsTreeData.getTreeItemChildren(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/partial-view/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return;

		const items = umbPartialViewsTreeData.getTreeItem(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];

const detailHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [
	rest.get(umbracoPath('/partial-view'), (req, res, ctx) => {
		const path = decodeURIComponent(req.url.searchParams.get('path') ?? '').replace('-cshtml', '.cshtml');
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewsData.getPartialView(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/partial-view'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbPartialViewsData.insertPartialView(requestBody);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath('/partial-view'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewsData.delete([path]);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.put(umbracoPath('/partial-view'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbPartialViewsData.updateData(requestBody);
		return res(ctx.status(200));
	}),
];
const folderHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [];

export const handlers = [...treeHandlers, ...detailHandlers, ...folderHandlers]