const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../data/stylesheet/stylesheet.db.js';
import {
	CreatePathFolderRequestModel,
	CreateStylesheetRequestModel,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const treeHandlers = [
	rest.get(umbracoPath('/tree/stylesheet/root'), (req, res, ctx) => {
		const response = umbStylesheetData.tree.getRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/stylesheet/children'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.tree.getChildrenOf(path);
		return res(ctx.status(200), ctx.json(response));
	}),
];

const detailHandlers = [
	rest.get(umbracoPath('/stylesheet'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.file.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/stylesheet'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const path = umbStylesheetData.file.create(requestBody);
		return res(
			ctx.status(200),
			ctx.set({
				Location: path,
			}),
		);
	}),

	rest.delete(umbracoPath('/stylesheet'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		umbStylesheetData.file.delete(path);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath('/stylesheet'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as UpdateStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbStylesheetData.file.update(requestBody);
		return res(ctx.status(200));
	}),
];

const itemHandlers = [
	rest.get(umbracoPath('/stylesheet/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return res(ctx.status(400, 'no body found'));
		const items = umbStylesheetData.item.getItems(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];

const collectionHandlers = [
	rest.get(umbracoPath('/stylesheet/all'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;
		const response = umbStylesheetData.getAllStylesheets();
		return res(ctx.status(200), ctx.json(response));
	}),
];

const folderHandlers = [
	rest.get(umbracoPath('/stylesheet/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.folder.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/stylesheet/folder'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreatePathFolderRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbStylesheetData.folder.create(requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/stylesheet/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		umbStylesheetData.folder.delete(path);
		return res(ctx.status(200));
	}),
];

export const handlers = [...treeHandlers, ...detailHandlers, ...itemHandlers, ...collectionHandlers, ...folderHandlers];
