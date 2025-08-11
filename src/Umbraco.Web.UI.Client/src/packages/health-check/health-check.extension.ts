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

/**
 * @deprecated Use `UmbHealthCheck` instead. This will be removed in Umbraco 18.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type, @typescript-eslint/naming-convention
export interface HealthCheck extends UmbHealthCheck {
	// Left empty
}

declare global {
	interface UmbExtensionManifestMap {
		umbHealthCheck: ManifestHealthCheck;
	}
}
