import { UmbTryExecuteController } from './try-execute.controller.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export async function tryExecute<T>(host: UmbControllerHost, promise: Promise<T>): Promise<UmbDataSourceResponse<T>> {
	const controller = new UmbTryExecuteController(host, promise);
	const response = await controller.tryExecute();
	controller.destroy();
	return response as UmbDataSourceResponse<T>;
}
