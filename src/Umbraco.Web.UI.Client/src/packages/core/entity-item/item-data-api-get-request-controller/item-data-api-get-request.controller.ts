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
	#disableNotifications: boolean;
	#batchSize: number = 40;

	constructor(host: UmbControllerHost, args: UmbItemDataApiGetRequestControllerArgs<ResponseModelType>) {
		super(host);
		this.#apiCallback = args.api;
		this.#uniques = args.uniques;
		this.#disableNotifications = args.disableNotifications ?? false;
	}

	async request() {
		if (!this.#uniques) throw new Error('Uniques are missing');

		let data: ResponseModelType['data'] | undefined;
		let error: UmbError | UmbApiError | UmbCancelError | Error | undefined;

		if (this.#uniques.length > this.#batchSize) {
			const chunks = batchArray<string>(this.#uniques, this.#batchSize);

			// batchTryExecute wraps each chunk in tryExecute, but its declared return type omits that wrapper, so the
			// error it resolves with is not visible without restating the shape here.
			const results = (await batchTryExecute(this, chunks, (chunk) => this.#apiCallback({ uniques: chunk }))) as Array<
				PromiseSettledResult<{ data?: ResponseModelType['data']; error?: UmbApiError | UmbCancelError }>
			>;

			// A failing chunk resolves rather than rejects, because batchTryExecute wraps each one in tryExecute.
			// Both shapes therefore have to be read to find out what actually failed.
			const errors = results
				.map((promiseResult) =>
					promiseResult.status === 'rejected' ? promiseResult.reason : promiseResult.value.error,
				)
				.filter((chunkError) => chunkError !== undefined);

			// A chunk that failed contributes no data, so the successful chunks are still returned alongside the error.
			data = results
				.filter((promiseResult) => promiseResult.status === 'fulfilled')
				.flatMap((promiseResult) => promiseResult.value.data)
				.filter((item: unknown) => item !== undefined);

			if (errors.length > 0) {
				error = await this.#getAndHandleErrorResult(errors);
			}
		} else {
			const result = await tryExecute(this, this.#apiCallback({ uniques: this.#uniques }), {
				disableNotifications: this.#disableNotifications,
			});
			data = result.data;
			error = result.error;
		}

		return { data, error };
	}

	async #getAndHandleErrorResult(errors: Array<unknown>) {
		// TODO: We currently expect all the errors to be the same, but we should handle this better in the future.
		const error = errors[0];

		if (this.#disableNotifications === false) {
			await umbPeekError(this, {
				headline: 'Error fetching items',
				message: 'An error occurred while fetching items.',
			});
		}

		return new UmbError(error instanceof Error ? error.message : String(error));
	}
}
