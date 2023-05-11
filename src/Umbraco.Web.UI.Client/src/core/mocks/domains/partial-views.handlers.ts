import { rest } from 'msw';
import { umbPartialViewsData } from '../data/partial-views.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/tree/partial-view/root'), (req, res, ctx) => {
		const response = umbPartialViewsData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/partial-view/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbPartialViewsData.getTreeItemChildren(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/partial-view/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return;

		const items = umbPartialViewsData.getTreeItem(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];
