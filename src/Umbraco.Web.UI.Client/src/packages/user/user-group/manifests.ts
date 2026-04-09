import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as sectionViewManifests } from './workspace/user-group-root/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...menuItemManifests,
	...modalManifests,
	...repositoryManifests,
	...sectionViewManifests,
	...workspaceManifests,
	{
		name: 'User Group Backoffice Entry Point',
		alias: 'Umb.EntryPoint.UserGroup',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
