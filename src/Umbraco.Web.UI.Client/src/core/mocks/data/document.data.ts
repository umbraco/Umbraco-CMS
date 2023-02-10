import { UmbEntityData } from './entity.data';
import { createDocumentTreeItem } from './utils';
import { DocumentTreeItemModel, PagedDocumentTreeItemModel } from '@umbraco-cms/backend-api';
import type { DocumentDetails } from '@umbraco-cms/models';

export const data: Array<DocumentDetails> = [
	{
		name: 'Multiple Text String',
		type: 'document',
		icon: 'favorite',
		hasChildren: false,
		key: '6f31e382-458c-4f96-95ea-cc26c51009d4',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		properties: [
			{
				alias: 'multipleTextString',
				label: 'Multiple Text String',
				description: '',
				dataTypeKey: 'dt-multipleTextString',
			},
		],
		data: [
			{
				alias: 'multipleTextString',
				value: [
					{
						value: 'Value 1',
					},
					{
						value: 'Value 2',
					},
					{
						value: 'Value 3',
					},
				],
			},
		],
		variants: [],
	},
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
	{
		name: 'Document 1',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myHeadline',
				label: 'Headline',
				description: 'Text string property',
				dataTypeKey: 'dt-textBox',
			},
			{
				alias: 'myDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeKey: 'dt-textArea',
			},
		],
		data: [
			{
				alias: 'myHeadline',
				value: 'The daily life at Umbraco HQ',
			},
			{
				alias: 'myDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [{ name: 'fake data' }],
		/*
	// Concept for node layout, separation of design from config and data.
	layout: [
	  {
		type: 'group',
		children: [
		  {
			type: 'property',
			alias: 'myHeadline'
		  },
		  {
			type: 'property',
			alias: 'myDescription'
		  }
		]
	  }
	],
	*/
	},
	{
		name: 'Document 2',
		type: 'document',
		icon: 'favorite',
		hasChildren: false,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myHeadline',
				label: 'Text string label',
				description: 'this is a text string property',
				dataTypeKey: 'dt-textBox',
			},
			{
				alias: 'myDescription',
				label: 'Textarea label',
				description: 'This is the a textarea property',
				dataTypeKey: 'dt-textArea',
			},
			{
				alias: 'myExternalEditor',
				label: 'My JS Property Editor',
				description: 'This is the a external property',
				dataTypeKey: 'dt-custom',
			},
			{
				alias: 'myContentPicker',
				label: 'Content Picker',
				description: 'This is a content picker',
				dataTypeKey: 'dt-contentPicker',
			},
		],
		data: [
			{
				alias: 'myHeadline',
				value: 'Is it all just fun and curling and scary rabbits?',
			},
			{
				alias: 'myDescription',
				value:
					"So no, there's not confetti every day. And no, there's not champagne every week or a crazy rabbit running around 🐰",
			},
			{
				alias: 'myExternalEditor',
				value: 'Tex lkasdfkljdfsa 1',
			},
			{
				alias: 'myContextExampleEditor',
				value: '',
			},
			{
				alias: 'myContentPicker',
				value: '',
			},
		],
		variants: [{ name: 'Variant 1' }],
	},
	{
		name: 'Document 3',
		type: 'document',
		icon: 'document',
		hasChildren: true,
		key: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeKey: 'dt-textArea',
			},
		],
		data: [
			{
				alias: 'myDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
	},
	{
		name: 'Document 4',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		key: 'f6f7a5b2-e7c0-463a-97bc-6cb5b9bcf447',
		isContainer: false,
		parentKey: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeKey: 'dt-textArea',
			},
		],
		data: [],
		variants: [],
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDocumentData extends UmbEntityData<DocumentDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedDocumentTreeItemModel {
		const items = this.data.filter((item) => item.parentKey === null);
		const treeItems = items.map((item) => createDocumentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(key: string): PagedDocumentTreeItemModel {
		const items = this.data.filter((item) => item.parentKey === key);
		const treeItems = items.map((item) => createDocumentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(keys: Array<string>): Array<DocumentTreeItemModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createDocumentTreeItem(item));
	}
}

export const umbDocumentData = new UmbDocumentData();
