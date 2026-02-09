const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentBlueprintMockDb } from '../../data/document-blueprint/document-blueprint.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const ids = new URL(request.url).searchParams.getAll('id');
		if (!ids) return;
		const items = umbDocumentBlueprintMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
