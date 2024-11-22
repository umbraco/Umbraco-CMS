import type { MetaEntityActionDefaultKind } from '../../default/index.js';
import type { ManifestEntityAction } from '../../entity-action.extension.js';

export interface ManifestEntityActionCreateKind extends ManifestEntityAction<MetaEntityActionCreateKind> {
	type: 'entityAction';
	kind: 'create';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityActionCreateKind extends MetaEntityActionDefaultKind {}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionCreateKind: ManifestEntityActionCreateKind;
	}
}
