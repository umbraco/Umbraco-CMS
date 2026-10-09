import type { ManifestTreeAction } from '../tree-action.extension.js';

export interface ManifestTreeActionCreateKind extends ManifestTreeAction {
	type: 'treeAction';
	kind: 'create';
}

export interface UmbTreeCreateOption {
	alias: string;
	label: string;
	icon?: string;
	href?: string;
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTreeActionCreateKind: ManifestTreeActionCreateKind;
	}
}
