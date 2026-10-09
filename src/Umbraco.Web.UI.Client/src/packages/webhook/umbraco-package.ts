import { manifests as webhookDeliveryManifests } from './webhook-delivery/manifests.js';
import { manifests as webhookEventManifests } from './webhook-event/manifests.js';
import { manifests as webhookManifests } from './webhook/manifests.js';
import { manifests as webhookRootManifests } from './webhook-root/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...webhookDeliveryManifests,
	...webhookEventManifests,
	...webhookManifests,
	...webhookRootManifests,
	{
		name: 'Webhook Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Webhook',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];

export const name = 'Umbraco.Core.Webhook';
export const extensions = [
	{
		name: 'Webhook Bundle',
		alias: 'Umb.Bundle.Webhook',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
