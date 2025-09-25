import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiItemDataCache } from './cache.js';
import type { UmbApiError, UmbCancelError, UmbApiResponse } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';

export interface UmbManagementApiItemDataRequestManagerArgs<ItemResponseModelType> {
	getItems: (unique: Array<string>) => Promise<UmbApiResponse<{ data: Array<ItemResponseModelType> }>>;
	dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	getUniqueMethod: (item: ItemResponseModelType) => string;
}

export class UmbManagementApiItemDataRequestManager<ItemResponseModelType> extends UmbControllerBase {
	#dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
	getUniqueMethod: (item: ItemResponseModelType) => string;

	#getItems;
	#isConnectedToServerEvents = false;

	constructor(host: UmbControllerHost, args: UmbManagementApiItemDataRequestManagerArgs<ItemResponseModelType>) {
		super(host);

		this.#getItems = args.getItems;
		this.#dataCache = args.dataCache;
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
		let serverItems: Array<ItemResponseModelType> | undefined;

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents) {
			const cachedIds = ids.filter((id) => this.#dataCache.has(id));
			cacheItems = cachedIds
				.map((id) => this.#dataCache.get(id))
				.filter((x) => x !== undefined) as Array<ItemResponseModelType>;
			idsToRequest = ids.filter((id) => !this.#dataCache.has(id));
		}

		if (idsToRequest.length > 0) {
			const getItemsController = new UmbItemDataApiGetRequestController(this, {
				api: (args) => this.#getItems(args.uniques),
				uniques: idsToRequest,
			});

			const { data: serverData, error: serverError } = await getItemsController.request();

			serverItems = serverData ?? [];
			error = serverError;

			if (this.#isConnectedToServerEvents) {
				// If we are connected to server events, we can cache the server data
				serverItems?.forEach((item) => this.#dataCache.set(this.getUniqueMethod(item), item));
			}
		}

		const data: Array<ItemResponseModelType> = [...cacheItems, ...(serverItems ?? [])];

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
