import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/repository/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbCreateFolderEntityAction, type MetaEntityActionFolderKind } from '@umbraco-cms/backoffice/tree';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbDocumentTypeFolderCreateOptionAction extends UmbCreateFolderEntityAction {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<MetaEntityActionFolderKind>) {
		super(host, { ...args, meta: { ...args.meta, folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } });
	}
}

export { UmbDocumentTypeFolderCreateOptionAction as api };
