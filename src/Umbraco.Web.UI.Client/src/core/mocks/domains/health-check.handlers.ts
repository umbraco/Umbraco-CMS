import { rest } from 'msw';
import { searchResultMockData, getIndexByName, PagedIndexers } from '../data/examine.data';

import { getGroupByName, healthGroups, healthGroupsWithoutResult } from '../data/health-check.data';

import { umbracoPath } from '@umbraco-cms/utils';
import { HealthCheckGroup, PagedHealthCheckGroup } from '@umbraco-cms/backend-api';

export const handlers = [
	rest.get(umbracoPath('/health-check-group'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedHealthCheckGroup>({ total: 9999, items: healthGroupsWithoutResult })
		);
	}),

	rest.get(umbracoPath('/health-check-group/:name'), (_req, res, ctx) => {
		const name = _req.params.name as string;

		if (!name) return;
		const group = getGroupByName(name);

		if (group) {
			return res(ctx.status(200), ctx.json<HealthCheckGroup>(group));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.post(umbracoPath('/postHealthCheckExecuteAction'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds
		return res(
			// Respond with a 200 status code
			ctx.status(200)
		);
	}),
];
