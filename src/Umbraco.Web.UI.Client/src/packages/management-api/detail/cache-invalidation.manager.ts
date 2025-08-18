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
