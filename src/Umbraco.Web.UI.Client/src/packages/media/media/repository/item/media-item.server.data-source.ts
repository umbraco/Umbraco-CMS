import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaItemModel } from './types.js';
import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute, UmbApiError, UmbCancelError, UmbError } from '@umbraco-cms/backoffice/resources';
import { batchArray } from '@umbraco-cms/backoffice/utils';

/**
 * A data source for Media items that fetches data from the server
 * @class UmbMediaItemServerDataSource
 * @implements {MediaTreeDataSource}
 */
export class UmbMediaItemServerDataSource extends UmbItemServerDataSourceBase<
	MediaItemResponseModel,
	UmbMediaItemModel
> {
	#host: UmbControllerHost;
	/**
	 * Creates an instance of UmbMediaItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
		this.#host = host;
	}

	/**
	 * @deprecated - The search method will be removed in v17. Use the
	 * Use the UmbMediaSearchProvider instead.
	 * Get it from:
	 * ```ts
	 * import { UmbMediaSearchProvider } from '@umbraco-cms/backoffice/media';
	 * ```
	 */
	async search({ query, skip, take }: { query: string; skip: number; take: number }) {
		const { data, error } = await tryExecute(
			this.#host,
			MediaService.getItemMediaSearch({ query: { query, skip, take } }),
		);
		const mapped = data?.items.map((item) => mapper(item));
		return { data: mapped, error };
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		let data: Array<MediaItemResponseModel> | undefined;
		let error: UmbError | UmbApiError | UmbCancelError | Error | undefined;

		const batchSize = 40;
		if (uniques.length > batchSize) {
			const chunks = batchArray<string>(uniques, batchSize);
			const results = await batchTryExecute(this.#host, chunks, (chunk) =>
				// eslint-disable-next-line local-rules/no-direct-api-import
				MediaService.getItemMedia({ query: { id: chunk } }),
			);

			const errors = results.filter((promiseResult) => promiseResult.status === 'rejected');

			if (errors.length > 0) {
				// TODO: investigate if its ok to only return the first error
				error = errors[0].reason;
			}

			data = results
				.filter((promiseResult) => promiseResult.status === 'fulfilled')
				.flatMap((promiseResult) => promiseResult.value.data);
		} else {
			const result = await tryExecute(this.#host, MediaService.getItemMedia({ query: { id: uniques } }));
			data = result.data;
			error = result.error;
		}

		if (data) {
			const items = data.map((item) => mapper(item));
			return { data: items };
		}

		return { error };
	}
}

const mapper = (item: MediaItemResponseModel): UmbMediaItemModel => {
	return {
		entityType: UMB_MEDIA_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isTrashed: item.isTrashed,
		unique: item.id,
		mediaType: {
			unique: item.mediaType.id,
			icon: item.mediaType.icon,
			collection: item.mediaType.collection ? { unique: item.mediaType.collection.id } : null,
		},
		name: item.variants[0]?.name, // TODO: get correct variant name
		parent: item.parent ? { unique: item.parent.id } : null,
		variants: item.variants.map((variant) => {
			return {
				culture: variant.culture || null,
				name: variant.name,
			};
		}),
	};
};

/**
 * Batches promises and returns a promise that resolves to an array of results
 * @param {Array<Array<BatchEntryType>>} chunks - The array of chunks to process
 * @param {(chunk: Array<BatchEntryType>) => Promise<PromiseResult>} callback - The function to call for each chunk
 * @returns {Promise<PromiseSettledResult<PromiseResult>[]>} - A promise that resolves to an array of results
 */
function batchPromises<BatchEntryType, PromiseResult>(
	chunks: Array<Array<BatchEntryType>>,
	callback: (chunk: Array<BatchEntryType>) => Promise<PromiseResult>,
): Promise<PromiseSettledResult<PromiseResult>[]> {
	return Promise.allSettled(chunks.map((chunk) => callback(chunk)));
}

/**
 * Batches promises and returns a promise that resolves to an array of results
 * @param {UmbControllerHost} host - The host to use for the request and where notifications will be shown
 * @param {Array<Array<BatchEntryType>>} chunks - The array of chunks to process
 * @param {(chunk: Array<BatchEntryType>) => Promise<PromiseResult>} callback - The function to call for each chunk
 * @returns {Promise<PromiseSettledResult<PromiseResult>[]>} - A promise that resolves to an array of results
 */
function batchTryExecute<BatchEntryType, PromiseResult>(
	host: UmbControllerHost,
	chunks: Array<Array<BatchEntryType>>,
	callback: (chunk: Array<BatchEntryType>) => Promise<PromiseResult>,
): Promise<PromiseSettledResult<PromiseResult>[]> {
	return batchPromises(chunks, (chunk) => tryExecute(host, callback(chunk)));
}
