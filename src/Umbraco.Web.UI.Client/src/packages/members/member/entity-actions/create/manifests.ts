import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Member.Create',
		name: 'Create Member Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_MEMBER_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Member.CreateOptions',
		name: 'Member Create Options Modal',
		element: () => import('./member-create-options-modal.element.js'),
	},
];
