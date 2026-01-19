const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaTypeMockDb } from '../../data/media-type/media-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const ids = new URL(request.url).searchParams.getAll('id');
		if (!ids) return;
		const items = umbMediaTypeMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),

	http.get(umbracoPath(`/item${UMB_SLUG}/allowed`), ({ request }) => {
		const fileExtension = new URL(request.url).searchParams.get('fileExtension');
		if (!fileExtension) return;

		const response = umbMediaTypeMockDb.getAllowedByFileExtension(fileExtension);

		return HttpResponse.json(response);
	}),
];
