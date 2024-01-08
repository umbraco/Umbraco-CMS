const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const overviewHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/overview`), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return;
		const response = umbStylesheetData.getAllStylesheets();
		return res(ctx.status(200), ctx.json(response));
	}),
];
