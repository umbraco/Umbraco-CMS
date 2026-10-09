import type { ManifestBlockAction, MetaBlockAction } from '../block-action.extension.js';

/** Manifest type for block actions using the default kind. */
export interface ManifestBlockActionDefaultKind extends ManifestBlockAction<MetaBlockActionDefaultKind> {
	type: 'blockAction';
	kind: 'default';
}

/** Metadata for the default block action kind. Provides icon and label for the button. */
export interface MetaBlockActionDefaultKind extends MetaBlockAction {
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDefaultBlockActionKind: ManifestBlockActionDefaultKind;
	}
}
