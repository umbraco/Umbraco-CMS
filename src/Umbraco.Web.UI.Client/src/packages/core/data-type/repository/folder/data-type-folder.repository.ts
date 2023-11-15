import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from '../../tree/data-type.tree.store.js';
import { folderToDataTypeTreeItemMapper } from '../utils.js';
import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDataTypeFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource, UMB_DATA_TYPE_TREE_STORE_CONTEXT, folderToDataTypeTreeItemMapper);
	}
}
