import { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_DICTIONARY_ITEM_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/modal';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DICTIONARY_ITEM_PICKER_MODAL_ALIAS,
		name: 'Dictionary Item Picker Modal',
		loader: () => import('./dictionary-item-picker/dictionary-item-picker-modal.element'),
	},
];

export const manifests = [...modals];
