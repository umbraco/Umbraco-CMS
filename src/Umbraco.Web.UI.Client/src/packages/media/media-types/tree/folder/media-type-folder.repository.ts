import { UmbMediaTypeFolderServerDataSource } from './media-type-folder.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbMediaTypeFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeFolderServerDataSource);
	}
}

export default UmbMediaTypeFolderRepository;
