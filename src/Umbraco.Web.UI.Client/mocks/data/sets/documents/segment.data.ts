import type { UmbMockSegmentModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockSegmentModel> = [
	{
		alias: 's1',
		name: 'Segment 1',
		cultures: null,
	},
	{
		alias: 's2',
		name: 'Segment 2',
	},
	{
		alias: 's-da',
		name: 'Danish Segment',
		cultures: ['da'],
	},
];
