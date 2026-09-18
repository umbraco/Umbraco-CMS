import type { UmbMediaCollectionOrderByOption } from './types.js';

export const UMB_MEDIA_COLLECTION_ORDER_BY_OPTIONS: Array<UmbMediaCollectionOrderByOption> = [
	{
		unique: 'updateDateDescending',
		label: '#media_sortLastUploadedFirst',
		config: {
			orderBy: 'updateDate',
			orderDirection: 'desc',
		},
	},
	{
		unique: 'updateDateAscending',
		label: '#media_sortLastEditedOldest',
		config: {
			orderBy: 'updateDate',
			orderDirection: 'asc',
		},
	},
	{
		unique: 'nameAscending',
		label: '#user_sortNameAscending',
		config: {
			orderBy: 'name',
			orderDirection: 'asc',
		},
	},
	{
		unique: 'nameDescending',
		label: '#user_sortNameDescending',
		config: {
			orderBy: 'name',
			orderDirection: 'desc',
		},
	},
];
