import { UmbScriptFolderServerDataSource } from './script-folder.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbScriptFolderRepository extends UmbDetailRepositoryBase<UmbFolderModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptFolderServerDataSource);
	}
}

export default UmbScriptFolderRepository;
