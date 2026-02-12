const { http, HttpResponse } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const snippetHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/snippet`), () => {
		const response = umbPartialViewMockDB.getSnippets();
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/snippet/:fileName`), ({ params }) => {
		const fileName = params.fileName as string;
		if (!fileName) return new HttpResponse(null, { status: 400 });
		const response = umbPartialViewMockDB.getSnippet(fileName);
		return HttpResponse.json(response);
	}),
];
