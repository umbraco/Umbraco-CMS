import { manifests as confirmManifests } from './confirm/manifests.js';
import { manifests as itemPickerManifests } from './item-picker/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.EmbeddedMedia',
		name: 'Embedded Media Modal',
		element: () => import('./embedded-media/embedded-media-modal.element.js'),
	},
	...confirmManifests,
	...itemPickerManifests,
];
