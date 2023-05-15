import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestHealthCheck extends ManifestElement {
	type: 'healthCheck';
	meta: MetaHealthCheck;
}

export interface MetaHealthCheck {
	label: string;
	api: any;
}

export interface HealthCheck {
	alias: string;
	name: string;
	description: string;
}
