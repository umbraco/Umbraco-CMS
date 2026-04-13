import type { ManifestBlockAction, MetaBlockAction } from '../block-action.extension.js';

export interface ManifestBlockActionDefaultKind extends ManifestBlockAction<MetaBlockActionDefaultKind> {
	type: 'blockAction';
	kind: 'default';
}

export interface MetaBlockActionDefaultKind extends MetaBlockAction {
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDefaultBlockActionKind: ManifestBlockActionDefaultKind;
	}
}
