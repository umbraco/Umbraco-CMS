import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/repository/constants.js';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDocumentTypeFolderCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async execute() {
		if (!this.args.entityType) throw new Error('Entity type is required to create a folder');

		const createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.args.unique ?? null,
			entityType: this.args.entityType,
			meta: { icon: '', label: '', folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS },
		});

		await createFolderAction.execute().catch(() => undefined);
	}
}

export { UmbDocumentTypeFolderCreateOptionAction as api };
