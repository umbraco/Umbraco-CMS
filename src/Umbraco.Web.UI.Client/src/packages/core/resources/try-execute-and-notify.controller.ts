import { isApiError, isCancelError } from './apiTypeValidators.function.js';
import { UmbResourceController } from './resource.controller.js';
import { UmbApiError, UmbCancelError } from './umb-error.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbTryExecuteAndNotifyController extends UmbResourceController {
	override async tryExecuteAndNotify<T>(): Promise<UmbDataSourceResponse<T>> {
		try {
			return { data: await this._promise };
		} catch (error) {
			// Error might be a legacy error, so we need to check if it is an UmbError
			let umbError = isApiError(error) ? UmbApiError.fromLegacyApiError(error) : error;
			umbError = isCancelError(umbError) ? UmbCancelError.fromLegacyCancelError(umbError) : umbError;
			return this.handleUmbErrors(umbError, true);
		}
	}
}
