const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberTypeMockDb } from '../../db/member-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const structureHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/allowed-at-root`), () => {
		const response = umbMemberTypeMockDb.getAllowedAtRoot();
		debugger;
		return HttpResponse.json(response);
	}),
];
