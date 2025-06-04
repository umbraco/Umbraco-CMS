import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

const demoModal: ManifestModal = {
	type: 'modal',
	name: 'Example Custom Modal Element',
	alias: 'example.modal.custom.element',
	js: () => import('./example-modal-view.element.js'),
};

const demoModalsDashboard: ManifestDashboard = {
	type: 'dashboard',
	name: 'Example Custom Modal Dashboard',
	alias: 'example.dashboard.custom.modal.element',
	element: () => import('./example-custom-modal-dashboard.element.js'),
	weight: 900,
	meta: {
		label: 'Custom Modal',
		pathname: 'custom-modal',
	},
	conditions: [
		{
			alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
			match: 'Umb.Section.Content',
		},
	],
};

export default [demoModal, demoModalsDashboard];
