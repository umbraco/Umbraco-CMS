import { UmbDocumentTypeFolderServerDataSource } from './document-type-folder.server.data-source.js';
import { UMB_DOCUMENT_TYPE_FOLDER_STORE_CONTEXT } from './document-type-folder.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentTypeFolderRepository extends UmbDetailRepositoryBase<
	UmbFolderModel,
	UmbDocumentTypeFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeFolderServerDataSource, UMB_DOCUMENT_TYPE_FOLDER_STORE_CONTEXT);
	}
}

export { UmbDocumentTypeFolderRepository as api };
