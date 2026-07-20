import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiInFlightRequestCache } from '../inflight-request/cache.js';
import type { UmbManagementApiItemDataCache } from './cache.js';
import type { UmbApiError, UmbCancelError, UmbApiResponse } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';

export interface UmbManagementApiItemDataRequestManagerArgs<ItemResponseModelType> {
	getItems: (unique: Array<string>) => Promise<UmbApiResponse<{ data: Array<ItemResponseModelType> }>>;
	dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	inflightRequestCache?: UmbManagementApiInFlightRequestCache<ItemResponseModelType>;
	getUniqueMethod: (item: ItemResponseModelType) => string;
}

export class UmbManagementApiItemDataRequestManager<ItemResponseModelType> extends UmbControllerBase {
	#dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	#inflightRequestCache?: UmbManagementApiInFlightRequestCache<ItemResponseModelType>;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
	getUniqueMethod: (item: ItemResponseModelType) => string;

	#getItems;
	#isConnectedToServerEvents = false;

	constructor(host: UmbControllerHost, args: UmbManagementApiItemDataRequestManagerArgs<ItemResponseModelType>) {
		super(host);

		this.#getItems = args.getItems;
		this.#dataCache = args.dataCache;
		this.#inflightRequestCache = args.inflightRequestCache;
		this.getUniqueMethod = args.getUniqueMethod;

		this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
			this.#serverEventContext = context;
			this.#observeServerEventsConnection();
		});
	}

	async getItems(ids: Array<string>): Promise<UmbApiResponse<{ data?: Array<ItemResponseModelType> }>> {
		let error: UmbApiError | UmbCancelError | undefined;
		let idsToRequest: Array<string> = [...ids];
		let cacheItems: Array<ItemResponseModelType> = [];

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents) {
			const cachedIds = ids.filter((id) => this.#dataCache.has(id));
			cacheItems = cachedIds
				.map((id) => this.#dataCache.get(id))
				.filter((x) => x !== undefined) as Array<ItemResponseModelType>;
			idsToRequest = ids.filter((id) => !this.#dataCache.has(id));
		}

		// Split remaining IDs into those already inflight vs those needing a new request
		const inflightPromises: Array<Promise<UmbApiResponse<{ data?: ItemResponseModelType }>>> = [];
		const newIds: Array<string> = [];

		for (const id of idsToRequest) {
			const inflightCacheKey = `item:${id}`;
			if (this.#inflightRequestCache?.has(inflightCacheKey)) {
				inflightPromises.push(this.#inflightRequestCache.get(inflightCacheKey)!.requestPromise);
			} else {
				newIds.push(id);
			}
		}

		// For new IDs, create per-item deferred promises and store in inflight cache before making the API call
		const deferredMap = new Map<
			string,
			{ resolve: (value: UmbApiResponse<{ data?: ItemResponseModelType }>) => void }
		>();

		for (const id of newIds) {
			const inflightCacheKey = `item:${id}`;
			let resolve!: (value: UmbApiResponse<{ data?: ItemResponseModelType }>) => void;
			const promise = new Promise<UmbApiResponse<{ data?: ItemResponseModelType }>>((r) => {
				resolve = r;
			});
			deferredMap.set(id, { resolve });
			this.#inflightRequestCache?.set(inflightCacheKey, promise);
		}

		// Fetch new IDs from the server
		let newlyFetchedItems: Array<ItemResponseModelType> = [];

		if (newIds.length > 0) {
			try {
				const getItemsController = new UmbItemDataApiGetRequestController(this, {
					api: (args) => this.#getItems(args.uniques),
					uniques: newIds,
				});

				const { data: serverData, error: serverError } = await getItemsController.request();
				const serverItems = serverData ?? [];
				error = serverError;

				if (this.#isConnectedToServerEvents) {
					serverItems.forEach((item) => this.#dataCache.set(this.getUniqueMethod(item), item));
				}

				newlyFetchedItems = serverItems;

				// Resolve each deferred promise with the corresponding item
				for (const id of newIds) {
					const item = serverItems.find((serverItem) => this.getUniqueMethod(serverItem) === id);
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
					this.#inflightRequestCache?.delete(`item:${id}`);
				}
			}
		}

		// Await inflight results from other concurrent getItems calls
		let inflightItems: Array<ItemResponseModelType> = [];

		if (inflightPromises.length > 0) {
			const inflightResults = await Promise.all(inflightPromises);
			inflightItems = inflightResults
				.map((result) => result.data)
				.filter((x): x is ItemResponseModelType => x !== undefined);

			// Propagate the first inflight error if we don't already have one from our own batch
			if (!error) {
				error = inflightResults.find((result) => result.error)?.error;
			}
		}

		const data: Array<ItemResponseModelType> = [...cacheItems, ...inflightItems, ...newlyFetchedItems];

		return { data, error };
	}

	#observeServerEventsConnection() {
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
