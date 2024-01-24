import { RelationItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const items: Array<RelationItemResponseModel> = [
	{
		nodeId: 'simple-document-id',
		nodeName: 'Simple Document',
		nodeType: 'document',
		nodePublished: true,
		contentTypeIcon: 'icon-document',
		contentTypeName: 'Simple Document Type',
		contentTypeAlias: 'blogPost',
		relationTypeIsBidirectional: false,
		relationTypeIsDependency: true,
		relationTypeName: 'Related Document',
	},
];
