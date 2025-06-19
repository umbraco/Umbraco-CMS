import { UmbScriptFolderServerDataSource } from './script-folder.server.data-source.js';
import { UMB_SCRIPT_FOLDER_STORE_CONTEXT } from './script-folder.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbScriptFolderRepository extends UmbDetailRepositoryBase<UmbFolderModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptFolderServerDataSource, UMB_SCRIPT_FOLDER_STORE_CONTEXT);
	}
}

export default UmbScriptFolderRepository;
