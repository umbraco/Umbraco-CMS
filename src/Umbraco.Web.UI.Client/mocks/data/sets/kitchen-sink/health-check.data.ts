import type {
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

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
						message: `Debug compilation mode is currently enabled. It is recommended to disable this setting before go live.`,
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
			},
		],
	},
];
