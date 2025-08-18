import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiItemDataCache } from './cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbManagementApiItemDataInvalidationManagerArgs<ItemResponseModelType> {
	dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	serverEventSources: Array<string>;
}

export class UmbManagementApiItemDataCacheInvalidationManager<ItemResponseModelType> extends UmbControllerBase {
	#dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	#serverEventSources: Array<string>;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbManagementApiItemDataInvalidationManagerArgs<ItemResponseModelType>) {
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
