import type { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbLogLevelCounts = {
	[level in LogLevelModel]: number;
};
