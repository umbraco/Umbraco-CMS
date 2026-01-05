const { http, HttpResponse } = window.MockServiceWorker;
import { umbObjectTypeData } from '../../data/object-type/object-type.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`${UMB_SLUG}`), () => {
		const response = umbObjectTypeData.getAll();
		return HttpResponse.json(response);
	}),
];
