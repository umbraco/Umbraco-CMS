import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ACTION_GROUP_CREATE } from '@umbraco-cms/backoffice/action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Member.Create',
		group: UMB_ACTION_GROUP_CREATE,
		name: 'Create Member Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_MEMBER_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_createFor',
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
