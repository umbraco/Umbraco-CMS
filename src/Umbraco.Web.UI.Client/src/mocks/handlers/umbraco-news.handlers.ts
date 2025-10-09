const { rest } = window.MockServiceWorker;
import { umbNewsData } from '../data/umbraco-news.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/news-dashboard'), (req, res, ctx) => {
		const response = umbNewsData;
		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),
];
