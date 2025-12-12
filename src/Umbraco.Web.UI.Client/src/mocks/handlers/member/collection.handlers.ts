const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberMockDb } from '../../data/member/member.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	http.get(umbracoPath(`/collection${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const filter = url.searchParams.get('filter');

		const options = {
			skip: skip || undefined,
			take: take || undefined,
			filter: filter || undefined,
		};

		const items = umbMemberMockDb.collection.getItems(options);
		return HttpResponse.json(items);
	}),
];
