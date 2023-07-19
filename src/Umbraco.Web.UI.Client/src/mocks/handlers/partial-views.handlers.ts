const { rest } = window.MockServiceWorker;
import { RestHandler, MockedRequest, DefaultBodyType } from 'msw';
import { umbPartialViewsTreeData } from '../data/partial-views.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

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

const detailHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [];
const folderHandlers: RestHandler<MockedRequest<DefaultBodyType>>[] = [];

export const handlers = [...treeHandlers, ...detailHandlers, ...folderHandlers]