import { UmbResourceController } from './resource.controller.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 *
 * @param promise
 */
export function tryExecute<T>(promise: Promise<T>): Promise<UmbDataSourceResponse<T>> {
	return UmbResourceController.tryExecute<T>(promise);
}
