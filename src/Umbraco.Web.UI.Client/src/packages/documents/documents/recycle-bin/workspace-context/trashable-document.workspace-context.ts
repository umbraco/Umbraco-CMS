import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_DOCUMENTS_SECTION_PATH } from '../../../section/paths.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbTrashableEntityWorkspaceContextBase } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbTrashableDocumentWorkspaceContext extends UmbTrashableEntityWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host);
		this._setRecycleBinRepositoryAlias(UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS);
	}

	protected override getRedirectPath({ entity }: { entity: UmbEntityModel }): string {
		if (!entity.unique) {
			return UMB_DOCUMENTS_SECTION_PATH;
		}
		return UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}
}
