import type { MetaEntityActionFolderKind } from '../../types.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionCreateFolderKind extends ManifestEntityAction<MetaEntityActionFolderKind> {
	type: 'entityAction';
	kind: 'folderCreate';
}

declare global {
	interface UmbExtensionManifestMap {
		umbCreateFolderEntityActionKind: ManifestEntityActionCreateFolderKind;
	}
}
