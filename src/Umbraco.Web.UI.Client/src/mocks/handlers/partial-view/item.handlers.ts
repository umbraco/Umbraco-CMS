const { rest } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/item`), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return res(ctx.status(400, 'no body found'));
		const items = umbPartialViewMockDB.item.getItems(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];
