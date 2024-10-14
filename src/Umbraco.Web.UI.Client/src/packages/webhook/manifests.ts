import { manifests as treeManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
export const manifests: Array<UmbExtensionManifest> = [
	...treeManifests,
	...workspaceManifests,
	...collectionManifests,
	...repositoryManifests,
	...entityActions,
	{
		type: 'modal',
		alias: 'Umb.Modal.Webhook.Events',
		name: 'Webhook Events Modal',
		js: () => import('./components/webhook-events-modal/webhook-events-modal.element.js'),
	},
];
