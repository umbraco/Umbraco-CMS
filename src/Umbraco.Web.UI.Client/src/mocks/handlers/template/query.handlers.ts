const { rest } = window.MockServiceWorker;
import { umbTemplateMockDb } from '../../data/template/template.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const queryHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/query/settings`), (req, res, ctx) => {
		const response = umbTemplateMockDb.query.getQuerySettings();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.post(umbracoPath(`${UMB_SLUG}/query/execute`), (req, res, ctx) => {
		const response = umbTemplateMockDb.query.getQueryResult();
		return res(ctx.status(200), ctx.json(response));
	}),
];
