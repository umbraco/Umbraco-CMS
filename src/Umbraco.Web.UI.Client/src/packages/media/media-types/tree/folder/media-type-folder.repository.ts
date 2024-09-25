import { UmbMediaTypeFolderServerDataSource } from './media-type-folder.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbMediaTypeFolderRepository extends UmbDetailRepositoryBase<UmbFolderModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeFolderServerDataSource);
	}
}

export default UmbMediaTypeFolderRepository;
