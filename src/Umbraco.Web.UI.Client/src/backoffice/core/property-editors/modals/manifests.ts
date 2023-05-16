import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertyEditorUIPicker',
		name: 'Property Editor UI Picker Modal',
		loader: () => import('./property-editor-ui-picker/property-editor-ui-picker-modal.element'),
	},
];

export const manifests = [...modals];
