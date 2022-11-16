import { rest } from 'msw';
import { searchResultMockData, getIndexByName, PagedIndexers } from '../data/examine.data';

import { umbracoPath } from '@umbraco-cms/utils';
import { Index, PagedIndex, PagedPaged, PagedSearcher, SearchResult } from '@umbraco-cms/backend-api';

export const handlers = [
	rest.get(umbracoPath('/search/index'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedIndex>(PagedIndexers)
		);
	}),

	// TODO: when using the umbracoPath helper you have to write parameters like {indexName}. MSW wants parameters as :indexName
	rest.get('/umbraco/management/api/v1/search/index/:indexName', (_req, res, ctx) => {
		const indexName = _req.params.indexName as string;

		if (!indexName) return;
		const indexFound = getIndexByName(indexName);

		if (indexFound) {
			return res(ctx.status(200), ctx.json<Index>(indexFound));
		} else {
			return res(ctx.status(404));
		}
	}),

	// TODO: when using the umbracoPath helper you have to write parameters like {indexName}. MSW wants parameters as :indexName
	rest.post('/umbraco/management/api/v1/search/index/:indexName/rebuild', async (_req, res, ctx) => {
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
			ctx.json<PagedSearcher>({
				total: 0,
				items: [{ name: 'ExternalSearcher' }, { name: 'InternalSearcher' }, { name: 'InternalMemberSearcher' }],
			})
		);
	}),

	// TODO: when using the umbracoPath helper you have to write parameters like {indexName}. MSW wants parameters as :indexName
	rest.get('/umbraco/management/api/v1/search/searcher/:searcherName/search', (_req, res, ctx) => {
		const query = _req.url.searchParams.get('query');
		const take = _req.url.searchParams.get('take');

		const searcherName = _req.params.searcherName as string;

		if (!searcherName || !query) return;

		if (searcherName) {
			return res(
				ctx.status(200),
				ctx.json<PagedPaged>({
					total: 0,
					items: [
						{
							total: 0,
							items: searchResultMockData,
						},
					],
				})
			);
		} else {
			return res(ctx.status(404));
		}
	}),
];
