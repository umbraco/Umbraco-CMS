import { rest } from 'msw';

import umbracoPath from '../../core/helpers/umbraco-path';
import type { ConsentLevelSettings, TelemetryModel } from '../../core/models';

let telemetryLevel: TelemetryModel['level'] = 'Basic';

export const handlers = [
	rest.get(umbracoPath('/telemetry/ConsentLevel'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<ConsentLevelSettings>({
				telemetryLevel,
			})
		);
	}),
	rest.get(umbracoPath('/telemetry/ConsentLevels'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<TelemetryModel['level'][]>(['Minimal', 'Basic', 'Detailed'])
		);
	}),

	rest.post<ConsentLevelSettings>(umbracoPath('/telemetry/ConsentLevel'), async (_req, res, ctx) => {
		telemetryLevel = (await _req.json<ConsentLevelSettings>()).telemetryLevel;
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),
];
