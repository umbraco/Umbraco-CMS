import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.PropertyEditorUIPicker',
		name: 'Property Editor UI Picker Modal',
		loader: () => import('./property-editor-ui-picker/property-editor-ui-picker-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypePickerFlow',
		name: 'Data Type Picker Flow Modal',
		loader: () => import('./data-type-picker-flow/data-type-picker-flow-modal.element'),
	},
];

export const manifests = [...modals];
