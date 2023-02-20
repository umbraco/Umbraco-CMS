import { UmbEntityData } from './entity.data';
import { createDocumentTreeItem } from './utils';
import {
	ContentStateModel,
	DocumentModel,
	DocumentTreeItemModel,
	PagedDocumentTreeItemModel,
} from '@umbraco-cms/backend-api';

/*

{
		name: 'All Property Editors',
		type: 'document',
		icon: 'favorite',
		hasChildren: false,
		key: '6f31e382-458c-4f96-97ea-cc26c41009d4',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		properties: [
			{
				alias: 'colorPicker',
				label: 'Color Picker',
				description: '',
				dataTypeKey: 'dt-colorPicker',
			},
			{
				alias: 'contentPicker',
				label: 'Content Picker',
				description: '',
				dataTypeKey: 'dt-contentPicker',
			},
			{
				alias: 'eyeDropper',
				label: 'Eye Dropper',
				description: '',
				dataTypeKey: 'dt-eyeDropper',
			},
			{
				alias: 'multiUrlPicker',
				label: 'Multi URL Picker',
				description: '',
				dataTypeKey: 'dt-multiUrlPicker',
			},
			{
				alias: 'multiNodeTreePicker',
				label: 'Multi Node Tree Picker',
				description: '',
				dataTypeKey: 'dt-multiNodeTreePicker',
			},
			{
				alias: 'datePicker',
				label: 'Date Picker',
				description: '',
				dataTypeKey: 'dt-datePicker',
			},
			{
				alias: 'email',
				label: 'Email',
				description: '',
				dataTypeKey: 'dt-email',
			},
			{
				alias: 'textBox',
				label: 'Text Box',
				description: '',
				dataTypeKey: 'dt-textBox',
			},
			{
				alias: 'dropdown',
				label: 'Dropdown',
				description: '',
				dataTypeKey: 'dt-dropdown',
			},
			{
				alias: 'textArea',
				label: 'Text Area',
				description: '',
				dataTypeKey: 'dt-textArea',
			},
			{
				alias: 'slider',
				label: 'Slider',
				description: '',
				dataTypeKey: 'dt-slider',
			},
			{
				alias: 'toggle',
				label: 'Toggle',
				description: '',
				dataTypeKey: 'dt-toggle',
			},
			{
				alias: 'tags',
				label: 'Tags',
				description: '',
				dataTypeKey: 'dt-tags',
			},
			{
				alias: 'markdownEditor',
				label: 'MarkdownEditor',
				description: '',
				dataTypeKey: 'dt-markdownEditor',
			},
			{
				alias: 'radioButtonList',
				label: 'Radio Button List',
				description: '',
				dataTypeKey: 'dt-radioButtonList',
			},
			{
				alias: 'checkboxList',
				label: 'Checkbox List',
				description: '',
				dataTypeKey: 'dt-checkboxList',
			},
			{
				alias: 'blockList',
				label: 'Block List',
				description: '',
				dataTypeKey: 'dt-blockList',
			},
			{
				alias: 'mediaPicker',
				label: 'Media Picker',
				description: '',
				dataTypeKey: 'dt-mediaPicker',
			},
			{
				alias: 'imageCropper',
				label: 'Image Cropper',
				description: '',
				dataTypeKey: 'dt-imageCropper',
			},
			{
				alias: 'uploadField',
				label: 'Upload Field',
				description: '',
				dataTypeKey: 'dt-uploadField',
			},
			{
				alias: 'blockGrid',
				label: 'Block Grid',
				description: '',
				dataTypeKey: 'dt-blockGrid',
			},
			{
				alias: 'blockGrid',
				label: 'Icon Picker',
				description: '',
				dataTypeKey: 'dt-iconPicker',
			},
			{
				alias: 'numberRange',
				label: 'Number Range',
				description: '',
				dataTypeKey: 'dt-numberRange',
			},
			{
				alias: 'orderDirection',
				label: 'Order Direction',
				description: '',
				dataTypeKey: 'dt-orderDirection',
			},
			{
				alias: 'overlaySize',
				label: 'Overlay Size',
				description: '',
				dataTypeKey: 'dt-overlaySize',
			},
			{
				alias: 'label',
				label: 'Label',
				description: '',
				dataTypeKey: 'dt-label',
			},
			{
				alias: 'integer',
				label: 'Integer',
				description: '',
				dataTypeKey: 'dt-integer',
			},
			{
				alias: 'decimal',
				label: 'Decimal',
				description: '',
				dataTypeKey: 'dt-decimal',
			},
			{
				alias: 'memberPicker',
				label: 'Member Picker',
				description: '',
				dataTypeKey: 'dt-memberPicker',
			},
			{
				alias: 'memberGroupPicker',
				label: 'Member Group Picker',
				description: '',
				dataTypeKey: 'dt-memberGroupPicker',
			},
			{
				alias: 'userPicker',
				label: 'User Picker',
				description: '',
				dataTypeKey: 'dt-userPicker',
			},
		],
		data: [],
		variants: [],
	},

*/

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
		properties: [
			// TODO: is 'properties' the correct name for this? The property comes from the doc type, and this only holds the values.
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
				value: null,
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
				name: 'Blog post A',
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
		properties: [
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
				name: 'Blog post A',
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
		properties: [
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

// TODO: make tree data:
export const treeData: Array<DocumentTreeItemModel> = [
	{
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
		isProtected: false,
		isPublished: true,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		key: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		isContainer: false,
		parentKey: null,
		name: 'Blog post A',
		type: 'document',
		icon: 'icon-item-arrangement',
		hasChildren: true,
	},
	{
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
