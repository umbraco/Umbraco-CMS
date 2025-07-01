import type { MetaEntityActionFolderKind } from '../../types.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionUpdateFolderKind extends ManifestEntityAction<MetaEntityActionFolderKind> {
	kind: 'folderUpdate';
}

declare global {
	interface UmbExtensionManifestMap {
		umbUpdateFolderEntityActionKind: ManifestEntityActionUpdateFolderKind;
	}
}
