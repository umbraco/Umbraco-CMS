import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionTrashKind extends ManifestEntityAction<MetaEntityActionTrashKind> {
	type: 'entityAction';
	kind: 'trash';
}

export interface MetaEntityActionTrashKind extends MetaEntityActionDefaultKind {
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTrashEntityActionKind: ManifestEntityActionTrashKind;
	}
}
