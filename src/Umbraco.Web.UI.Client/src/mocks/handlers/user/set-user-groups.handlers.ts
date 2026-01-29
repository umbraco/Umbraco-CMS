const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../db/user.db.js';
import { UMB_SLUG } from './slug.js';
import type { UpdateUserGroupsOnUserRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.post(umbracoPath(`${UMB_SLUG}/set-user-groups`), async ({ request }) => {
		const data = await request.json();
		if (!data) return;

			umbUserMockDb.setUserGroups(data);

		return new HttpResponse(null, { status: 200 });
	}),
];
