import { rest } from 'msw';
import { umbLogviewerData } from '../data/log-viewer.data';
import { umbracoPath } from '@umbraco-cms/utils';
import { SavedLogSearchModel } from '@umbraco-cms/backend-api';

export const handlers = [
	//#region Searches
	rest.get(umbracoPath('/log-viewer/saved-search'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogviewerData.searches.getSavedSearches(skipNumber, takeNumber);

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/log-viewer/saved-search/:name'), (req, res, ctx) => {
		const name = req.params.key as string;

		if (!name) return;

		const item = umbLogviewerData.searches.getByName(name);
		return res(ctx.delay(), ctx.status(200), ctx.json(item));
	}),

	rest.post<SavedLogSearchModel>(umbracoPath('/log-viewer/saved-search'), async (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.delete(umbracoPath('/log-viewer/saved-search/:name'), async (req, res, ctx) => {
		return res(ctx.status(200));
	}),
	//#endregion

	//#region Temaplates
	rest.get(umbracoPath('/log-viewer/message-template'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogviewerData.templates.getTemplates(skipNumber, takeNumber);

		const response = {
			total: umbLogviewerData.templates.total,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),
	//#endregion
	//#region Logs
	rest.get(umbracoPath('/log-viewer/level'), (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200), ctx.json(umbLogviewerData.logLevels));
	}),

	rest.get(umbracoPath('/log-viewer/level-count'), (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200), ctx.json(umbLogviewerData.logs.getLevelCount()));
	}),

	rest.get(umbracoPath('/log-viewer/validate-logs-size'), (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.get(umbracoPath('/log-viewer/log'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogviewerData.logs.getLogs(skipNumber, takeNumber);
		const response = {
			total: umbLogviewerData.logs.total,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),
];
