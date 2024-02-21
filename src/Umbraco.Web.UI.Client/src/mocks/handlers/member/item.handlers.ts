const { rest } = window.MockServiceWorker;
import { umbMemberMockDb } from '../../data/member/member.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`/item${UMB_SLUG}`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbMemberMockDb.item.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
