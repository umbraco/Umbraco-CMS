const { rest } = window.MockServiceWorker;
import { CreateTextFileViewModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { umbStylesheetData } from '../data/stylesheet.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const treeHandlers = [
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

const detailHandlers = [
	rest.get(umbracoPath('/v1/stylesheet'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbStylesheetData.getStylesheet(path);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.post(umbracoPath('/partial-view'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbStylesheetData.insertStyleSheet(requestBody);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath('/partial-view'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.delete([path]);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.put(umbracoPath('/partial-view'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbStylesheetData.updateData(requestBody);
		return res(ctx.status(200));
	}),
];

export const handlers = [...treeHandlers, ...detailHandlers];
