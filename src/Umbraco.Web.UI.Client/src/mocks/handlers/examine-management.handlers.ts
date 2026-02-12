const { http, HttpResponse } = window.MockServiceWorker;
import { searchResultMockData, getIndexByName, PagedIndexers } from '../data/examine.data.js';

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	IndexResponseModel,
	PagedIndexResponseModel,
	PagedSearcherResponseModel,
	PagedSearchResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/indexer'), () => {
		return HttpResponse.json<PagedIndexResponseModel>(PagedIndexers);
	}),

	http.get(umbracoPath('/indexer/:indexName'), ({ params }) => {
		const indexName = params.indexName as string;

		if (!indexName) return;
		const indexFound = getIndexByName(indexName);

		if (indexFound) {
			return HttpResponse.json<IndexResponseModel>(indexFound);
		} else {
			return new HttpResponse(null, { status: 404 });
		}
	}),

	http.post(umbracoPath('/indexer/:indexName/rebuild'), async ({ params }) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		const indexName = params.indexName as string;
		if (!indexName) return;

		const indexFound = getIndexByName(indexName);
		if (indexFound) {
			return new HttpResponse(null, { status: 201 });
		} else {
			return new HttpResponse(null, { status: 404 });
		}
	}),

	http.get(umbracoPath('/searcher'), () => {
		return HttpResponse.json<PagedSearcherResponseModel>({
			total: 0,
			items: [{ name: 'ExternalSearcher' }, { name: 'InternalSearcher' }, { name: 'InternalMemberSearcher' }],
		});
	}),

	http.get(umbracoPath('/searcher/:searcherName/query'), ({ request, params }) => {
		const query = new URL(request.url).searchParams.get('term');

		const searcherName = params.searcherName as string;

		if (!searcherName || !query) return;

		if (searcherName) {
			return HttpResponse.json<PagedSearchResultResponseModel>({
				total: 0,
				items: searchResultMockData,
			});
		} else {
			return new HttpResponse(null, { status: 404 });
		}
	}),
];
