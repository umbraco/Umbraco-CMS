import { UmbEntityData } from './entity.data';
import { createDocumentTypeTreeItem } from './utils';
import { DocumentTypeTreeItem } from '@umbraco-cms/backend-api';
import type { DocumentTypeDetails } from '@umbraco-cms/models';

export const data: Array<DocumentTypeDetails> = [
	{
		name: 'Document Type 1',
		type: 'document-type',
		hasChildren: false,
		key: 'd81c7957-153c-4b5a-aa6f-b434a4964624',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		isElement: false,
		isTrashed: false,
		icon: '',
		alias: 'documentType1',
		properties: [],
	},
	{
		name: 'Document Type 2',
		type: 'document-type',
		hasChildren: false,
		key: 'a99e4018-3ffc-486b-aa76-eecea9593d17',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		isElement: false,
		isTrashed: false,
		icon: '',
		alias: 'documentType2',
		properties: [],
	},
];

// Temp mocked database
class UmbDocumentTypeData extends UmbEntityData<DocumentTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<DocumentTypeTreeItem> {
		const rootItems = this.data.filter((item) => item.parentKey === null);
		return rootItems.map((item) => createDocumentTypeTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<DocumentTypeTreeItem> {
		const childItems = this.data.filter((item) => item.parentKey === key);
		return childItems.map((item) => createDocumentTypeTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<DocumentTypeTreeItem> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createDocumentTypeTreeItem(item));
	}
}

export const umbDocumentTypeData = new UmbDocumentTypeData();