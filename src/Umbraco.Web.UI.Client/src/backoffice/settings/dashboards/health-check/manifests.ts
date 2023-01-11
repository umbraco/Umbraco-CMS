import { HealthCheckGroup, HealthCheckResource } from '@umbraco-cms/backend-api';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { UmbHealthCheckContext } from './health-check.context';

const _getAllGroups = async () => {
	const response = await HealthCheckResource.getHealthCheckGroup({ skip: 0, take: 9999 });
	return response.items;
};
const groups: HealthCheckGroup[] = await _getAllGroups();

const _createManifests = (groups: HealthCheckGroup[]): Array<ManifestHealthCheck> => {
	return groups.map((group) => {
		return {
			type: 'healthCheck',
			alias: `Umb.HealthCheck.${group.name}`,
			name: `${group.name} Health Check`,
			weight: 500,
			meta: {
				label: group.name || '',
				api: UmbHealthCheckContext,
			},
		};
	});
};

const healthChecks = _createManifests(groups);

/*
const healthchecks: Array<ManifestHealthCheck> = [
	{
		type: 'healthCheck',
		alias: 'Umb.HealthCheck.Security',
		name: 'Security Health Check',
		weight: 500,
		meta: {
			label: 'SecurityTest',
			checks: [
				{
					alias: 'applicationUrlConfiguration',
					name: 'Application URL Configuration',
					description: 'Checks if the Umbraco application URL is configured for your site.',
				},
				{
					alias: 'clickJackingProtection',
					name: 'Click-Jacking Protection',
					description:
						'Checks if your site is allowed to be IFRAMEd by another site and thus would be susceptible to click-jacking.',
				},
				{
					alias: 'contentSniffingProtection',
					name: 'Content/MIME Sniffing Protection',
					description: 'Checks that your site contains a header used to protect against MIME sniffing vulnerabilities.',
				},
				{
					alias: 'cookieHijackingProtection',
					name: 'Cookie hijacking and protocol downgrade attacks Protection (Strict-Transport-Security Header (HSTS))',
					description:
						'Checks if your site, when running with HTTPS, contains the Strict-Transport-Security Header (HSTS).',
				},
				{
					alias: 'crossSiteProtection',
					name: 'Cross-site scripting Protection (X-XSS-Protection header)',
					description:
						'This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.',
				},
				{
					alias: 'excessiveHeaders',
					name: 'Excessive Headers',
					description:
						'Checks to see if your site is revealing information in its headers that gives away unnecessary details about the technology used to build and host it.',
				},
				{
					alias: 'HttpsConfiguration',
					name: 'HTTPS Configuration',
					description:
						'Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.',
				},
			],
		},
	},
	{
		type: 'healthCheck',
		alias: 'Umb.HealthCheck.Configuration',
		name: 'Configuration Health Check',

		weight: 500,
		meta: {
			label: 'Configuration Test',
			checks: [
				{
					alias: 'macroErrors',
					name: 'Macro errors',
					description:
						'Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.',
				},
				{
					alias: 'notificationEmailSettings',
					name: 'Notification Email Settings',
					description:
						"If notifications are used, the 'from' email address should be specified and changed from the default value.",
				},
			],
		},
	},
];*/

export const manifests = [...healthChecks];
