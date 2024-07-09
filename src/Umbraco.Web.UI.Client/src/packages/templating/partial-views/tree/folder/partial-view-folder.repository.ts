import { UmbPartialViewFolderServerDataSource } from './partial-view-folder.server.data-source.js';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPartialViewFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewFolderServerDataSource);
	}
}

export { UmbPartialViewFolderRepository as api };
