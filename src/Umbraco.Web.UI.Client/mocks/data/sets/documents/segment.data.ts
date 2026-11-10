import type { SegmentResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<SegmentResponseModel> = [
	{ alias: 's1', name: 'Segment 1' },
	{ alias: 's2', name: 'Segment 2' },
	{ alias: 'da-only', name: 'Danish Only Segment', cultures: ['da'] },
];
