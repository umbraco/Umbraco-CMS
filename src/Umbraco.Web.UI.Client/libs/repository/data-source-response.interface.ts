import type { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

export interface DataSourceResponse<T = undefined> {
	data?: T;
	error?: ProblemDetailsModel;
}
