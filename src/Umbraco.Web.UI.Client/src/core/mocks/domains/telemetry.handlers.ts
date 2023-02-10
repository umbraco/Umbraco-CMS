import { rest } from 'msw';

import { umbracoPath } from '@umbraco-cms/utils';
import { PagedTelemetryModel, TelemetryModel, TelemetryLevelModel } from '@umbraco-cms/backend-api';

let telemetryLevel = TelemetryLevelModel.BASIC;

export const handlers = [
	rest.get(umbracoPath('/telemetry/level'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<TelemetryModel>({
				telemetryLevel,
			})
		);
	}),

	rest.get(umbracoPath('/telemetry'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedTelemetryModel>({
				total: 3,
				items: [
					{ telemetryLevel: TelemetryLevelModel.MINIMAL },
					{ telemetryLevel: TelemetryLevelModel.BASIC },
					{ telemetryLevel: TelemetryLevelModel.DETAILED },
				],
			})
		);
	}),

	rest.post<TelemetryModel>(umbracoPath('/telemetry/level'), async (_req, res, ctx) => {
		const newLevel = (await _req.json<TelemetryModel>()).telemetryLevel;
		if (newLevel) {
			telemetryLevel = newLevel;
			return res(
				// Respond with a 200 status code
				ctx.status(200)
			);
		} else {
			return res(ctx.status(400));
		}
	}),
];
