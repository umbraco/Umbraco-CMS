import { UmbEntityData } from '../entity.data.js';
import type { RelationResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<RelationResponseModel> = [
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '2',
		childName: 'Child 1',
		createDate: '2021-01-01',
		comment: 'Comment 1',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '3',
		childName: 'Child 2',
		createDate: '2021-01-01',
		comment: 'Comment 2',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '4',
		childName: 'Child 3',
		createDate: '2021-01-01',
		comment: 'Comment 3',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '5',
		childName: 'Child 4',
		createDate: '2021-01-01',
		comment: 'Comment 4',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '6',
		childName: 'Child 5',
		createDate: '2021-01-01',
		comment: 'Comment 5',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '7',
		childName: 'Child 6',
		createDate: '2021-01-01',
		comment: 'Comment 6',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
		childId: '8',
		childName: 'Child 7',
		createDate: '2021-01-01',
		comment: 'Comment 7',
	},
	{
		parentId: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		parentName: 'Relate Document On Copy',
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

	getRelationsByParentId(id: string) {
		const test = this.data.filter((relation) => relation.parentId === id);
		return { items: test, total: test.length };
	}
}

export const umbRelationData = new UmbRelationData();
