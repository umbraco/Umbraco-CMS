const { http, HttpResponse } = window.MockServiceWorker;
import { umbRelationMockDb } from '../../data/relation/relation.db.js';
import type { GetRelationByRelationTypeIdResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath('/relation/type/:id'), ({ request, params }) => {
		const skipParam = new URL(request.url).searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = new URL(request.url).searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbRelationMockDb.get({ skip, take });
		response.items = response.items.filter((item) => item.relationType.id === params.id);
		response.total = response.items.length;

		return HttpResponse.json<GetRelationByRelationTypeIdResponse>(response);
	}),
];
