import { UmbDocumentBlueprintFolderServerDataSource } from './document-blueprint-folder.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentBlueprintFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintFolderServerDataSource);
	}
}

export { UmbDocumentBlueprintFolderRepository as api };
