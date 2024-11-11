import type { MetaEntityActionFolderKind } from '../../types.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionDeleteFolderKind extends ManifestEntityAction<MetaEntityActionFolderKind> {
	type: 'entityAction';
	kind: 'folderDelete';
}

declare global {
	interface UmbExtensionManifestMap {
		umbDeleteFolderEntityActionKind: ManifestEntityActionDeleteFolderKind;
	}
}
