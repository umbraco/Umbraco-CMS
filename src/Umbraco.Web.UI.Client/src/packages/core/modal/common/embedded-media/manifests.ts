import { manifests as repositories } from './repository/manifests.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.EmbeddedMedia',
		name: 'Embedded Media Modal',
		element: () => import('./embedded-media-modal.element.js'),
	},
];

export const manifests = [...modals, ...repositories];
