const { rest } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const snippetHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/snippet`), (req, res, ctx) => {
		const response = umbPartialViewMockDB.getSnippets();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/snippet/:fileName`), (req, res, ctx) => {
		const fileName = req.params.fileName as string;
		if (!fileName) return res(ctx.status(400));
		const response = umbPartialViewMockDB.getSnippet(fileName);
		return res(ctx.status(200), ctx.json(response));
	}),
];
