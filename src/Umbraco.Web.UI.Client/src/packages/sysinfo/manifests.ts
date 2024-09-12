import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modalManifest: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.Sysinfo',
	name: 'Sysinfo Modal',
	js: () => import('./components/sysinfo.element.js'),
};

export const manifests: Array<ManifestTypes> = [modalManifest];
