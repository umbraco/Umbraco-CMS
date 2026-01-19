const { http, HttpResponse } = window.MockServiceWorker;
import { umbLogViewerData } from '../data/log-viewer.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { SavedLogSearchRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	//#region Searches
	http.get(umbracoPath('/log-viewer/saved-search'), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogViewerData.searches.getSavedSearches(skipNumber, takeNumber);

		const response = {
			total: umbLogViewerData.searches.total,
			items,
		};

		return HttpResponse.json(response);
	}),

	http.get(umbracoPath('/log-viewer/saved-search/:name'), ({ params }) => {
		const name = params.name as string;

		if (!name) return;

		const item = umbLogViewerData.searches.getByName(name);
		return HttpResponse.json(item);
	}),

	http.post<SavedLogSearchRequestModel>(umbracoPath('/log-viewer/saved-search'), async () => {
		return HttpResponse.json(null);
	}),

	http.delete(umbracoPath('/log-viewer/saved-search/:name'), async () => {
		// TODO: implement this
		return HttpResponse.json(null);
	}),
	//#endregion

	//#region Templates
	http.get(umbracoPath('/log-viewer/message-template'), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogViewerData.templates.getTemplates(skipNumber, takeNumber);

		const response = {
			total: umbLogViewerData.templates.total,
			items,
		};

		return HttpResponse.json(response);
	}),
	//#endregion

	//#region Logs
	http.get(umbracoPath('/log-viewer/level'), () => {
		return HttpResponse.json(umbLogViewerData.logLevels);
	}),

	http.get(umbracoPath('/log-viewer/level-count'), () => {
		return HttpResponse.json(umbLogViewerData.logs.getLevelCount());
	}),

	http.get(umbracoPath('/log-viewer/validate-logs-size'), () => {
		return HttpResponse.json(null);
	}),

	http.get(umbracoPath('/log-viewer/log'), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogViewerData.logs.getLogs(skipNumber, takeNumber);
		const response = {
			total: umbLogViewerData.logs.total,
			items,
		};

		return HttpResponse.json(response);
	}),
];
