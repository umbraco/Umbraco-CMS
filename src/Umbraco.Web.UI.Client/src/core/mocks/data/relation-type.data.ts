import { UmbEntityData } from './entity.data';
import { createFolderTreeItem } from './utils';
import type { FolderTreeItemModel, RelationTypeResponseModel } from '@umbraco-cms/backend-api';

// TODO: investigate why we don't get an entity type as part of the RelationTypeResponseModel
export const data: Array<RelationTypeResponseModel & { type: 'relation-type' }> = [];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbRelationTypeData extends UmbEntityData<RelationTypeResponseModel> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<FolderTreeItemModel> {
		const rootItems = this.data;
		return rootItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<FolderTreeItemModel> {
		const childItems = this.data;
		return childItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<FolderTreeItemModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createFolderTreeItem(item));
	}
}

export const umbRelationTypeData = new UmbRelationTypeData();
