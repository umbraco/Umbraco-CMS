const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbUsersData.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
