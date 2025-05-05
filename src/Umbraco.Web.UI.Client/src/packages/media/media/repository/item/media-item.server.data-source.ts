import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaItemModel } from './types.js';
import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import {
	UmbError,
	type UmbApiError,
	type UmbCancelError,
	type UmbApiDataResponse,
} from '@umbraco-cms/backoffice/resources';
import { batchTryExecute, tryExecute } from '@umbraco-cms/backoffice/resources';
import { batchArray } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbPeekError } from '@umbraco-cms/backoffice/notification';

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

		const itemRequestManager = new UmbDataApiItemGetRequestController(this.#host, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => MediaService.getItemMedia({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

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

interface UmbDataApiItemGetRequestControllerArgs<ResponseModelType extends UmbApiDataResponse> {
	api: (args: { uniques: Array<string> }) => Promise<ResponseModelType>;
	uniques: Array<string>;
}
class UmbDataApiItemGetRequestController<ResponseModelType extends UmbApiDataResponse> extends UmbControllerBase {
	#apiCallback: (args: { uniques: Array<string> }) => Promise<ResponseModelType>;
	#uniques: Array<string>;
	#batchSize: number = 1;

	constructor(host: UmbControllerHost, args: UmbDataApiItemGetRequestControllerArgs<ResponseModelType>) {
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
