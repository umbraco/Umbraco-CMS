import type { ApiLoaderProperty, ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestHealthCheck extends ManifestBase {
	type: 'healthCheck';
	meta: MetaHealthCheck;
	/**
	 * The API to load for this health check. This should implement or extend the `UmbHealthCheckContext` interface.
	 */
	api: ApiLoaderProperty;
}

export interface MetaHealthCheck {
	label: string;
}

/**
 * Represents a health check.
 */
export interface UmbHealthCheck {
	alias: string;
	name: string;
	description: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbHealthCheck: ManifestHealthCheck;
	}
}
