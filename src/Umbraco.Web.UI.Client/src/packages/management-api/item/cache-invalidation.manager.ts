import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiServerEventModel } from '../server-event/types.js';
import type { UmbManagementApiItemDataCache } from './cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbManagementApiItemDataInvalidationManagerArgs<ItemResponseModelType> {
	dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	eventSources: Array<string>;
	eventTypes?: Array<string>;
}

export class UmbManagementApiItemDataCacheInvalidationManager<ItemResponseModelType> extends UmbControllerBase {
	protected _dataCache: UmbManagementApiItemDataCache<ItemResponseModelType>;
	#eventSources: Array<string>;
	#eventTypes: Array<string>;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbManagementApiItemDataInvalidationManagerArgs<ItemResponseModelType>) {
		super(host);
		{
			this._dataCache = args.dataCache;
			this.#eventSources = args.eventSources;
			this.#eventTypes = args.eventTypes ?? ['Updated', 'Deleted'];

			this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
				this.#serverEventContext = context;
				this.#observeServerEvents();
			});
		}
	}

	/**
	 * Handles server events
	 * @protected
	 * @param {UmbManagementApiServerEventModel} event - The server event to handle
	 * @memberof UmbManagementApiItemDataCacheInvalidationManager
	 */
	protected _onServerEvent(event: UmbManagementApiServerEventModel) {
		this._dataCache.delete(event.key);
	}

	#observeServerEvents() {
		// Invalidate cache entries when entities are updated or deleted
		this.observe(
			this.#serverEventContext?.byEventSourcesAndEventTypes(this.#eventSources, this.#eventTypes),
			(event) => {
				if (!event) return;
				this._onServerEvent(event);
			},
			'umbObserveServerEvents',
		);
	}

	override destroy(): void {
		this._dataCache.clear();
	}
}
