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
	UmbManagementApiDetailDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDocumentTypeDetailDataRequestManager extends UmbManagementApiDetailDataRequestManager<
	DocumentTypeResponseModel,
	UpdateDocumentTypeRequestModel,
	CreateDocumentTypeRequestModel
> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<DocumentTypeResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			create: (body: CreateDocumentTypeRequestModel) => DocumentTypeService.postDocumentType({ body }),
			read: (id: string) => DocumentTypeService.getDocumentTypeById({ path: { id } }),
			update: (id: string, body: UpdateDocumentTypeRequestModel) =>
				DocumentTypeService.putDocumentTypeById({ path: { id }, body }),
			delete: (id: string) => DocumentTypeService.deleteDocumentTypeById({ path: { id } }),
			readMany: (ids: Array<string>) => DocumentTypeService.getDocumentTypeFetch({ query: { id: ids } }),
			dataCache: documentTypeDetailCache,
			inflightRequestCache: UmbManagementApiDocumentTypeDetailDataRequestManager.#inflightRequestCache,
		});
	}
}
