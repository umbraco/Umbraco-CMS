const { http, HttpResponse } = window.MockServiceWorker;
import { umbDataTypeMockDb } from '../../db/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const configurationHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
		return HttpResponse.json(umbDataTypeMockDb.getConfiguration());
	}),
];
