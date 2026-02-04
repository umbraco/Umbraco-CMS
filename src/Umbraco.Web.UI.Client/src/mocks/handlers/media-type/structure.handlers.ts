const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaTypeMockDb } from '../../data/media-type/media-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const structureHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/allowed-children`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const response = umbMediaTypeMockDb.getAllowedChildren(id);
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/allowed-at-root`), () => {
		const response = umbMediaTypeMockDb.getAllowedAtRoot();
		return HttpResponse.json(response);
	}),
];
