import { UmbResourceController } from './resource.controller.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbTryExecuteController extends UmbResourceController {
	async tryExecute<T>(): Promise<UmbDataSourceResponse<T>> {
		try {
			return { data: await this._promise };
		} catch (error) {
			// Error might be a legacy error, so we need to check if it is an UmbError
			return {
				error: this.mapToUmbError(error),
			};
		}
	}
}
