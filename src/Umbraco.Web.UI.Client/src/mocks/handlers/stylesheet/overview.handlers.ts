const { rest } = window.MockServiceWorker;
import { umbStylesheetMockDb } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const overviewHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/overview`), async (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const takeParam = req.url.searchParams.get('take');
		const response = umbStylesheetMockDb.getOverview({
			skip: skipParam ? parseInt(skipParam) : undefined,
			take: takeParam ? parseInt(takeParam) : undefined,
		});
		return res(ctx.status(200), ctx.json(response));
	}),
];
