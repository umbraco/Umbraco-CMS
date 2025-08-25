import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiInFlightRequestCache } from '../inflight-request/cache.js';
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
	inflightRequestCache: UmbManagementApiInFlightRequestCache<DetailResponseModelType>;
}

export class UmbManagementApiDetailDataRequestManager<
	DetailResponseModelType,
	CreateRequestModelType,
	UpdateRequestModelType,
> extends UmbControllerBase {
	#dataCache: UmbManagementApiDetailDataCache<DetailResponseModelType>;
	#inflightRequestCache: UmbManagementApiInFlightRequestCache<DetailResponseModelType>;

	#create;
	#read;
	#update;
	#delete;
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
	}
}
