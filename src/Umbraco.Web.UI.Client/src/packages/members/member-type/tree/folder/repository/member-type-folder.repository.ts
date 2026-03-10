import { UmbMemberTypeFolderServerDataSource } from './member-type-folder.server.data-source.js';
import { UMB_MEMBER_TYPE_FOLDER_STORE_CONTEXT } from './member-type-folder.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbMemberTypeFolderRepository extends UmbDetailRepositoryBase<UmbFolderModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeFolderServerDataSource, UMB_MEMBER_TYPE_FOLDER_STORE_CONTEXT);
	}
}

export default UmbMemberTypeFolderRepository;
