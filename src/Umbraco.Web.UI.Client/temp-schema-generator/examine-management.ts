import { body, defaultResponse, endpoint, pathParams, queryParams, request, response, String } from '@airtasker/spot';
import { ProblemDetails } from './models';

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
	success(@body body: IndexModel[]) {}

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
	success(@body body: IndexModel) {}

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
	success(@body body: SearcherModel[]) {}

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
	success(@body body: SearchResultsModel[]) {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}

//Interfaces

export interface SearcherModel {
	name: string;
	providerProperties: ProviderPropertiesModel;
}

export interface IndexModel {
	name: string;
	canRebuild: boolean;
	healthStatus: string;
	isHealthy: boolean;
	providerProperties: ProviderPropertiesModel;
}

export interface SearchResultsModel {
	id: number;
	name: string;
	fields: FieldViewModel[];
	score: number;
}

export interface ProviderPropertiesModel {
	CommitCount: number;
	DefaultAnalyzer: string;
	DocumentCount: number;
	FieldCount: number;
	LuceneDirectory: string;
	LuceneIndexFolder: string;
	DirectoryFactory: string;
	EnableDefaultEventHandler: boolean;
	PublishedValuesOnly: boolean;
	SupportProtectedContent: boolean;
	IncludeFields?: string[];
}

export interface FieldViewModel {
	name: string;
	values: string[];
}
