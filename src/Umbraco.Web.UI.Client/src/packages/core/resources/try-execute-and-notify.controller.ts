import { UmbResourceController } from './resource.controller.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A controller that tries to execute a promise and returns the result.
 * @internal
 * @deprecated Use the {UmbTryExecuteController} instead and handle the error in the caller.
 * This class is kept for backwards compatibility and will be removed in a future version.
 */
export class UmbTryExecuteAndNotifyController extends UmbResourceController {
	/**
	 * @internal
	 * @deprecated Use the {UmbTryExecuteController.tryExecute} instead and handle the error in the caller.
	 * This method is kept for backwards compatibility and will be removed in a future version.
	 */
	override async tryExecuteAndNotify<T>(): Promise<UmbDataSourceResponse<T>> {
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
