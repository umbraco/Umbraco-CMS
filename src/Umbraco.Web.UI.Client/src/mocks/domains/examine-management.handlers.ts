import { rest } from 'msw';
// import dashboardMediaManagementStories from '../../backoffice/dashboards/media-management/dashboard-media-management.stories';

import umbracoPath from '../../core/helpers/umbraco-path';

import { Indexer, Searcher, getIndexByName, getIndexers, SearchResult, searchResFromIndex } from '../data/examine.data';

export const handlers = [
	rest.get(umbracoPath('/examine/index'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<Indexer[]>(getIndexers())
		);
	}),
	rest.get(umbracoPath('/examine/index/:indexName'), (_req, res, ctx) => {
		const indexName = _req.params.indexName as string;
		if (!indexName) return;

		const indexFound = getIndexByName(indexName);
		if (indexFound) {
			return res(ctx.status(200), ctx.json<Indexer>(indexFound));
		} else {
			return res(ctx.status(404));
		}
	}),
	rest.get(umbracoPath('/examine/searchers'), (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json<Searcher[]>([
				{ name: 'ExternalSearcher', providerProperties: ['Cake'] },
				{ name: 'InternalSearcher', providerProperties: ['Panda'] },
				{ name: 'InternalMemberSearcher', providerProperties: ['Bamboo?'] },
			])
		);
	}),
	rest.post(umbracoPath('/examine/index/:indexName/rebuild'), async (_req, res, ctx) => {
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

	rest.get(umbracoPath('/examine/index/:indexName/:searchQuery'), (_req, res, ctx) => {
		const indexName = _req.params.indexName as string;
		const searchQuery = _req.params.searchQuery as string;
		if (!indexName || !searchQuery) return;

		const indexFound = getIndexByName(indexName);
		if (indexFound) {
			return res(ctx.status(200), ctx.json<SearchResult[]>(searchResFromIndex()));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.get(umbracoPath('/examine/searchers/:searcherName/:searchQuery'), (_req, res, ctx) => {
		const searcherName = _req.params.searcherName as string;
		const searchQuery = _req.params.searchQuery as string;
		if (!searcherName || !searchQuery) return;

		//const searcherFound = getIndexByName(indexName);
		if (searcherName) {
			return res(
				ctx.status(200),
				ctx.json<SearchResult[]>([
					{ id: 1, name: 'Home', fields: { __Key: 'Stuff' }, score: 10 },
					{ id: 2, score: 5, name: 'NotHome', fields: { __Key: 'Stuff' } },
				])
			);
		} else {
			return res(ctx.status(404));
		}
	}),
];
