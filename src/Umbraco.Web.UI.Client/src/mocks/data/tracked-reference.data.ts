import type { RelationItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

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
