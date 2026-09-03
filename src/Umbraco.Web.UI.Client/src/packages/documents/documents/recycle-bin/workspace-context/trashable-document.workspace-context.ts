import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_DOCUMENTS_SECTION_PATH } from '../../../section/paths.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbTrashableEntityWorkspaceContextBase } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbTrashableDocumentWorkspaceContext extends UmbTrashableEntityWorkspaceContextBase {
	protected override getRedirectPath({ entity }: { entity: UmbEntityModel }): string {
		if (!entity.unique) {
			return UMB_DOCUMENTS_SECTION_PATH;
		}
		return UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}
}

export { UmbTrashableDocumentWorkspaceContext as api };
