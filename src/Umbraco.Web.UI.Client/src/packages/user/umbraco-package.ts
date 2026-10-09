import { manifests as userGroupManifests } from './user-group/manifests.js';
import { manifests as userManifests } from './user/manifests.js';
import { manifests as userSectionManifests } from './section/manifests.js';
import { manifests as currentUserManifests } from './current-user/manifests.js';
import { manifests as userPermissionManifests } from './user-permission/manifests.js';
import { manifests as changePasswordManifests } from './change-password/manifests.js';

// We need to load any components that are not loaded by the user management bundle to register them in the browser.
import './user-group/components/index.js';
import './user-permission/components/index.js';
import './user/components/index.js';

export const manifests = [
	...userGroupManifests,
	...userManifests,
	...userSectionManifests,
	...currentUserManifests,
	...userPermissionManifests,
	...changePasswordManifests,
];

export const name = 'Umbraco.Core.UserManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'User Management Bundle',
		alias: 'Umb.Bundle.UserManagement',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
