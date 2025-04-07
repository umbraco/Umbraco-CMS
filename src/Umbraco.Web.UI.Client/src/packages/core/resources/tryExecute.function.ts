import { UmbTryExecuteController } from './try-execute.controller.js';
import type { UmbTryExecuteOptions } from './types.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export async function tryExecute<T>(
	host: UmbControllerHost,
	promise: Promise<T>,
	opts?: UmbTryExecuteOptions,
): Promise<UmbDataSourceResponse<T>> {
	const controller = new UmbTryExecuteController(host, promise);
	const response = await controller.tryExecute(opts);
	controller.destroy();
	return response as UmbDataSourceResponse<T>;
}
