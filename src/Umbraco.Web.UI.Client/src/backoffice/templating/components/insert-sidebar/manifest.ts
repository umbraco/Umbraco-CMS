import { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.ChooseType.Sidebar';
export const UMB_MODAL_TEMPLATING_INSERT_VALUE_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.Value.Sidebar';
export const UMB_MODAL_TEMPLATING_INSERT_PARTIAL_VIEW_SIDEBAR_ALIAS = 'Umb.Modal.Templating.Insert.PartialView.Sidebar';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
		name: 'Choose insert type sidebar',
		loader: () => import('./insert-choose-type-sidebar.element'),
	},
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_VALUE_SIDEBAR_ALIAS,
		name: 'Insert value type sidebar',
		loader: () => import('./insert-value-sidebar.element'),
	},
	{
		type: 'modal',
		alias: UMB_MODAL_TEMPLATING_INSERT_PARTIAL_VIEW_SIDEBAR_ALIAS,
		name: 'Insert value type sidebar',
		loader: () => import('./insert-partial-view-sidebar.element'),
	},
];

export const manifests = [...modals];
