import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/modal';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.ChooseType.Sidebar';
export const UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.Section.Sidebar';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
		name: 'Choose insert type sidebar',
		js: () => import('./insert-choose-type-sidebar.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS,
		name: 'Partial View Picker Modal',
		js: () => import('../../templating/modals/partial-view-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS,
		name: 'Partial Insert Section Picker Modal',
		js: () => import('./insert-section-modal/insert-section-modal.element.js'),
	},
];

export const manifests = [...modals];
