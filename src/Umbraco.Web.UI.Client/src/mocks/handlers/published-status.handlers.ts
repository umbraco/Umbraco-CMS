const { rest } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/published-cache/status'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<string>(
				'Database cache is ok. ContentStore contains 1 item and has 1 generation and 0 snapshot. MediaStore contains 5 items and has 1 generation and 0 snapshot.',
			),
		);
	}),

	rest.post(umbracoPath('/published-cache/reload'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return res(
			// Respond with a 200 status code
			ctx.status(200),
		);
	}),

	rest.post(umbracoPath('/published-cache/rebuild'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return res(
			// Respond with a 200 status code
			ctx.status(200),
		);
	}),

	rest.post(umbracoPath('/published-cache/collect'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
		);
	}),
];
