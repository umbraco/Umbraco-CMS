const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../../data/stylesheet/stylesheet.db.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath('/stylesheet/item'), (req, res, ctx) => {
		const paths = req.url.searchParams.getAll('paths');
		if (!paths) return res(ctx.status(400, 'no body found'));
		const items = umbStylesheetData.item.getItems(paths);
		return res(ctx.status(200), ctx.json(items));
	}),
];
