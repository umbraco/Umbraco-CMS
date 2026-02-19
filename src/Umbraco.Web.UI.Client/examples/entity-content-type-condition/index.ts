import { UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/content-type';
import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';

export const manifests = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Example.EntityAction.EntityContentTypeUniqueCondition',
		name: 'Example Entity Action With Entity Content Type Unique Condition',
		api: () => import('./unique-condition-entity-action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-science',
			label: 'Conditional (Content Type Unique)',
		},
		conditions: [
			{
				alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
				match: '6bcb87dc-76b4-4ef7-9139-5750fd10d4ff',
				//oneOf: ['6bcb87dc-76b4-4ef7-9139-5750fd10d4ff'], // Example uniques
			},
		],
	},
];
