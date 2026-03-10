const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import type { DisableUserRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.post<object, DisableUserRequestModel>(umbracoPath(`${UMB_SLUG}/disable`), async ({ request }) => {
		const data = await request.json();
		if (!data) return;
		if (!data.userIds) return;

		const ids = data.userIds.map((ref) => ref.id);
		umbUserMockDb.disable(ids);

		return new HttpResponse(null, { status: 200 });
	}),
];
