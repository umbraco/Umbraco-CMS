import { manifests as userPermissionModalManifests } from './modals/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...userPermissionModalManifests,
	{
		type: 'userPermission',
		alias: 'Umb.UserPermission.Developer',
		name: 'Developer User Permission',
		weight: 1000,
		meta: {
			label: 'Developer User Permission',
			description: 'Enables access to developer hints and tools.',
			verbs: ['Umb.Developer'],
		},
	},
];
