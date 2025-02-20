import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDynamicRootOrigin extends ManifestBase {
	type: 'dynamicRootOrigin';
	meta: MetaDynamicRootOrigin;
}

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
