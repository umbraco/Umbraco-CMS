const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentTypeMockDb } from '../../data/document-type/document-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const ids = new URL(request.url).searchParams.getAll('id');
		if (!ids) return;
		const items = umbDocumentTypeMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
