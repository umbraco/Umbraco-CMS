const { http, HttpResponse } = window.MockServiceWorker;
import { UMB_SLUG } from './slug.js';
import type { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const uploadResponse: ImportDictionaryRequestModel = {
	temporaryFile: { id: 'c:/path/to/tempfilename.udt' },
	parent: { id: 'b7e7d0ab-53ba-485d-dddd-12537f9925aa' },
};

export const uploadHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}/upload`), async ({ request }) => {
		if (!request.arrayBuffer()) return;

		return HttpResponse.json(uploadResponse);
	}),
];
