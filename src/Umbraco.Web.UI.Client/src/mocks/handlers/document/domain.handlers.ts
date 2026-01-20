const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: temp handlers until we have a real API
export const domainHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/domains`), ({ params }) => {
		const id = params.id as string;
		if (!id) return;
		const response = umbDocumentMockDb.getDomainsForDocument();
		return HttpResponse.json(response);
	}),
];
