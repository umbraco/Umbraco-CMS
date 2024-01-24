import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CultureAndHostnames',
		name: 'Culture And Hostnames Modal',
		js: () => import('./modal/culture-and-hostnames-modal.element.js'),
	},
];

export const manifests = [...modals, ...repositoryManifests];
