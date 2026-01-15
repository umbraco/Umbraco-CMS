const { http, HttpResponse } = window.MockServiceWorker;
import { umbRelationTypeMockDb } from '../../data/relation-type/relationType.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	GetRelationTypeByIdResponse,
	GetRelationTypeResponse,
} from '@umbraco-cms/backoffice/external/backend-api';

export const detailHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}`), ({ request }) => {
		const skipParam = new URL(request.url).searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = new URL(request.url).searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbRelationTypeMockDb.get({ skip, take });

		return HttpResponse.json<GetRelationTypeResponse>(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbRelationTypeMockDb.detail.read(id);
		return HttpResponse.json<GetRelationTypeByIdResponse>(response);
	}),
];
