import type { UmbMockRelationModel } from '../../types/mock-data-set.types.js';

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
