import type { UmbMockRelationModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockRelationModel> = [
	{
		id: 'relation1',
		child: { id: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1' },
		createDate: '2021-09-01T00:00:00',
		parent: { id: 'simple-document-id' },
		relationType: {
			id: 'relationType1',
		},
		comment: 'Comment 1',
	},
];
