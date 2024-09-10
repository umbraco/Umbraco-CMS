import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SYSINFO_MODAL_ALIAS = 'Umb.Modal.Sysinfo';

const modalManifest: ManifestModal = {
	type: 'modal',
	alias: UMB_SYSINFO_MODAL_ALIAS,
	name: 'Sysinfo Modal',
	js: () => import('./components/sysinfo.element.js'),
};

export const manifests: Array<ManifestTypes> = [modalManifest];
