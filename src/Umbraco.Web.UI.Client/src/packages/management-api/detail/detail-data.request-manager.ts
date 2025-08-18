import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiDetailDataCache } from './cache.js';
import {
	tryExecute,
	type UmbApiError,
	type UmbCancelError,
	type UmbApiResponse,
	type UmbApiWithErrorResponse,
} from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbManagementApiDetailDataRequestManagerArgs<
	DetailResponseModelType,
	CreateRequestModelType,
	UpdateRequestModelType,
> {
	create: (data: CreateRequestModelType) => Promise<UmbApiResponse<{ data: unknown }>>;
	read: (id: string) => Promise<UmbApiResponse<{ data: DetailResponseModelType }>>;
	update: (id: string, data: UpdateRequestModelType) => Promise<UmbApiResponse<{ data: unknown }>>;
	delete: (id: string) => Promise<UmbApiResponse<{ data: unknown }>>;
	dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	serverEventSource: string;
}

export class UmbManagementApiDetailDataRequestManager<
	DetailResponseModelType,
	CreateRequestModelType,
	UpdateRequestModelType,
> extends UmbControllerBase {
	#dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	#serverEventSource: string;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;

	#create;
	#read;
	#update;
	#delete;
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

		this.#dataCache = args.dataCache;
		this.#serverEventSource = args.serverEventSource;

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

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents && this.#dataCache.has(id)) {
			data = this.#dataCache.get(id);
		} else {
			const { data: serverData, error: serverError } = await tryExecute(this, this.#read(id));

			if (this.#isConnectedToServerEvents && serverData) {
				this.#dataCache.set(id, serverData);
			}

			data = serverData;
			error = serverError;
		}

		return { data, error };
	}

	async update(id: string, data: UpdateRequestModelType): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		const { error } = await tryExecute(this, this.#update(id, data));

		if (!error) {
			return this.read(id);
		}

		return { error };
	}

	async delete(id: string): Promise<UmbApiWithErrorResponse> {
		const { error } = await this.#delete(id);

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

		// Invalidate cache entries when entities are updated or deleted
		this.observe(
			this.#serverEventContext?.byEventSourceAndTypes(this.#serverEventSource, ['Updated', 'Deleted']),
			(event) => {
				if (!event) return;
				this.#dataCache.delete(event.key);
			},
			'umbObserveServerEvents',
		);
	}
}
