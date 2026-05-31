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

	/**
	 * Creates a new item and returns the created item data.
	 * @param {CreateRequestModelType} data - The data to create the item with.
	 * @returns {Promise<UmbApiResponse<{ data?: DetailResponseModelType }>>} The API response containing the created item or an error.
	 */
	async create(data: CreateRequestModelType): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		const { data: createdId, error } = await tryExecute(this, this.#create(data));

		if (!error) {
			return this.read(createdId as string);
		}

		return { error };
	}

	/**
	 * Reads a single item by its ID.
	 * Uses the cache if connected to server events and the item is cached, otherwise fetches from the server.
	 * Deduplicates concurrent requests for the same ID using an in-flight request cache.
	 * @param {string} id - The ID of the item to read.
	 * @returns {Promise<UmbApiResponse<{ data?: DetailResponseModelType }>>} The API response containing the item or an error.
	 */
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
	 * Deduplicates concurrent requests: if an ID is already being fetched by another
	 * concurrent readMany call, it waits for that request instead of re-fetching.
	 * @param {Array<string>} ids - The IDs of the items to read
	 * @returns {Promise<UmbApiResponse<{ data?: { items: Array<DetailResponseModelType> } }>>} - The API response containing the items or an error
	 */
	async readMany(ids: Array<string>): Promise<UmbApiResponse<{ data?: { items: Array<DetailResponseModelType> } }>> {
		if (!this.#readMany) {
			throw new Error('readMany is not available. No readMany function was provided in the constructor args.');
		}

		let error: UmbApiError | UmbCancelError | undefined;
		let idsToRequest: Array<string> = [...ids];
		let cacheItems: Array<DetailResponseModelType> = [];

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents) {
			const cachedIds = ids.filter((id) => this.#dataCache.has(id));
			cacheItems = cachedIds
				.map((id) => this.#dataCache.get(id))
				.filter((x) => x !== undefined) as Array<DetailResponseModelType>;
			idsToRequest = ids.filter((id) => !this.#dataCache.has(id));
		}

		// Split remaining IDs into those already inflight vs those needing a new request
		const inflightPromises: Array<Promise<UmbApiResponse<{ data?: DetailResponseModelType }>>> = [];
		const newIds: Array<string> = [];

		for (const id of idsToRequest) {
			const inflightCacheKey = `read:${id}`;
			if (this.#inflightRequestCache.has(inflightCacheKey)) {
				inflightPromises.push(this.#inflightRequestCache.get(inflightCacheKey)!.requestPromise);
			} else {
				newIds.push(id);
			}
		}

		// For new IDs, create per-item deferred promises and store in inflight cache before making the API call
		const deferredMap = new Map<
			string,
			{ resolve: (value: UmbApiResponse<{ data?: DetailResponseModelType }>) => void }
		>();

		for (const id of newIds) {
			const inflightCacheKey = `read:${id}`;
			let resolve!: (value: UmbApiResponse<{ data?: DetailResponseModelType }>) => void;
			const promise = new Promise<UmbApiResponse<{ data?: DetailResponseModelType }>>((r) => {
				resolve = r;
			});
			deferredMap.set(id, { resolve });
			this.#inflightRequestCache.set(inflightCacheKey, promise);
		}

		// Fetch new IDs from the server
		let newlyFetchedItems: Array<DetailResponseModelType> = [];

		if (newIds.length > 0) {
			try {
				const getItemsController = new UmbItemDataApiGetRequestController(this, {
					api: (args) => this.#readMany!(args.uniques),
					uniques: newIds,
				});

				const { data: serverData, error: serverError } = await getItemsController.request();
				const serverItems = serverData?.items ?? [];
				error = serverError;

				if (this.#isConnectedToServerEvents) {
					serverItems.forEach((item) => this.#dataCache.set(item.id, item));
				}

				newlyFetchedItems = serverItems;

				// Resolve each deferred promise with the corresponding item
				for (const id of newIds) {
					const item = serverItems.find((serverItem) => serverItem.id === id);
					deferredMap.get(id)!.resolve({ data: item, error: serverError });
				}
			} catch (e) {
				// If the batch call throws, resolve all deferred promises with the error
				for (const id of newIds) {
					deferredMap.get(id)!.resolve({ data: undefined, error: e as UmbApiError | UmbCancelError });
				}
			} finally {
				// Always clean up inflight cache entries
				for (const id of newIds) {
					this.#inflightRequestCache.delete(`read:${id}`);
				}
			}
		}

		// Await inflight results from other concurrent readMany calls
		let inflightItems: Array<DetailResponseModelType> = [];

		if (inflightPromises.length > 0) {
			const inflightResults = await Promise.all(inflightPromises);
			inflightItems = inflightResults
				.map((result) => result.data)
				.filter((x): x is DetailResponseModelType => x !== undefined);

			// Propagate the first inflight error if we don't already have one from our own batch
			if (!error) {
				error = inflightResults.find((result) => result.error)?.error;
			}
		}

		const items: Array<DetailResponseModelType> = [...cacheItems, ...inflightItems, ...newlyFetchedItems];

		return { data: { items }, error };
	}

	/**
	 * Updates an existing item and returns the updated item data.
	 * @param {string} id - The ID of the item to update.
	 * @param {UpdateRequestModelType} data - The data to update the item with.
	 * @returns {Promise<UmbApiResponse<{ data?: DetailResponseModelType }>>} The API response containing the updated item or an error.
	 */
	async update(id: string, data: UpdateRequestModelType): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		const { error } = await tryExecute(this, this.#update(id, data));

		if (!error) {
			return this.read(id);
		}

		return { error };
	}

	/**
	 * Deletes an item by its ID.
	 * If connected to server events, the item is also removed from the cache.
	 * @param {string} id - The ID of the item to delete.
	 * @returns {Promise<UmbApiWithErrorResponse>} The API response containing an error if the deletion failed.
	 */
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
