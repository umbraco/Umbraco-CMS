const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { ServertimeOffset } from '@umbraco-cms/backoffice/models';

export const handlers = [
	rest.get(umbracoPath('/config/servertimeoffset'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<ServertimeOffset>({ offset: -120 }),
		);
	}),
];
