import type { ManifestEntitySign, MetaEntitySign } from '../../types.js';

export interface ManifestEntitySignIconKind extends ManifestEntitySign<MetaEntitySignIconKind> {
	type: 'entitySign';
	kind: 'icon';
}

export interface MetaEntitySignIconKind extends MetaEntitySign {
	iconName: string;
	label: string;
	iconColorAlias?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntitySignIconKind: ManifestEntitySignIconKind;
	}
}
