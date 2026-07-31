const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberTypeMockDb } from '../../db/member-type.db.js';
import { UMB_SLUG } from './slug.js';
import { pageResponse } from '../../utils.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const structureHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/allowed-at-root`), ({ request }) => {
		const response = umbMemberTypeMockDb.getAllowedAtRoot();
		return HttpResponse.json(pageResponse(response, request));
	}),
];
