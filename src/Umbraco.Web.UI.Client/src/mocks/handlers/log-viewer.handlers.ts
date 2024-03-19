const { rest } = window.MockServiceWorker;
import { umbLogViewerData } from '../data/log-viewer.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { SavedLogSearchRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	//#region Searches
	rest.get(umbracoPath('/log-viewer/saved-search'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogViewerData.searches.getSavedSearches(skipNumber, takeNumber);

		const response = {
			total: umbLogViewerData.searches.total,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/log-viewer/saved-search/:name'), (req, res, ctx) => {
		const name = req.params.name as string;

		if (!name) return;

		const item = umbLogViewerData.searches.getByName(name);
		return res(ctx.delay(), ctx.status(200), ctx.json(item));
	}),

	rest.post<SavedLogSearchRequestModel>(umbracoPath('/log-viewer/saved-search'), async (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.delete(umbracoPath('/log-viewer/saved-search/:name'), async (req, res, ctx) => {
		// TODO: implement this
		return res(ctx.status(200));
	}),
	//#endregion

	//#region Templates
	rest.get(umbracoPath('/log-viewer/message-template'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogViewerData.templates.getTemplates(skipNumber, takeNumber);

		const response = {
			total: umbLogViewerData.templates.total,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),
	//#endregion

	//#region Logs
	rest.get(umbracoPath('/log-viewer/level'), (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200), ctx.json(umbLogViewerData.logLevels));
	}),

	rest.get(umbracoPath('/log-viewer/level-count'), (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200), ctx.json(umbLogViewerData.logs.getLevelCount()));
	}),

	rest.get(umbracoPath('/log-viewer/validate-logs-size'), (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.get(umbracoPath('/log-viewer/log'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogViewerData.logs.getLogs(skipNumber, takeNumber);
		const response = {
			total: umbLogViewerData.logs.total,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),
];
