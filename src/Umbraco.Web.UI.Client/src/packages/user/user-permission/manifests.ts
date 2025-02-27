import { manifests as userPermissionModalManifests } from './modals/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...userPermissionModalManifests,
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.HealthCheck.Browse',
		name: 'Browse Health Check User Permission',
		element: () => import('./hello.js'),
		meta: {
			label: 'Browse',
			description: 'Allow access to browser Health Check reports',
			permission: {
				context: 'Umbraco.HealthCheck',
				verbs: ['browse'],
			},
		},
	},
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.HealthCheck.Execute',
		name: 'Execute Health Check User Permission',
		element: () => import('./hello.js'),
		meta: {
			label: 'Execute',
			description: 'Allow access to execute Health Checks',
			permission: {
				context: 'Umbraco.HealthCheck',
				verbs: ['execute'],
			},
		},
	},
];
