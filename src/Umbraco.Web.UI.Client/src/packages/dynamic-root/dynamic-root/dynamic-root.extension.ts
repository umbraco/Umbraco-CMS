import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

/**
 * An origin a dynamic root query can start from. `meta.originAlias` is sent to the server, which resolves it to the origin node.
 */
export interface ManifestDynamicRootOrigin extends ManifestBase {
	type: 'dynamicRootOrigin';
	meta: MetaDynamicRootOrigin;
}

/**
 * A step that moves from the nodes a dynamic root query has resolved so far. `meta.queryStepAlias` is sent to the server, which runs the matching step.
 */
export interface ManifestDynamicRootQueryStep extends ManifestBase {
	type: 'dynamicRootQueryStep';
	meta: MetaDynamicRootQueryStep;
}

export interface MetaDynamicRootOrigin {
	originAlias: string;
	label?: string;
	description?: string;
	icon?: string;
}

export interface MetaDynamicRootQueryStep {
	queryStepAlias: string;
	label?: string;
	description?: string;
	icon?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDynamicRootOrigin: ManifestDynamicRootOrigin;
		umbDynamicRootQueryStep: ManifestDynamicRootQueryStep;
	}
}
