import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiDetailDataCache } from './cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbManagementApiDetailDataInvalidationManagerArgs<DetailResponseModelType> {
	dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	serverEventSources: Array<string>;
}

export class UmbManagementApiDetailDataCacheInvalidationManager<DetailResponseModelType> extends UmbControllerBase {
	#dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	#serverEventSources: Array<string>;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
	#isConnectedToServerEvents = false;

	constructor(
		host: UmbControllerHost,
		args: UmbManagementApiDetailDataInvalidationManagerArgs<DetailResponseModelType>,
	) {
		super(host);
		{
			this.#dataCache = args.dataCache;
			this.#serverEventSources = args.serverEventSources;

			this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
				this.#serverEventContext = context;
				this.#observeServerEvents();
			});
		}
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

		// Invalidate cache entries when entities are updated or deleted
		this.observe(
			this.#serverEventContext?.byEventSourcesAndTypes(this.#serverEventSources, ['Updated', 'Deleted']),
			(event) => {
				if (!event) return;
				this.#dataCache.delete(event.key);
			},
			'umbObserveServerEvents',
		);
	}

	override destroy(): void {
		this.#dataCache.clear();
	}
}
