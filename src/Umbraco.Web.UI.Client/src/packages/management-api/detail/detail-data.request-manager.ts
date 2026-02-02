import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiInFlightRequestCache } from '../inflight-request/cache.js';
import type { UmbManagementApiDetailDataCache } from './cache.js';
import {
	tryExecute,
	type UmbApiError,
	type UmbCancelError,
	type UmbApiResponse,
	type UmbApiWithErrorResponse,
	type UmbDataApiResponse,
} from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';

export interface UmbManagementApiDetailDataRequestManagerArgs<
	DetailResponseModelType,
	CreateRequestModelType,
	UpdateRequestModelType,
> {
	create: (data: CreateRequestModelType) => Promise<UmbApiResponse<{ data: unknown }>>;
	read: (id: string) => Promise<UmbApiResponse<{ data: DetailResponseModelType }>>;
	update: (id: string, data: UpdateRequestModelType) => Promise<UmbApiResponse<{ data: unknown }>>;
	delete: (id: string) => Promise<UmbApiResponse<{ data: unknown }>>;
	readMany?: (ids: Array<string>) => Promise<UmbDataApiResponse<{ data: { items: Array<DetailResponseModelType> } }>>;
	dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	inflightRequestCache: UmbManagementApiInFlightRequestCache<DetailResponseModelType>;
}

export class UmbManagementApiDetailDataRequestManager<
	DetailResponseModelType extends { id: string },
	CreateRequestModelType,
	UpdateRequestModelType,
> extends UmbControllerBase {
	#dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	#inflightRequestCache: UmbManagementApiInFlightRequestCache<DetailResponseModelType>;

	#create;
	#read;
	#update;
	#delete;
	#readMany;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
	#isConnectedToServerEvents = false;

	constructor(
		host: UmbControllerHost,
		args: UmbManagementApiDetailDataRequestManagerArgs<
			DetailResponseModelType,
			CreateRequestModelType,
			UpdateRequestModelType
		>,
	) {
		super(host);

		this.#create = args.create;
		this.#read = args.read;
		this.#update = args.update;
		this.#delete = args.delete;
		this.#readMany = args.readMany;

		this.#dataCache = args.dataCache;
		this.#inflightRequestCache = args.inflightRequestCache;

		this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
			this.#serverEventContext = context;
			this.#observeServerEvents();
		});
	}

	async create(data: CreateRequestModelType): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		const { data: createdId, error } = await tryExecute(this, this.#create(data));

		if (!error) {
			return this.read(createdId as string);
		}

		return { error };
	}

	async read(id: string): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		let data: DetailResponseModelType | undefined;
		let error: UmbApiError | UmbCancelError | undefined;

		const inflightCacheKey = `read:${id}`;

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents && this.#dataCache.has(id)) {
			data = this.#dataCache.get(id);
		} else {
			const hasInflightRequest = this.#inflightRequestCache.has(inflightCacheKey);

			const request = hasInflightRequest
				? this.#inflightRequestCache.get(inflightCacheKey)?.requestPromise
				: tryExecute(this, this.#read(id));

			if (!request) {
				throw new Error(
					`Failed to create or retrieve 'read' request for ID: ${id} (cache key: ${inflightCacheKey}). Aborting read.`,
				);
			}

			this.#inflightRequestCache.set(inflightCacheKey, request);

			try {
				const { data: serverData, error: serverError } = await request;

				if (this.#isConnectedToServerEvents && serverData) {
					this.#dataCache.set(id, serverData);
				}

				data = serverData;
				error = serverError;
			} finally {
				this.#inflightRequestCache.delete(inflightCacheKey);
			}
		}

		return { data, error };
	}

	/**
	 * Reads multiple items by their IDs.
	 * Only available if a readMany function was provided in the constructor args.
	 * @param {Array<string>} ids - The IDs of the items to read
	 * @returns {Promise<UmbApiResponse<{ data?: { items: Array<DetailResponseModelType> } }>>}
	 */
	async readMany(ids: Array<string>): Promise<UmbApiResponse<{ data?: { items: Array<DetailResponseModelType> } }>> {
		if (!this.#readMany) {
			throw new Error('readMany is not available. No readMany function was provided in the constructor args.');
		}

		let error: UmbApiError | UmbCancelError | undefined;
		let idsToRequest: Array<string> = [...ids];
		let cacheItems: Array<DetailResponseModelType> = [];
		let serverItems: Array<DetailResponseModelType> | undefined;

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents) {
			const cachedIds = ids.filter((id) => this.#dataCache.has(id));
			cacheItems = cachedIds
				.map((id) => this.#dataCache.get(id))
				.filter((x) => x !== undefined) as Array<DetailResponseModelType>;
			idsToRequest = ids.filter((id) => !this.#dataCache.has(id));
		}

		if (idsToRequest.length > 0) {
			const getItemsController = new UmbItemDataApiGetRequestController(this, {
				api: (args) => this.#readMany!(args.uniques),
				uniques: idsToRequest,
			});

			const { data: serverData, error: serverError } = await getItemsController.request();

			serverItems = serverData.items ?? [];
			error = serverError;

			if (this.#isConnectedToServerEvents) {
				// If we are connected to server events, we can cache the server data
				serverItems?.forEach((item) => this.#dataCache.set(item.id, item));
			}
		}

		const items: Array<DetailResponseModelType> = [...cacheItems, ...(serverItems ?? [])];

		return { data: { items }, error };
	}

	async update(id: string, data: UpdateRequestModelType): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		const { error } = await tryExecute(this, this.#update(id, data));

		if (!error) {
			return this.read(id);
		}

		return { error };
	}

	async delete(id: string): Promise<UmbApiWithErrorResponse> {
		const { error } = await tryExecute(this, this.#delete(id));

		// Only update the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents && !error) {
			this.#dataCache.delete(id);
		}

		return { error };
	}

	#observeServerEvents() {
		this.observe(
			this.#serverEventContext?.isConnected,
			(isConnected) => {
				/* We purposefully ignore the initial value of isConnected.
				We only care about whether the connection is established or not (true/false) */
				if (isConnected === undefined) return;
				this.#isConnectedToServerEvents = isConnected;

				// Clear the cache if we lose connection to the server events
				if (this.#isConnectedToServerEvents === false) {
					this.#dataCache.clear();
				}
			},
			'umbObserveServerEventsConnection',
		);
	}
}
