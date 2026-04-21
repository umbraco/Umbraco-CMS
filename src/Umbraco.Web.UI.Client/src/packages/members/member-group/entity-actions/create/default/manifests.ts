import { UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.MemberGroup.Default',
		name: 'Default Member Group Entity Create Option Action',
		weight: 1000,
		api: () => import('./default-member-group-create-option-action.js'),
		forEntityTypes: [UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-users',
			label: '#actions_create',
			additionalOptions: true,
		},
	},
];
