const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../data/media/media.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	http.get(umbracoPath(`/collection${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const id = url.searchParams.get('id') ?? '';

		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));

		const response = umbMediaMockDb.collection.getCollectionMedia({ id, skip, take });

		return HttpResponse.json(response);
	}),
];
