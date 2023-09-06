const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../data/stylesheet.data.js';
import {
	CreateTextFileViewModelBaseModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	InterpolateRichTextStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
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
	rest.get(umbracoPath('/stylesheet'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbStylesheetData.getStylesheet(path);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.post(umbracoPath('/stylesheet'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbStylesheetData.insertStyleSheet(requestBody);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath('/stylesheet'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.delete([path]);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.put(umbracoPath('/stylesheet'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbStylesheetData.updateData(requestBody);
		return res(ctx.status(200));
	}),
	rest.get(umbracoPath('/v1/stylesheet/all'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbStylesheetData.getAllStylesheets();
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.get(umbracoPath('/v1/stylesheet/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('path');
		if (!paths) return;

		const items = umbStylesheetData.getStylesheetItem(paths[0]);
		return res(ctx.status(200), ctx.json(items));
	}),
];

type getRulesRequestBody = { path?: string };

const rulesHandlers = [
	rest.post(umbracoPath('/stylesheet/rich-text/extract-rules'), async (req, res, ctx) => {
		const requestBody = req.json() as ExtractRichTextStylesheetRulesRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = await umbStylesheetData.extractRules({ requestBody });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath('/stylesheet/rich-text/interpolate-rules'), async (req, res, ctx) => {
		const requestBody = (await req.json()) as InterpolateRichTextStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbStylesheetData.interpolateRules({ requestBody });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/stylesheet/rich-text/rules'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		try {
			const response = umbStylesheetData.getRules(path);
			return res(ctx.status(200), ctx.json(response));
		} catch (e) {
			return res(ctx.status(404));
		}
	}),
];

const folderHandlers = [
	rest.get(umbracoPath('/v1/stylesheet/all'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;

		const response = umbStylesheetData.getFolder(path);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.post(umbracoPath('/stylesheet/folder'), (req, res, ctx) => {
		const requestBody = req.json() as CreateTextFileViewModelBaseModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const response = umbStylesheetData.insertFolder(requestBody);
		return res(ctx.status(200), ctx.json(response));
	}),
	rest.delete(umbracoPath('/stylesheet/folder'), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.delete([path]);
		return res(ctx.status(200), ctx.json(response));
	}),
];

export const handlers = [...treeHandlers, ...detailHandlers, ...folderHandlers, ...rulesHandlers];
