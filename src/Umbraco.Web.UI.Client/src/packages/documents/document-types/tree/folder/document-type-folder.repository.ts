import { UmbDocumentTypeFolderServerDataSource } from './document-type-folder.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentTypeFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeFolderServerDataSource);
	}
}

export { UmbDocumentTypeFolderRepository as api };
