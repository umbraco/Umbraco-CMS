const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.post(umbracoPath(`${slug}/set-user-groups`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbUsersData.setUserGroups(data);

		return res(ctx.status(200));
	}),
];
