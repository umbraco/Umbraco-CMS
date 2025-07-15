import type {
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param name
 */
export function getGroupByName(name: string) {
	return healthGroupsWithoutResult.find((group) => group.name?.toLowerCase() == name.toLowerCase());
}

/**
 *
 * @param name
 */
export function getGroupWithResultsByName(name: string) {
	return healthGroups.find((group) => group.name.toLowerCase() === name.toLowerCase());
}

export const healthGroups: Array<HealthCheckGroupWithResultResponseModel & { name: string }> = [
	{
		name: 'Configuration',
		checks: [
			{
				id: '3e2f7b14-4b41-452b-9a30-e67fbc8e1206',
				results: [
					{
						message: `Notification email is still set to the default value of <strong>your@email.here</strong>.`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-notification-email',
					},
				],
			},
		],
	},
	{
		name: 'Data Integrity',
		checks: [
			{
				id: '73dd0c1c-e0ca-4c31-9564-1dca509788af',
				results: [
					{
						message: `All document paths are valid`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{ message: `All media paths are valid`, resultType: StatusResultTypeModel.SUCCESS },
				],
			},
		],
	},
	{
		name: 'Live Environment',
		checks: [
			{
				id: '61214ff3-fc57-4b31-b5cf-1d095c977d6d',
				results: [
					{
						message: `Debug compilation mode is currently enabled. It is recommended to disable this setting before
						go live.`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-compilation-debug',
					},
				],
			},
		],
	},
	{
		name: 'Permissions',
		checks: [
			{
				id: '53dba282-4a79-4b67-b958-b29ec40fcc23',
				results: [
					{
						message: `Folder creation`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: `File writing for packages`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: `File writing`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: `Media folder creation`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
				],
			},
		],
	},
	{
		name: 'Security',
		checks: [
			{
				id: '6708ca45-e96e-40b8-a40a-0607c1ca7f28',
				results: [
					{
						message: `The appSetting 'Umbraco:CMS:WebRouting:UmbracoApplicationUrl' is not set`,
						resultType: StatusResultTypeModel.WARNING,
						readMoreLink: 'https://umbra.co/healthchecks-umbraco-application-url',
					},
				],
			},
			{
				id: 'ed0d7e40-971e-4be8-ab6d-8cc5d0a6a5b0',
				results: [
					{
						message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-click-jacking',
					},
				],
			},
			{
				id: '1cf27db3-efc0-41d7-a1bb-ea912064e071',
				results: [
					{
						message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-no-sniff',
					},
				],
			},
			{
				id: 'e2048c48-21c5-4be1-a80b-8062162df124',
				results: [
					{
						message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-hsts',
					},
				],
			},
			{
				id: 'f4d2b02e-28c5-4999-8463-05759fa15c3a',
				results: [
					{
						message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-xss-protection',
					},
				],
			},
			{
				id: '92abbaa2-0586-4089-8ae2-9a843439d577',
				results: [
					{
						message: `Error pinging the URL https://localhost:44361 - 'The SSL connection could not be established,
						see inner exception.'`,
						resultType: StatusResultTypeModel.WARNING,
						readMoreLink: 'https://umbra.co/healthchecks-excessive-headers',
					},
				],
			},
			{
				id: 'eb66bb3b-1bcd-4314-9531-9da2c1d6d9a7',
				results: [
					{
						message: `You are currently viewing the site using HTTPS scheme`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: `The appSetting 'Umbraco:CMS:Global:UseHttps' is set to 'False' in your appSettings.json file,
						your cookies are not marked as secure.`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-https-config',
					},
					{
						message: `Error pinging the URL https://localhost:44361/ - 'The SSL connection could not be established,
						see inner exception.'"`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-https-request',
					},
				],
			},
		],
	},
	{
		name: 'Services',
		checks: [
			{
				id: '1b5d221b-ce99-4193-97cb-5f3261ec73df',
				results: [
					{
						message: `The 'Umbraco:CMS:Global:Smtp' configuration could not be found.`,
						readMoreLink: 'https://umbra.co/healthchecks-smtp',
						resultType: StatusResultTypeModel.ERROR,
					},
				],
			},
		],
	},
];
export const healthGroupsWithoutResult: HealthCheckGroupPresentationModel[] = [
	{
		name: 'Configuration',
		checks: [
			{
				id: '3e2f7b14-4b41-452b-9a30-e67fbc8e1206',
				name: 'Notification Email Settings',
				description:
					"If notifications are used, the 'from' email address should be specified and changed from the default value.",
			},
		],
	},
	{
		name: 'Data Integrity',
		checks: [
			{
				id: '73dd0c1c-e0ca-4c31-9564-1dca509788af',
				name: 'Database data integrity check',
				description: 'Checks for various data integrity issues in the Umbraco database.',
				//group: 'Data Integrity',
			},
		],
	},
	{
		name: 'Live Environment',
		checks: [
			{
				id: '61214ff3-fc57-4b31-b5cf-1d095c977d6d',
				name: 'Debug Compilation Mode',
				description:
					'Leaving debug compilation mode enabled can severely slow down a website and take up more memory on the server.',
				//group: 'Live Environment',
			},
		],
	},
	{
		name: 'Permissions',
		checks: [
			{
				id: '53dba282-4a79-4b67-b958-b29ec40fcc23',
				name: 'Folder & File Permissions',
				description: 'Checks that the web server folder and file permissions are set correctly for Umbraco to run.',
				//group: 'Permissions',
			},
		],
	},
	{
		name: 'Security',
		checks: [
			{
				id: '6708ca45-e96e-40b8-a40a-0607c1ca7f28',
				name: 'Application URL Configuration',
				description: 'Checks if the Umbraco application URL is configured for your site.',
				//group: 'Security',
			},
			{
				id: 'ed0d7e40-971e-4be8-ab6d-8cc5d0a6a5b0',
				name: 'Click-Jacking Protection',
				description:
					'Checks if your site is allowed to be IFRAMEd by another site and thus would be susceptible to click-jacking.',
				//group: 'Security',
			},
			{
				id: '1cf27db3-efc0-41d7-a1bb-ea912064e071',
				name: 'Content/MIME Sniffing Protection',
				description: 'Checks that your site contains a header used to protect against MIME sniffing vulnerabilities.',
				//group: 'Security',
			},
			{
				id: 'e2048c48-21c5-4be1-a80b-8062162df124',
				name: 'Cookie hijacking and protocol downgrade attacks Protection (Strict-Transport-Security Header (HSTS))',
				description:
					'Checks if your site, when running with HTTPS, contains the Strict-Transport-Security Header (HSTS).',
				//group: 'Security',
			},
			{
				id: 'f4d2b02e-28c5-4999-8463-05759fa15c3a',
				name: 'Cross-site scripting Protection (X-XSS-Protection header)',
				description:
					'This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.',
				//group: 'Security',
			},
			{
				id: '92abbaa2-0586-4089-8ae2-9a843439d577',
				name: 'Excessive Headers',
				description:
					'Checks to see if your site is revealing information in its headers that gives away unnecessary details about the technology used to build and host it.',
				//group: 'Security',
			},
			{
				id: 'eb66bb3b-1bcd-4314-9531-9da2c1d6d9a7',
				name: 'HTTPS Configuration',
				description:
					'Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.',
				//group: 'Security',
			},
		],
	},
	{
		name: 'Services',
		checks: [
			{
				id: '1b5d221b-ce99-4193-97cb-5f3261ec73df',
				name: 'SMTP Settings',
				description: 'Checks that valid settings for sending emails are in place.',
				//group: 'Services',
			},
		],
	},
];
