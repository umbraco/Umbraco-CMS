import { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS = 'Umb.Modal.DynamicRoot.OriginPicker';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
		name: 'Choose an origin',
		js: () => import('./dynamic-root-origin-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
