import { rest } from 'msw';
import { umbStylesheetData } from '../data/stylesheet.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/tree/stylesheet/root'), (req, res, ctx) => {
		const response = umbStylesheetData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/stylesheet/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbStylesheetData.getTreeItemChildren(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/stylesheet/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return;

		const items = umbStylesheetData.getTreeItem(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];
