import { UmbStylesheetFolderServerDataSource } from './stylesheet-folder.server.data-source.js';
import { UMB_STYLESHEET_FOLDER_STORE_CONTEXT } from './stylesheet-folder.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbStylesheetFolderRepository extends UmbDetailRepositoryBase<
	UmbFolderModel,
	UmbStylesheetFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetFolderServerDataSource, UMB_STYLESHEET_FOLDER_STORE_CONTEXT);
	}
}

export default UmbStylesheetFolderRepository;
