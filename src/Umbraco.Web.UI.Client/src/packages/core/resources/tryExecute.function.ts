import { UmbResourceController } from './resource.controller.js';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export function tryExecute<T>(promise: Promise<T>): Promise<DataSourceResponse<T>> {
	return UmbResourceController.tryExecute<T>(promise);
}
