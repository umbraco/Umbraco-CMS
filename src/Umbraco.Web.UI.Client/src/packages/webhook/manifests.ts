import { manifests as deliveryManifests } from './delivery/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as webhookManifests } from './webhook/manifests.js';
import { manifests as webhookRootManifests } from './webhook-root/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...deliveryManifests,
	...entityActions,
	...repositoryManifests,
	...webhookManifests,
	...webhookRootManifests,
	...workspaceManifests,
	{
		type: 'modal',
		alias: 'Umb.Modal.Webhook.Events',
		name: 'Webhook Events Modal',
		js: () => import('./components/webhook-events-modal/webhook-events-modal.element.js'),
	},
];
