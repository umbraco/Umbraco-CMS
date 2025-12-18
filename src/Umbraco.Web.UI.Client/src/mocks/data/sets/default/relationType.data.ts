import type { UmbMockRelationTypeModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockRelationTypeModel> = [
	{
		name: 'Relation Type 1 (bidirectional, deletable)',
		id: 'relationType1',
		alias: 'relationType1',
		isBidirectional: true,
		isDependency: false,
		isDeletable: true,
		childObject: {
			id: 'child1',
			name: 'Child Object 1',
		},
		parentObject: {
			id: 'parent1',
			name: 'Parent Object 1',
		},
		flags: [],
	},
	{
		name: 'Relation Type 2 (unidirectional, not deletable)',
		id: 'relationType2',
		alias: 'relationType2',
		isBidirectional: false,
		isDependency: false,
		isDeletable: false,
		flags: [],
	},
	{
		name: 'Relation Type 3 (bidirectional, deletable)',
		id: 'relationType3',
		alias: 'relationType3',
		isBidirectional: true,
		isDependency: false,
		isDeletable: true,
		flags: [],
	},
];
