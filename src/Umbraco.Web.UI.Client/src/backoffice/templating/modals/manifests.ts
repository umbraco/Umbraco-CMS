import { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/modal';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.ChooseType.Sidebar';
export const UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.Value.Sidebar';
export const UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.Section.Sidebar';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
		name: 'Choose insert type sidebar',
		loader: () => import('./insert-choose-type-sidebar.element'),
	},
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS,
		name: 'Insert value type sidebar',
		loader: () => import('./insert-value-sidebar.element'),
	},
	{
		type: 'modal',
		alias: UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS,
		name: 'Partial View Picker Modal',
		loader: () => import('../../templating/modals/partial-view-picker-modal.element'),
	},
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS,
		name: 'Partial Insert Section Picker Modal',
		loader: () => import('./insert-section-modal/insert-section-modal.element'),
	},
];

export const manifests = [...modals];
