import { UmbScriptFolderServerDataSource } from './script-folder.server.data-source.js';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbScriptFolderServerDataSource);
	}
}

export default UmbScriptFolderRepository;
