const { rest } = window.MockServiceWorker;
import { umbStaticFileMockDb } from '../../data/static-file/static-file.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`/item${UMB_SLUG}`), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return res(ctx.status(400, 'no body found'));
		const items = umbStaticFileMockDb.item.getItems(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];
