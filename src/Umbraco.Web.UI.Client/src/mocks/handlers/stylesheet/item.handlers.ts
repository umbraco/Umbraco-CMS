const { rest } = window.MockServiceWorker;
import { umbStylesheetMockDb } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`/item${UMB_SLUG}`), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('path');
		if (!paths) return res(ctx.status(400, 'no body found'));
		const decodedPaths = paths.map((path) => decodeURI(path));
		const items = umbStylesheetMockDb.item.getItems(decodedPaths);
		return res(ctx.status(200), ctx.json(items));
	}),
];
