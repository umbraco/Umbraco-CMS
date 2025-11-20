const { rest } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PagedSegmentResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	rest.get(umbracoPath('/segment'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedSegmentResponseModel>({
				total: 0,
				items: [],
			}),
		);
	}),
];
