import { rest } from 'msw';
import { searchResultMockData, getIndexByName, getIndexers } from '../data/examine.data';
import {
	IndexModel,
	SearcherModel,
	SearchResultsModel,
} from 'src/backoffice/dashboards/examine-management/examine-extension';

import { umbracoPath } from '@umbraco-cms/utils';

export const handlers = [
	rest.get(umbracoPath('/search/index'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<IndexModel[]>(getIndexers())
		);
	}),

	// TODO: when using the umbracoPath helper you have to write parameters like {indexName}. MSW wants parameters as :indexName
	rest.get('umbraco/backoffice/search/index/:indexName', (_req, res, ctx) => {
		const query = _req.url.searchParams.get('query');
		const indexName = _req.params.indexName as string;
		if (!indexName) return;

		const indexFound = getIndexByName(indexName);
		if (indexFound) {
			return res(ctx.status(200), ctx.json<IndexModel>(indexFound));
		} else {
			return res(ctx.status(404));
		}
	}),

	// TODO: when using the umbracoPath helper you have to write parameters like {indexName}. MSW wants parameters as :indexName
	rest.post('/umbraco/backoffice/search/index/:indexName/rebuild', async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		const indexName = _req.params.indexName as string;
		if (!indexName) return;

		const indexFound = getIndexByName(indexName);
		if (indexFound) {
			return res(ctx.status(201));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.get(umbracoPath('/search/searcher'), (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json<SearcherModel[]>([
				{ name: 'ExternalSearcher', providerProperties: ['Cake'] },
				{ name: 'InternalSearcher', providerProperties: ['Panda'] },
				{ name: 'InternalMemberSearcher', providerProperties: ['Bamboo?'] },
			])
		);
	}),

	// TODO: when using the umbracoPath helper you have to write parameters like {indexName}. MSW wants parameters as :indexName
	rest.get('/umbraco/backoffice/search/searcher/:searcherName', (_req, res, ctx) => {
		const query = _req.url.searchParams.get('query');
		const take = _req.url.searchParams.get('take');

		const searcherName = _req.params.searcherName as string;

		if (!searcherName || !query) return;

		//const searcherFound = getIndexByName(indexName);
		if (searcherName) {
			return res(ctx.status(200), ctx.json<SearchResultsModel[]>(searchResultMockData));
		} else {
			return res(ctx.status(404));
		}
	}),
];
