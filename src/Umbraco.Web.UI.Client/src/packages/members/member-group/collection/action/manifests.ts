import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'button',
		name: 'Create Member Group Collection Action',
		alias: 'Umb.CollectionAction.MemberGroup.Create',
		weight: 200,
		meta: {
			label: '#general_create',
			href: 'section/member-management/workspace/member-group/create',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: 'Umb.Collection.MemberGroup',
			},
		],
	},
];
