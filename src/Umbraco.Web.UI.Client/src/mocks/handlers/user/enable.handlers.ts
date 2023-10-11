const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { slug } from './slug.js';
import { EnableUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.post<EnableUserRequestModel>(umbracoPath(`${slug}/enable`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;
		if (!data.userIds) return;

		umbUsersData.enable(data.userIds);

		return res(ctx.status(200));
	}),
];
