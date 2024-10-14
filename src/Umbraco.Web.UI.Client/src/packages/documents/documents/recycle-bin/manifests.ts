import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Allow Document Recycle Bin Current User Condition',
		alias: 'Umb.Condition.CurrentUser.AllowDocumentRecycleBin',
		api: () => import('./allow-document-recycle-bin.condition.js'),
	},
	...entityActionManifests,
	...menuItemManifests,
	...repositoryManifests,
	...treeManifests,
];
