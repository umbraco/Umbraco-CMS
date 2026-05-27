import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionTrashFolderKind extends ManifestEntityAction<MetaEntityActionTrashFolderKind> {
	type: 'entityAction';
	kind: 'trashFolder';
}

export interface MetaEntityActionTrashFolderKind extends MetaEntityActionDefaultKind {
	folderRepositoryAlias: string;
	recycleBinRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTrashFolderEntityActionKind: ManifestEntityActionTrashFolderKind;
	}
}
