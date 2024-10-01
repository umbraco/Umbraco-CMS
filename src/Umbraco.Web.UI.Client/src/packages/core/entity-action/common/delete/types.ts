import type { MetaEntityActionDefaultKind } from '../../default/index.js';
import type { ManifestEntityAction } from '../../entity-action.extension.js';

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
