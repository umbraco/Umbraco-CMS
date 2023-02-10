import { rest } from 'msw';

import {
	getGroupByName,
	getGroupWithResultsByName,
	healthGroups,
	healthGroupsWithoutResult,
} from '../data/health-check.data';

import {
	HealthCheckActionModel,
	HealthCheckGroupModel,
	HealthCheckGroupWithResultModel,
	HealthCheckResultModel,
	PagedHealthCheckGroupModelBaseModel,
	StatusResultTypeModel,
} from '@umbraco-cms/backend-api';
import { umbracoPath } from '@umbraco-cms/utils';

export const handlers = [
	rest.get(umbracoPath('/health-check-group'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedHealthCheckGroupModelBaseModel>({ total: 9999, items: healthGroupsWithoutResult })
		);
	}),

	rest.get(umbracoPath('/health-check-group/:name'), (_req, res, ctx) => {
		const name = _req.params.name as string;

		if (!name) return;
		const group = getGroupByName(name);

		if (group) {
			return res(ctx.status(200), ctx.json<HealthCheckGroupModel>(group));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.post(umbracoPath('/health-check-group/:name/check'), (_req, res, ctx) => {
		const name = _req.params.name as string;
		if (!name) return;

		const group = getGroupWithResultsByName(name);

		if (group) {
			return res(ctx.status(200), ctx.json<HealthCheckGroupWithResultModel>(group));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.post<HealthCheckActionModel>(umbracoPath('/health-check/execute-action'), async (req, res, ctx) => {
		const body = await req.json<HealthCheckActionModel>();
		const healthCheckKey = body.healthCheckKey;
		// Find the health check based on the healthCheckKey from the healthGroups[].checks
		const healthCheck = healthGroups.flatMap((group) => group.checks).find((check) => check?.key === healthCheckKey);

		if (!healthCheck) {
			return res(ctx.status(404));
		}

		const result = healthCheck.results?.at(0);

		if (!result) {
			return res(ctx.status(404));
		}

		result.resultType = StatusResultTypeModel.SUCCESS;

		return res(
			// Respond with a 200 status code
			ctx.delay(1000),
			ctx.status(200),
			ctx.json<HealthCheckResultModel>(result)
		);
	}),
];
