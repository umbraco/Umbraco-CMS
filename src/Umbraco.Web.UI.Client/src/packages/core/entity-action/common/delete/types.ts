import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '../../types.js';

export interface ManifestEntityActionDeleteKind extends ManifestEntityAction<MetaEntityActionDeleteKind> {
	type: 'entityAction';
	kind: 'delete';
}

export interface MetaEntityActionDeleteKind extends MetaEntityActionDefaultKind {
	detailRepositoryAlias: string;
	itemRepositoryAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDeleteEntityActionKind: ManifestEntityActionDeleteKind;
	}
}
