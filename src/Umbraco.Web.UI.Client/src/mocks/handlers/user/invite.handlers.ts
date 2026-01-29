const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../db/user.db.js';
import { UMB_SLUG } from './slug.js';
import type { InviteUserRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const inviteSlug = `${UMB_SLUG}/invite`;

export const handlers = [
	http.post<object, InviteUserRequestModel>(umbracoPath(`${inviteSlug}`), async ({ request }) => {
		const data = await request.json();
		if (!data) return;

		const { userId } = umbUserMockDb.invite(data);

		if (!userId) return new HttpResponse(null, { status: 400 });

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + userId,
				'Umb-Generated-Resource': userId,
			},
		});
	}),

	http.post(umbracoPath(`${inviteSlug}/resend`), async ({ request }) => {
		const data = await request.json();
		if (!data) return;

		return new HttpResponse(null, { status: 200 });
	}),
];
