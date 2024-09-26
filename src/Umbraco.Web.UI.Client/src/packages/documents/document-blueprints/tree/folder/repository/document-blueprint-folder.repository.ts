import { UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_CONTEXT } from './document-blueprint-folder.store.context-token.js';
import { UmbDocumentBlueprintFolderServerDataSource } from './document-blueprint-folder.server.data-source.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentBlueprintFolderRepository extends UmbDetailRepositoryBase<UmbFolderModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintFolderServerDataSource, UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_CONTEXT);
	}
}

export { UmbDocumentBlueprintFolderRepository as api };
