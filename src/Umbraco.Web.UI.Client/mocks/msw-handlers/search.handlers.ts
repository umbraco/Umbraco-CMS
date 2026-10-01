const { http, HttpResponse } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	IndexResponseModel,
	PagedIndexResponseModel,
	SearchResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { HealthStatusModel, UmbracoObjectTypesModel } from '@umbraco-cms/backoffice/external/backend-api';

const indexes: Array<IndexResponseModel> = [
	{
		indexAlias: 'DraftContentIndex',
		providerName: 'search-examine-provider',
		documentCount: 42,
		healthStatus: HealthStatusModel.HEALTHY,
	},
	{
		indexAlias: 'PublishedContentIndex',
		providerName: 'search-examine-provider',
		documentCount: 38,
		healthStatus: HealthStatusModel.HEALTHY,
	},
	{
		indexAlias: 'MemberIndex',
		providerName: 'search-examine-provider',
		documentCount: 0,
		healthStatus: HealthStatusModel.EMPTY,
	},
];

const documents: SearchResultResponseModel['documents'] = [
	{
		id: 'd1b2c3d4-0000-4000-8000-000000000001',
		objectType: UmbracoObjectTypesModel.DOCUMENT,
		name: 'Home',
		icon: 'icon-home',
	},
	{
		id: 'd1b2c3d4-0000-4000-8000-000000000002',
		objectType: UmbracoObjectTypesModel.DOCUMENT,
		name: 'Products',
		icon: 'icon-document',
	},
];

export const handlers = [
	http.get(umbracoPath('/search/indexes'), () => {
		return HttpResponse.json<PagedIndexResponseModel>({
			total: indexes.length,
			items: indexes,
		});
	}),

	http.get(umbracoPath('/search/indexes/:indexAlias'), ({ params }) => {
		const index = indexes.find((x) => x.indexAlias === params.indexAlias);
		if (!index) return HttpResponse.json({ status: 404 }, { status: 404 });
		return HttpResponse.json<IndexResponseModel>(index);
	}),

	http.put(umbracoPath('/search/rebuild'), ({ request }) => {
		const indexAlias = new URL(request.url).searchParams.get('indexAlias');
		const index = indexes.find((x) => x.indexAlias === indexAlias);
		if (!index) return HttpResponse.json({ status: 404 }, { status: 404 });

		// The real server rebuilds asynchronously and reports completion over the
		// IndexRebuildCompleted server event, which MSW cannot raise.
		index.healthStatus = HealthStatusModel.REBUILDING;
		return new HttpResponse(null, { status: 200 });
	}),

	http.post(umbracoPath('/search/search'), () => {
		return HttpResponse.json<SearchResultResponseModel>({
			total: documents.length,
			documents,
			facets: [],
		});
	}),

	// The Examine provider serves its own document off a route of its own, outside the
	// Management API path, so it cannot go through `umbracoPath`.
	http.get('/umbraco/examine/api/v1/:indexAlias/document/:documentKey', ({ params }) => {
		return HttpResponse.json({
			key: String(params.documentKey),
			documents: [
				{
					fields: [
						{ name: 'Sys_Culture', type: 'keywords', values: ['none'] },
						{ name: 'Umb_Name', type: 'keywords', values: ['Mock document'] },
						{ name: 'Umb_Name', type: 'textsr1', values: ['Mock document'] },
						{ name: 'Umb_Id', type: 'keywords', values: [String(params.documentKey)] },
					],
				},
			],
		});
	}),
];
