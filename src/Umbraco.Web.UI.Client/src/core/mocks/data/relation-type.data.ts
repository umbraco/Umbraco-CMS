import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import type { EntityTreeItemResponseModel, RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: investigate why we don't get an entity type as part of the RelationTypeResponseModel
export const data: Array<RelationTypeResponseModel> = [
	{
		key: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		alias: 'relateDocumentOnCopy',
		name: 'Relate Document On Copy',
		path: '',
		isSystemRelationType: true,
		isBidirectional: false,
		isDependency: false,
		parentObjectType: 'Document',
		childObjectType: 'Document',
		parentObjectTypeName: 'Document',
		childObjectTypeName: 'Document',
	},
	{
		key: 'ac68cde6-763f-4231-a751-1101b57defd2',
		alias: 'relateParentDocumentOnDelete',
		name: 'Relate Parent Document On Delete',
		path: '',
		isSystemRelationType: true,
		isBidirectional: false,
		isDependency: false,
		parentObjectType: 'Document',
		childObjectType: 'Document',
		parentObjectTypeName: 'Document',
		childObjectTypeName: 'Document',
	},
	{
		key: '6f9b800c-762c-42d4-85d9-bf40a77d689e',
		alias: 'relateParentMediaFolderOnDelete',
		name: 'Relate Parent Media Folder On Delete',
		path: '',
		isSystemRelationType: true,
		isBidirectional: false,
		isDependency: false,
		parentObjectType: 'Document',
		childObjectType: 'Document',
		parentObjectTypeName: 'Document',
		childObjectTypeName: 'Document',
	},
	{
		key: 'd421727d-43de-4205-b4c6-037404f309ad',
		alias: 'relatedMedia',
		name: 'Related Media',
		path: '',
		isSystemRelationType: true,
		isBidirectional: false,
		isDependency: false,
		parentObjectType: 'Document',
		childObjectType: 'Document',
		parentObjectTypeName: 'Document',
		childObjectTypeName: 'Document',
	},
	{
		key: 'e9a0a28e-2d5b-4229-ac00-66f2df230513',
		alias: 'relatedDocument',
		name: 'Related Document',
		path: '',
		isSystemRelationType: true,
		isBidirectional: false,
		isDependency: false,
		parentObjectType: 'Document',
		childObjectType: 'Document',
		parentObjectTypeName: 'Document',
		childObjectTypeName: 'Document',
	},
];

export const treeData: Array<EntityTreeItemResponseModel> = [
	{
		key: 'e0d39ff5-71d8-453f-b682-9d8d31ee5e06',
		$type: 'EntityTreeItemViewModel',
		isContainer: false,
		parentKey: null,
		name: 'Relate Document On Copy',
		icon: 'umb:trafic',
		type: 'relation-type',
	},
	{
		key: 'ac68cde6-763f-4231-a751-1101b57defd2',
		$type: 'EntityTreeItemViewModel',
		isContainer: false,
		parentKey: null,
		name: 'Relate Parent Document On Delete',
		icon: 'umb:trafic',
		type: 'relation-type',
	},
	{
		key: '6f9b800c-762c-42d4-85d9-bf40a77d689e',
		$type: 'EntityTreeItemViewModel',
		isContainer: false,
		parentKey: null,
		name: 'Relate Parent Media Folder On Delete',
		icon: 'umb:trafic',
		type: 'relation-type',
	},
	{
		key: 'd421727d-43de-4205-b4c6-037404f309ad',
		$type: 'EntityTreeItemViewModel',
		isContainer: false,
		parentKey: null,
		name: 'Related Media',
		icon: 'umb:trafic',
		type: 'relation-type',
	},
	{
		key: 'e9a0a28e-2d5b-4229-ac00-66f2df230513',
		$type: 'EntityTreeItemViewModel',
		isContainer: false,
		parentKey: null,
		name: 'Related Document',
		icon: 'umb:trafic',
		type: 'relation-type',
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbRelationTypeData extends UmbEntityData<RelationTypeResponseModel> {
	private treeData = treeData;
	constructor() {
		super(data);
	}

	//TODO Can relation types have children?
	getTreeRoot(): Array<EntityTreeItemResponseModel> {
		const rootItems = this.treeData;
		return rootItems.map((item) => createEntityTreeItem(item));
	}

	//TODO Can relation types have children?
	getTreeItemChildren(key: string): Array<EntityTreeItemResponseModel> {
		const childItems = this.treeData.filter((item) => item.parentKey === key);
		return childItems.map((item) => createEntityTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItemResponseModel> {
		const items = this.treeData.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbRelationTypeData = new UmbRelationTypeData();
