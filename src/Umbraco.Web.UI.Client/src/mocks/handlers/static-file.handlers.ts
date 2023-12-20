const { rest } = window.MockServiceWorker;
import { umbStaticFileData } from '../data/static-file.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const treeHandlers = [
	rest.get(umbracoPath('/tree/static-file/root'), (req, res, ctx) => {
		const response = umbStaticFileData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.get(umbracoPath('/tree/static-file/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbStaticFileData.getTreeItemChildren(path);
		return res(ctx.status(200), ctx.json(response));
	}),
];

const __treeHandlers = [
	rest.get(umbracoPath('/tree/static-file/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return;

		const items = umbStaticFileData.getTreeItem(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];

const itemHandlers = [
	rest.get(umbracoPath('/static-file/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return;

		const items = umbStaticFileData.getTreeItem(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];

export const handlers = [...treeHandlers, ...itemHandlers];
