/* eslint-disable local-rules/no-direct-api-import */
import { documentTypeDetailCache } from './document-type-detail.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentTypeService,
	type CreateDocumentTypeRequestModel,
	type DocumentTypeResponseModel,
	type UpdateDocumentTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT,
	UmbManagementApiDetailDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';
import type { UmbApiError, UmbApiResponse, UmbCancelError } from '@umbraco-cms/backoffice/resources';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';

export class UmbManagementApiDocumentTypeDetailDataRequestManager extends UmbManagementApiDetailDataRequestManager<
	DocumentTypeResponseModel,
	UpdateDocumentTypeRequestModel,
	CreateDocumentTypeRequestModel
> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<DocumentTypeResponseModel>();

	#serverEventContext?: typeof UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT.TYPE;
	#isConnectedToServerEvents = false;

	constructor(host: UmbControllerHost) {
		super(host, {
			create: (body: CreateDocumentTypeRequestModel) => DocumentTypeService.postDocumentType({ body }),
			read: (id: string) => DocumentTypeService.getDocumentTypeById({ path: { id } }),
			update: (id: string, body: UpdateDocumentTypeRequestModel) =>
				DocumentTypeService.putDocumentTypeById({ path: { id }, body }),
			delete: (id: string) => DocumentTypeService.deleteDocumentTypeById({ path: { id } }),
			dataCache: documentTypeDetailCache,
			inflightRequestCache: UmbManagementApiDocumentTypeDetailDataRequestManager.#inflightRequestCache,
		});

		this.consumeContext(UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT, (context) => {
			this.#serverEventContext = context;
			this.#observeServerEventsConnection();
		});
	}

	/**
	 * Reads multiple document types by their IDs
	 * @param {Array<string>} ids - The IDs of the document types to read
	 * @returns {Promise<UmbApiResponse<{ data?: Array<DocumentTypeResponseModel> }>>}
	 */
	async readMany(ids: Array<string>): Promise<UmbApiResponse<{ data?: Array<DocumentTypeResponseModel> }>> {
		let error: UmbApiError | UmbCancelError | undefined;
		let idsToRequest: Array<string> = [...ids];
		let cacheItems: Array<DocumentTypeResponseModel> = [];
		let serverItems: Array<DocumentTypeResponseModel> | undefined;

		// Only read from the cache when we are connected to the server events
		if (this.#isConnectedToServerEvents) {
			const cachedIds = ids.filter((id) => documentTypeDetailCache.has(id));
			cacheItems = cachedIds
				.map((id) => documentTypeDetailCache.get(id))
				.filter((x) => x !== undefined) as Array<DocumentTypeResponseModel>;
			idsToRequest = ids.filter((id) => !documentTypeDetailCache.has(id));
		}

		if (idsToRequest.length > 0) {
			const getItemsController = new UmbItemDataApiGetRequestController(this, {
				api: (args) => DocumentTypeService.getDocumentTypeFetch({ query: { id: args.uniques } }),
				uniques: idsToRequest,
			});

			const { data: serverData, error: serverError } = await getItemsController.request();

			serverItems = serverData?.items ?? [];
			error = serverError;

			if (this.#isConnectedToServerEvents) {
				// If we are connected to server events, we can cache the server data
				serverItems?.forEach((item) => documentTypeDetailCache.set(item.id, item));
			}
		}

		const data: Array<DocumentTypeResponseModel> = [...cacheItems, ...(serverItems ?? [])];

		return { data, error };
	}

	#observeServerEventsConnection() {
		this.observe(
			this.#serverEventContext?.isConnected,
			(isConnected) => {
				if (isConnected === undefined) return;
				this.#isConnectedToServerEvents = isConnected;
			},
			'umbObserveServerEventsConnectionForReadMany',
		);
	}
}
