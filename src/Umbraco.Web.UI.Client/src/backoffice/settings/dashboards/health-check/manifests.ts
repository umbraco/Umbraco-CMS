import type { ManifestHealthCheck } from '@umbraco-cms/models';

const healthchecks: Array<ManifestHealthCheck> = [
	{
		type: 'healthCheck',
		alias: 'Umb.HealthCheck.Security',
		name: 'Security Health Check',
		loader: () => import('./groups/security-health-check.element'),
		weight: 500,
		meta: {
			label: 'SecurityTest',
		},
	},
	{
		type: 'healthCheck',
		alias: 'Umb.HealthCheck.Configuration',
		name: 'Configuration Health Check',
		loader: () => import('./groups/security-health-check.element'),
		weight: 500,
		meta: {
			label: 'Configuration Test',
		},
	},
];

export const manifests = [...healthchecks];
