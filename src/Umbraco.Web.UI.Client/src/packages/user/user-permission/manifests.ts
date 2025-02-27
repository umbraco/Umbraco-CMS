import { manifests as userPermissionModalManifests } from './modals/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...userPermissionModalManifests,
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.HealthCheck.Browse',
		name: 'Browse Health Check User Permission',
		element: () => import('./hello.js'),
		meta: {
			label: 'Browse Health Check User Permission',
			description: 'Something Something Health Check',
			permission: {
				context: 'Umbraco.HealthCheck',
				permission: 'browse',
			},
		},
	},
];
