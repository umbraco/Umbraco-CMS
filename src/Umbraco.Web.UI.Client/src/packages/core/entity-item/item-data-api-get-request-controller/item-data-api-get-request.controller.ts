import type { UmbItemDataApiGetRequestControllerArgs } from './types.js';
import {
	batchTryExecute,
	tryExecute,
	UmbError,
	type UmbApiError,
	type UmbCancelError,
	type UmbDataApiResponse,
} from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { batchArray } from '@umbraco-cms/backoffice/utils';
import { umbPeekError } from '@umbraco-cms/backoffice/notification';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbItemDataApiGetRequestController<
	ResponseModelType extends UmbDataApiResponse,
> extends UmbControllerBase {
	#apiCallback: (args: { uniques: Array<string> }) => Promise<ResponseModelType>;
	#uniques: Array<string>;
	#batchSize: number = 1;

	constructor(host: UmbControllerHost, args: UmbItemDataApiGetRequestControllerArgs<ResponseModelType>) {
		super(host);
		this.#apiCallback = args.api;
		this.#uniques = args.uniques;
	}

	async request() {
		if (!this.#uniques) throw new Error('Uniques are missing');

		let data: ResponseModelType['data'] | undefined;
		let error: UmbError | UmbApiError | UmbCancelError | Error | undefined;

		if (this.#uniques.length > this.#batchSize) {
			const chunks = batchArray<string>(this.#uniques, this.#batchSize);
			const results = await batchTryExecute(this, chunks, (chunk) => this.#apiCallback({ uniques: chunk }));

			const errors = results.filter((promiseResult) => promiseResult.status === 'rejected');

			if (errors.length > 0) {
				error = await this.#getAndHandleErrorResult(errors);
			}

			data = results
				.filter((promiseResult) => promiseResult.status === 'fulfilled')
				.flatMap((promiseResult) => promiseResult.value.data);
		} else {
			const result = await tryExecute(this, this.#apiCallback({ uniques: this.#uniques }));
			data = result.data;
			error = result.error;
		}

		return { data, error };
	}

	async #getAndHandleErrorResult(errors: Array<PromiseRejectedResult>) {
		// TODO: We currently expect all the errors to be the same, but we should handle this better in the future.
		const error = errors[0];
		await umbPeekError(this, {
			headline: 'Error fetching items',
			message: 'An error occurred while fetching items.',
		});

		return new UmbError(error.reason);
	}
}
