const { rest } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../data/media/media.db.js';
import type { GetImagingResizeUrlsResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const imagingHandlers = [
	rest.get(umbracoPath('/imaging/resize/urls'), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return res(ctx.status(404));

		const media = umbMediaMockDb.getAll().filter((item) => ids.includes(item.id));

		const response: GetImagingResizeUrlsResponse = media.map((item) => ({
			id: item.id,
			urlInfos: item.urls,
		}));

		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json(response),
		);
	}),
];
