import type { RelationResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<RelationResponseModel> = [
	{
		parentId: '1',
		parentName: 'TEST Parent 1',
		childId: '2',
		childName: 'Child 1',
		createDate: '2021-01-01',
		comment: 'Comment 1',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '3',
		childName: 'Child 2',
		createDate: '2021-01-01',
		comment: 'Comment 2',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '4',
		childName: 'Child 3',
		createDate: '2021-01-01',
		comment: 'Comment 3',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '5',
		childName: 'Child 4',
		createDate: '2021-01-01',
		comment: 'Comment 4',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '6',
		childName: 'Child 5',
		createDate: '2021-01-01',
		comment: 'Comment 5',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '7',
		childName: 'Child 6',
		createDate: '2021-01-01',
		comment: 'Comment 6',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '8',
		childName: 'Child 7',
		createDate: '2021-01-01',
		comment: 'Comment 7',
	},
	{
		parentId: '1',
		parentName: 'Parent 1',
		childId: '9',
		childName: 'Child 8',
		createDate: '2021-01-01',
		comment: 'Comment 8',
	},
];

// Temp mocked database
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbRelationData extends UmbEntityData<RelationResponseModel> {
	constructor() {
		super(data);
	}
}

export const umbRelationData = new UmbRelationData();
