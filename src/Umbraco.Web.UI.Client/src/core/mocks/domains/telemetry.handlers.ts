import { rest } from 'msw';

import { umbracoPath } from '@umbraco-cms/utils';
import { PagedTelemetry, Telemetry, TelemetryLevel } from '@umbraco-cms/backend-api';

let telemetryLevel = TelemetryLevel.BASIC;

export const handlers = [
	rest.get(umbracoPath('/telemetry/level'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<Telemetry>({
				telemetryLevel,
			})
		);
	}),

	rest.get(umbracoPath('/telemetry'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedTelemetry>({
				total: 3,
				items: [
					{ telemetryLevel: TelemetryLevel.MINIMAL },
					{ telemetryLevel: TelemetryLevel.BASIC },
					{ telemetryLevel: TelemetryLevel.DETAILED },
				],
			})
		);
	}),

	rest.post<Telemetry>(umbracoPath('/telemetry/ConsentLevel'), async (_req, res, ctx) => {
		const newLevel = (await _req.json<Telemetry>()).telemetryLevel;
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
