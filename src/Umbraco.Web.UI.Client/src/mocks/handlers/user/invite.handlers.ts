const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { UMB_SLUG } from './slug.js';
import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.post<InviteUserRequestModel>(umbracoPath(`${UMB_SLUG}/invite`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbUsersData.invite(data);

		return res(ctx.status(200));
	}),

	rest.post<any>(umbracoPath(`${UMB_SLUG}/invite/resend`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		return res(ctx.status(200));
	}),
];
