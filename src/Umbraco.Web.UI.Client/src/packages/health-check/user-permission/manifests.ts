import {
	UMB_USER_PERMISSION_CONTEXT_HEALTH_CHECK,
	UMB_USER_PERMISSION_HEALTH_CHECK_BROWSE,
	UMB_USER_PERMISSION_HEALTH_CHECK_EXECUTE,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'contextualUserPermission',
		alias: 'Umb.UserPermission.HealthCheck.Browse',
		name: 'Browse Health Check User Permission',
		meta: {
			label: 'Browse',
			description: 'Allow access to browser Health Check reports',
			group: 'Health Check',
			permission: {
				context: UMB_USER_PERMISSION_CONTEXT_HEALTH_CHECK,
				verbs: [UMB_USER_PERMISSION_HEALTH_CHECK_BROWSE],
			},
		},
	},
	{
		type: 'contextualUserPermission',
		alias: 'Umb.UserPermission.HealthCheck.Execute',
		name: 'Execute Health Check User Permission',
		meta: {
			label: 'Execute',
			description: 'Allow access to execute Health Checks',
			group: 'Health Check',
			permission: {
				context: UMB_USER_PERMISSION_CONTEXT_HEALTH_CHECK,
				verbs: [UMB_USER_PERMISSION_HEALTH_CHECK_EXECUTE],
			},
		},
	},
];
