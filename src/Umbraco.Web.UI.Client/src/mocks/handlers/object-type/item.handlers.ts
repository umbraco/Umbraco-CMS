const { http, HttpResponse } = window.MockServiceWorker;
import { umbObjectTypeMockDb } from '../../db/object-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`${UMB_SLUG}`), () => {
		const response = umbObjectTypeMockDb.getAll();
		return HttpResponse.json(response);
	}),
];
