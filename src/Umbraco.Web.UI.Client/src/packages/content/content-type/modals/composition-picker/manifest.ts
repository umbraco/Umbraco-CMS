import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.CompositionPicker',
	name: 'ContentType Composition Picker Modal',
	element: () => import('./composition-picker-modal.element.js'),
};
