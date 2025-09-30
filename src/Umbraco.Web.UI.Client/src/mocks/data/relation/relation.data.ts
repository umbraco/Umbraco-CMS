import type { RelationResponseModel, RelationReferenceModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockRelationModel = RelationResponseModel;
export type UmbMockRelationReferenceModel = RelationReferenceModel;

export const data: Array<UmbMockRelationModel> = [
	{
		id: 'relation1',
		child: {
			id: 'child1',
			name: 'Child 1',
		},
		createDate: '2021-09-01T00:00:00',
		parent: {
			id: 'parent1',
			name: 'Parent 1',
		},
		relationType: {
			id: 'relationType1',
		},
		comment: 'Comment 1',
	},
];
