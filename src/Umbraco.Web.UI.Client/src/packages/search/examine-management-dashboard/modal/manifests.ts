import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Examine.FieldsSettings',
		name: 'Examine Field Settings Modal',
		js: () => import('./fields-settings/examine-fields-settings-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Examine.FieldsViewer',
		name: 'Examine Field Viewer Modal',
		js: () => import('./fields-viewer/examine-fields-viewer-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modals];
