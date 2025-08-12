import { UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT } from '../server-event/constants.js';
import type { UmbManagementApiRuntimeCache } from '../runtime-cache/index.js';
import {
	tryExecute,
	type UmbApiError,
	type UmbCancelError,
	type UmbApiResponse,
	type UmbApiWithErrorResponse,
} from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbManagementApiDetailDataRequestManagerArgs<DetailResponseModelType, UpdateRequestModelType> {
	read: (id: string) => Promise<UmbApiResponse<{ data: DetailResponseModelType }>>;
	update: (id: string, data: UpdateRequestModelType) => Promise<UmbApiResponse<{ data: unknown }>>;
	delete: (id: string) => Promise<UmbApiResponse<{ data: unknown }>>;
	cache: UmbManagementApiRuntimeCache<DetailResponseModelType>;
	serverEventSource: string;
}

export class UmbManagementApiDetailDataRequestManager<
	DetailResponseModelType,
	UpdateRequestModelType,
> extends UmbControllerBase {
	#cache: UmbManagementApiRuntimeCache<DetailResponseModelType>;
	#serverEventSource: string;
	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;

	#read;
	#update;
	#delete;

	constructor(
		host: UmbControllerHost,
		args: UmbManagementApiDetailDataRequestManagerArgs<DetailResponseModelType, UpdateRequestModelType>,
	) {
		super(host);

		this.#read = args.read;
		this.#update = args.update;
		this.#delete = args.delete;

		this.#cache = args.cache;
		this.#serverEventSource = args.serverEventSource;

		this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
			this.#serverEventContext = context;
			this.#observeServerEvents();
		});
	}

	async read(id: string): Promise<UmbApiResponse<{ data?: DetailResponseModelType }>> {
		let data: DetailResponseModelType | undefined;
		let error: UmbApiError | UmbCancelError | undefined;

		if (this.#cache.has(id)) {
			data = this.#cache.get(id);
		} else {
			const { data: serverData, error: serverError } = await tryExecute(this, this.#read(id));

			if (serverData) {
				this.#cache.set(id, serverData);
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

		if (!error) {
			this.#cache.delete(id);
		}

		return { error };
	}

	#observeServerEvents() {
		this.observe(
			this.#serverEventContext?.byEventSourceAndTypes(this.#serverEventSource, ['Updated', 'Deleted']),
			(event) => {
				if (!event) return;
				this.#invalidateCache(event.key);
			},
		);
	}

	#invalidateCache(id: string) {
		this.#cache.delete(id);
	}
}
