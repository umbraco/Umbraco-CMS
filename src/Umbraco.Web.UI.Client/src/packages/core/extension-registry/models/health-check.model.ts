import type { UmbHealthCheckContext } from '../../../health-check/health-check.context.js';
import type { ApiLoaderProperty, ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestHealthCheck extends ManifestElementAndApi {
	type: 'healthCheck';
	meta: MetaHealthCheck;
	api: ApiLoaderProperty<UmbHealthCheckContext>;
}

export interface MetaHealthCheck {
	label: string;
}

export interface HealthCheck {
	alias: string;
	name: string;
	description: string;
}
