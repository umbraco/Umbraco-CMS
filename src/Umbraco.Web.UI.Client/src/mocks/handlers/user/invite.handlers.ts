const { rest } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import type { InviteUserRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const inviteSlug = `${UMB_SLUG}/invite`;

export const handlers = [
	rest.post<InviteUserRequestModel>(umbracoPath(`${inviteSlug}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const { userId } = umbUserMockDb.invite(data);

		if (!userId) return res(ctx.status(400));

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + userId,
				'Umb-Generated-Resource': userId,
			}),
		);
	}),

	rest.post<any>(umbracoPath(`${inviteSlug}/resend`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		return res(ctx.status(200));
	}),
];
