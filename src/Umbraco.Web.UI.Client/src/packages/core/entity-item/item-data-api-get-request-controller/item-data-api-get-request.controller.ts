import type { UmbItemDataApiGetRequestControllerArgs } from './types.js';
import {
	batchTryExecute,
	tryExecute,
	UmbError,
	type UmbApiError,
	type UmbApiResponse,
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
	#batchSize: number = 40;

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

			// batchTryExecute runs each chunk through tryExecute, which resolves a failed chunk with an error rather than
			// rejecting it.
			const responses = results.map((promiseResult) =>
				promiseResult.status === 'fulfilled'
					? (promiseResult.value as UmbApiResponse<ResponseModelType>)
					: ({ error: promiseResult.reason } as UmbApiResponse<ResponseModelType>),
			);

			const errors = responses.filter((response) => response.error).map((response) => response.error);

			if (errors.length > 0) {
				error = await this.#getAndHandleErrorResult(errors);
			}

			data = responses.filter((response) => !response.error).flatMap((response) => response.data);
		} else {
			const result = await tryExecute(this, this.#apiCallback({ uniques: this.#uniques }));
			data = result.data;
			error = result.error;
		}

		return { data, error };
	}

	async #getAndHandleErrorResult(errors: Array<unknown>) {
		// TODO: We currently expect all the errors to be the same, but we should handle this better in the future.
		const error = errors[0];
		await umbPeekError(this, {
			headline: 'Error fetching items',
			message: 'An error occurred while fetching items.',
		});

		return new UmbError(error instanceof Error ? error.message : String(error));
	}
}
