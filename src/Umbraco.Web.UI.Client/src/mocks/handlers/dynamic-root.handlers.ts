import { umbDocumentMockDb } from '../data/document/document.db.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

export const handlers = [
	http.post(umbracoPath('/dynamic-root/query'), async () => {
		const response = umbDocumentMockDb.tree
			.getRoot()
			.items.map((item) => item.id)
			.slice(0, 1);
		return HttpResponse.json(response);
	}),
];
