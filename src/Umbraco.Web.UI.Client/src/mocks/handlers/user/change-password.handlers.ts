const { rest } = window.MockServiceWorker;
import { slug } from './slug.js';
import { ChangePasswordUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.post<ChangePasswordUserRequestModel>(umbracoPath(`${slug}/change-password/:id`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;
		if (!data.newPassword) return;

		/* we don't have to update any mock data when a password is changed
		so we just return a 200 */
		return res(ctx.status(200));
	}),
];
