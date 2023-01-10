import type { ManifestElement } from './models';

export interface ManifestHealthCheck extends ManifestElement {
	type: 'healthCheck';
	meta: MetaHealthCheck;
}

export interface MetaHealthCheck {
	label: string;
	checks: HealthCheck[];
}

export interface HealthCheck {
	alias: string;
	name: string;
	description: string;
}
