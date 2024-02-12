const { rest } = window.MockServiceWorker;

import {
	getGroupByName,
	getGroupWithResultsByName,
	healthGroups,
	healthGroupsWithoutResult,
} from '../data/health-check.data.js';

import type {
	HealthCheckActionRequestModel,
	HealthCheckGroupResponseModel,
	HealthCheckGroupWithResultResponseModel,
	HealthCheckResultResponseModel,
	PagedHealthCheckGroupResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/health-check-group'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedHealthCheckGroupResponseModel>({ total: 9999, items: healthGroupsWithoutResult }),
		);
	}),

	rest.get(umbracoPath('/health-check-group/:name'), (_req, res, ctx) => {
		const name = _req.params.name as string;

		if (!name) return;
		const group = getGroupByName(name);

		if (group) {
			return res(ctx.status(200), ctx.json<HealthCheckGroupResponseModel>(group));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.post(umbracoPath('/health-check-group/:name/check'), (_req, res, ctx) => {
		const name = _req.params.name as string;
		if (!name) return;

		const group = getGroupWithResultsByName(name);

		if (group) {
			return res(ctx.status(200), ctx.json<HealthCheckGroupWithResultResponseModel>(group));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.post<HealthCheckActionRequestModel>(umbracoPath('/health-check/execute-action'), async (req, res, ctx) => {
		const body = await req.json<HealthCheckActionRequestModel>();
		const healthCheckId = body.healthCheck.id;
		// Find the health check based on the healthCheckId from the healthGroups[].checks
		const healthCheck = healthGroups.flatMap((group) => group.checks).find((check) => check?.id === healthCheckId);

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
			ctx.json<HealthCheckResultResponseModel>(result),
		);
	}),
];
