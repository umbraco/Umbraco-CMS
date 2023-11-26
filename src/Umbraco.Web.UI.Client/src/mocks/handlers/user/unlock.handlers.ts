const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { UMB_SLUG } from './slug.js';
import { UnlockUsersRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.post<UnlockUsersRequestModel>(umbracoPath(`${UMB_SLUG}/unlock`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;
		if (!data.userIds) return;

		umbUsersData.unlock(data.userIds);

		return res(ctx.status(200));
	}),
];
