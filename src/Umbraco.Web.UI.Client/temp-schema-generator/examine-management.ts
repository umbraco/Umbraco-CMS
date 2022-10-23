import { body, defaultResponse, endpoint, pathParams, request, response, String } from '@airtasker/spot';
import { ProblemDetails } from './models';
import { Indexer, Searcher, SearchResult } from '../src/mocks/data/examine.data';

@endpoint({
	method: 'GET',
	path: '/examine/index',
})
export class getIndexers {
	@response({ status: 200 })
	success(@body body: Indexer[]) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'GET',
	path: '/examine/index/:indexName',
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
	method: 'GET',
	path: '/examine/searchers',
})
export class getSearchers {
	@response({ status: 200 })
	success(@body body: Searcher[]) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'POST',
	path: '/examine/index/:indexName/rebuild',
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
	path: '/examine/searchers/:searcherName/:searchQuery',
})
export class getSearchSearchers {
	@request
	request(
		@pathParams
		pathParams: {
			searcherName: String;
			searchQuery: String;
		}
	) {}

	@response({ status: 200 })
	success(@body body: SearchResult[]) {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'GET',
	path: '/examine/index/:indexName/:searchQuery',
})
export class getSearchIndex {
	@request
	request(
		@pathParams
		pathParams: {
			indexName: String;
			searchQuery: String;
		}
	) {}

	@response({ status: 200 })
	success(@body body: SearchResult[]) {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}
