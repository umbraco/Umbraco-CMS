import { UmbEntityData } from './entity.data';
import { createDocumentTreeItem } from './utils';
import {
	ContentStateModel,
	DocumentModel,
	DocumentTreeItemModel,
	PagedDocumentTreeItemModel,
} from '@umbraco-cms/backend-api';

export const data: Array<DocumentModel> = [
	{
		urls: [
			{
				culture: 'en-US',
				url: '/',
			},
		],
		templateKey: null,
		key: 'all-property-editors-document-key',
		contentTypeKey: 'all-property-editors-document-type-key',
		values: [
			{
				alias: 'email',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'colorPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'contentPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'eyeDropper',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'multiUrlPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'multiNodeTreePicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'datePicker',
				culture: null,
				segment: null,
				value: '2023-12-24',
			},
			{
				alias: 'datePickerTime',
				culture: null,
				segment: null,
				value: '2023-12-24 14:52',
			},
			{
				alias: 'time',
				culture: null,
				segment: null,
				value: '14:52:00',
			},
			{
				alias: 'email',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'textBox',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'dropdown',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'textArea',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'slider',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'toggle',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'tags',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'markdownEditor',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'radioButtonList',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'checkboxList',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'blockList',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'mediaPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'imageCropper',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'uploadField',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'blockGrid',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'blockGrid',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'numberRange',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'orderDirection',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'overlaySize',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'label',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'integer',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'decimal',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'memberPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'memberGroupPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				alias: 'userPicker',
				culture: null,
				segment: null,
				value: null,
			},
		],
		variants: [
			{
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'All properties',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
	},
	{
		urls: [
			{
				culture: 'en-US',
				url: '/',
			},
		],
		templateKey: null,
		key: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		contentTypeKey: '29643452-cff9-47f2-98cd-7de4b6807681',
		values: [
			{
				culture: null,
				segment: null,
				alias: 'masterText',
				value: 'i have a master text',
			},
			{
				culture: null,
				segment: null,
				alias: 'pageTitle',
				value: 'with a page title',
			},
			{
				culture: null,
				segment: null,
				alias: 'blogPostText',
				value: 'My first blog post',
			},
			{
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'in the master tab',
			},
			{
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'which is under another group in the tab',
			},
			{
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'på master dokument tab B',
			},
			{
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'denne er under en anden gruppe i tab B',
			},
			{
				culture: 'no-no',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'Norsk på master dokument tab B',
			},
			{
				culture: 'no-no',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'Norsk denne er under en anden gruppe i tab B',
			},
			{
				culture: null,
				segment: null,
				alias: 'localBlogTabString',
				value: '1234567',
			},
		],
		variants: [
			{
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'Article in english',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'da-dk',
				segment: null,
				name: 'Artikel på Dansk',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'no-no',
				segment: null,
				name: 'Artikel på Norsk',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
	},
	{
		urls: [],
		templateKey: null,
		key: 'fd56a0b5-01a0-4da2-b428-52773bfa9cc4',
		contentTypeKey: '29643452-cff9-47f2-98cd-7de4b6807681',
		values: [
			{
				culture: null,
				segment: null,
				alias: 'masterText',
				value: 'i have a master text B',
			},
			{
				culture: null,
				segment: null,
				alias: 'pageTitle',
				value: 'with a page title B',
			},
			{
				culture: null,
				segment: null,
				alias: 'blogPostText',
				value: 'My first blog post B',
			},
			{
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'in the master tab B',
			},
			{
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'which is under another group in the tab B',
			},
			{
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'på master dokument tab B',
			},
			{
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'denne er under en anden gruppe i tab B',
			},
			{
				culture: null,
				segment: null,
				alias: 'localBlogTabString',
				value: '1234567890',
			},
		],
		variants: [
			{
				state: ContentStateModel.DRAFT,
				publishDate: '2023-02-06T15:32:24.957009',
				culture: 'en-us',
				segment: null,
				name: 'Blog post B',
				createDate: '2023-02-06T15:32:05.350038',
				updateDate: '2023-02-06T15:32:24.957009',
			},
		],
	},
];

export const treeData: Array<DocumentTreeItemModel> = [
	{
		$type: 'DocumentTreeItemViewModel',
		isProtected: false,
		isPublished: true,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		key: 'all-property-editors-document-key',
		isContainer: false,
		parentKey: null,
		name: 'All property editors',
		type: 'document',
		icon: 'icon-item-arrangement',
		hasChildren: true,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		isProtected: false,
		isPublished: true,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		key: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		isContainer: false,
		parentKey: null,
		name: 'Article in english',
		type: 'document',
		icon: 'icon-item-arrangement',
		hasChildren: true,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		isProtected: false,
		isPublished: false,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		key: 'fd56a0b5-01a0-4da2-b428-52773bfa9cc4',
		isContainer: false,
		parentKey: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		name: 'Blog post B',
		type: 'document',
		icon: 'icon-item-arrangement',
		hasChildren: false,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		name: 'Document 5',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		key: 'f6n7a5b2-e7c1-463a-956bc-6ck5b9bdf447',
		isContainer: false,
		parentKey: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDocumentData extends UmbEntityData<DocumentModel> {
	private treeData = treeData;

	constructor() {
		super(data);
	}

	getTreeRoot(): PagedDocumentTreeItemModel {
		const items = this.treeData.filter((item) => item.parentKey === null);
		const treeItems = items.map((item) => createDocumentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(key: string): PagedDocumentTreeItemModel {
		const items = this.treeData.filter((item) => item.parentKey === key);
		const treeItems = items.map((item) => createDocumentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(keys: Array<string>): Array<DocumentTreeItemModel> {
		const items = this.treeData.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createDocumentTreeItem(item));
	}
}

export const umbDocumentData = new UmbDocumentData();
