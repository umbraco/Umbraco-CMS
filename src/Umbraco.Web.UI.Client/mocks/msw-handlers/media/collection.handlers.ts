const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../db/media.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';

export const collectionHandlers = [
	http.get(umbracoPath(`/collection${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const number = (name: string) => (url.searchParams.has(name) ? Number(url.searchParams.get(name)) : undefined);

		const response = umbMediaMockDb.collection.getCollectionMedia({
			id: url.searchParams.get('id') ?? '',
			dataTypeId: url.searchParams.get('dataTypeId') ?? undefined,
			orderBy: url.searchParams.get('orderBy') ?? undefined,
			orderDirection: (url.searchParams.get('orderDirection') as DirectionModel) ?? undefined,
			filter: url.searchParams.get('filter') ?? undefined,
			skip: number('skip'),
			take: number('take'),
		});

		return HttpResponse.json(response);
	}),
];
