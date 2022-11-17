import { Entity } from './entities';
import { UmbEntityData } from './entity.data';
import { DocumentTypeTreeItem } from '@umbraco-cms/backend-api';

export interface DocumentTypeEntity extends Entity {
	key: string;
	name: string;
	alias: string;
	type: string;
	parentKey: string;
	isTrashed: boolean;
	hasChildren: boolean;
	properties: [];
}

export const data: Array<DocumentTypeEntity> = [
	{
		key: 'd81c7957-153c-4b5a-aa6f-b434a4964624',
		name: 'Document Type 1',
		alias: 'documentType1',
		type: 'documentType',
		parentKey: 'f50eb86d-3ef2-4011-8c5d-c56c04eec0da',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		properties: [],
	},
	{
		key: 'a99e4018-3ffc-486b-aa76-eecea9593d17',
		name: 'Document Type 2',
		alias: 'documentType2',
		type: 'documentType',
		parentKey: 'f50eb86d-3ef2-4011-8c5d-c56c04eec0da',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		properties: [],
	},
];

// Temp mocked database
class UmbDocumentTypeData extends UmbEntityData<DocumentTypeEntity> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<DocumentTypeTreeItem> {
		return this.data.filter((item) => item.parentKey === null);
	}

	getTreeItemChildren(key: string): Array<DocumentTypeTreeItem> {
		return this.data.filter((item) => item.parentKey === key);
	}

	getTreeItem(keys: Array<string>): Array<DocumentTypeTreeItem> {
		return this.data.filter((item) => keys.includes(item.key ?? ''));
	}
}

export const umbDocumentTypeData = new UmbDocumentTypeData();
