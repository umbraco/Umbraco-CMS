import type { LogMessageResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';

// Minimal logs data for kenn set
export const data: Array<LogMessageResponseModel> = [
	{
		timestamp: new Date().toISOString(),
		level: LogLevelModel.INFORMATION,
		messageTemplate: 'Application started',
		renderedMessage: 'Application started',
		properties: [],
		exception: null,
	},
];
