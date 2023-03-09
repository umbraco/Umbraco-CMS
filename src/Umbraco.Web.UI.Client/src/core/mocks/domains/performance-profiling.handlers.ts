import { rest } from 'msw';

import { umbracoPath } from '@umbraco-cms/utils';
import type { ProfilingStatusModel } from '@umbraco-cms/backend-api';

export const handlers = [
	rest.get(umbracoPath('/profiling/status'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<ProfilingStatusModel>({ enabled: true })
		);
	}),
];
