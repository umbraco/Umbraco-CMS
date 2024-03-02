import { UmbResourceController } from './resource.controller.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export function tryExecute<T>(promise: Promise<T>): Promise<UmbDataSourceResponse<T>> {
	return UmbResourceController.tryExecute<T>(promise);
}
