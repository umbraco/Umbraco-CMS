import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_MEDIA_SECTION_PATH } from '../../../media-section/paths.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbTrashableEntityWorkspaceContextBase } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbTrashableMediaWorkspaceContext extends UmbTrashableEntityWorkspaceContextBase {
	protected override getRedirectPath({ entity }: { entity: UmbEntityModel }): string {
		if (!entity.unique) {
			return UMB_MEDIA_SECTION_PATH;
		}
		return UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}
}
