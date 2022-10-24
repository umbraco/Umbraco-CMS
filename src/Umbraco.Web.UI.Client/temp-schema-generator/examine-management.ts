import { body, defaultResponse, endpoint, pathParams, queryParams, request, response, String } from '@airtasker/spot';
import { ProblemDetails } from './models';
import { Indexer, Searcher, SearchResult } from '../src/core/mocks/data/examine.data';

@endpoint({
	method: 'GET',
	path: '/search/index',
})
export class getIndexers {
	@request
	request(
		@queryParams
		queryParams: {
			skip: number;
			take: number;
		}
	) {}

	@response({ status: 200 })
	success(@body body: Indexer[]) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'GET',
	path: '/search/index/:indexName',
})
export class getIndex {
	@request
	request(
		@pathParams
		pathParams: {
			indexName: String;
		}
	) {}

	@response({ status: 200 })
	success(@body body: Indexer) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'POST',
	path: '/search/index/:indexName/rebuild',
})
export class postIndexRebuild {
	@request
	request(
		@pathParams
		pathParams: {
			indexName: String;
		}
	) {}
	@response({ status: 201 })
	success() {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'GET',
	path: '/search/searcher',
})
export class getSearchers {
	@request
	request(
		@queryParams
		queryParams: {
			skip: number;
			take: number;
		}
	) {}

	@response({ status: 200 })
	success(@body body: Searcher[]) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'GET',
	path: '/search/searcher/:searcherName',
})
export class getSearchSearchers {
	@request
	request(
		@pathParams
		pathParams: {
			searcherName: String;
		},
		@queryParams
		queryParams: {
			query: String;
			take: number;
		}
	) {}

	@response({ status: 200 })
	success(@body body: SearchResult[]) {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}
