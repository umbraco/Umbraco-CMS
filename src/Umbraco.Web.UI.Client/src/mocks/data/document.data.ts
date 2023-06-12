import { UmbEntityData } from './entity.data.js';
import { createDocumentTreeItem } from './utils.js';
import {
	ContentStateModel,
	DocumentResponseModel,
	DocumentTreeItemResponseModel,
	PagedDocumentTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<DocumentResponseModel> = [
	{
		urls: [
			{
				culture: 'en-US',
				url: '/',
			},
		],
		templateId: null,
		id: 'all-property-editors-document-id',
		contentTypeId: 'all-property-editors-document-type-id',
		values: [
			{
				$type: '', 
				alias: 'richTextEditor',
				culture: null,
				segment: null,
				value: 'Some value for the RTE with an <a href="http://foo.com">external link</a> and an <a href="/{localLink:umb://document/c05da24d7740447b9cdcbd8ce2172e38}">internal link</a> foo foo <div class="umb-macro-holder TestMacro umb-macro-mce_1 mceNonEditable"><!-- <?UMBRACO_MACRO macroAlias="TestMacro" /> --><ins>Macro alias: <strong>TestMacro</strong></ins></div>',
			},
			{
				$type: '',
				alias: 'email',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'colorPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'contentPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'eyeDropper',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'multiUrlPicker',
				culture: 'en-us',
				segment: null,
				value: [
					{
						name: undefined,
						published: undefined,
						queryString: undefined,
						target: undefined,
						trashed: undefined,
						udi: 'umb://document/c05da24d7740447b9cdcbd8ce2172e38',
						url: 'umb://document/c05da24d7740447b9cdcbd8ce2172e38',
					},
				],
			},
			{
				$type: '',
				alias: 'multiUrlPicker',
				culture: 'da-dk',
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'multiNodeTreePicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'datePicker',
				culture: null,
				segment: null,
				value: '2023-12-24',
			},
			{
				$type: '',
				alias: 'datePickerTime',
				culture: null,
				segment: null,
				value: '2023-12-24 14:52',
			},
			{
				$type: '',
				alias: 'time',
				culture: null,
				segment: null,
				value: '14:52:00',
			},
			{
				$type: '',
				alias: 'email',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'textBox',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'dropdown',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'textArea',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'slider',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'toggle',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'tags',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'markdownEditor',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'radioButtonList',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'checkboxList',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'blockList',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'mediaPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'imageCropper',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'uploadField',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'blockGrid',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'blockGrid',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'numberRange',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'orderDirection',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'overlaySize',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'label',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'integer',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'decimal',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'memberPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'memberGroupPicker',
				culture: null,
				segment: null,
				value: null,
			},
			{
				$type: '',
				alias: 'userPicker',
				culture: null,
				segment: null,
				value: null,
			},
		],
		variants: [
			{
				$type: '',
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'All properties',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				$type: '',
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'da-dk',
				segment: null,
				name: 'Alle redigeringsfelter',
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
		templateId: null,
		id: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		contentTypeId: '29643452-cff9-47f2-98cd-7de4b6807681',
		values: [
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'masterText',
				value: 'i have a master text',
			},
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'pageTitle',
				value: 'with a page title',
			},
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'blogPostText',
				value: 'My first blog post',
			},
			{
				$type: '',
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'in the master tab',
			},
			{
				$type: '',
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'which is under another group in the tab',
			},
			{
				$type: '',
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'på master dokument tab B',
			},
			{
				$type: '',
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'denne er under en anden gruppe i tab B',
			},
			{
				$type: '',
				culture: 'no-no',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'Norsk på master dokument tab B',
			},
			{
				$type: '',
				culture: 'no-no',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'Norsk denne er under en anden gruppe i tab B',
			},
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'localBlogTabString',
				value: '1234567',
			},
		],
		variants: [
			{
				$type: '',
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'Article in english',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				$type: '',
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'da-dk',
				segment: null,
				name: 'Artikel på Dansk',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				$type: '',
				state: ContentStateModel.PUBLISHED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'no-no',
				segment: null,
				name: 'Artikel på Norsk',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				$type: '',
				state: ContentStateModel.PUBLISHED_PENDING_CHANGES,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'es-es',
				segment: null,
				name: 'Articulo en ingles',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
			{
				$type: '',
				state: ContentStateModel.NOT_CREATED,
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'pl-pl',
				segment: null,
				name: 'Artykuł w języku polskim',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
	},
	{
		urls: [],
		templateId: null,
		id: 'fd56a0b5-01a0-4da2-b428-52773bfa9cc4',
		contentTypeId: '29643452-cff9-47f2-98cd-7de4b6807681',
		values: [
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'masterText',
				value: 'i have a master text B',
			},
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'pageTitle',
				value: 'with a page title B',
			},
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'blogPostText',
				value: 'My first blog post B',
			},
			{
				$type: '',
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'in the master tab B',
			},
			{
				$type: '',
				culture: 'en-us',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'which is under another group in the tab B',
			},
			{
				$type: '',
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderMasterTab',
				value: 'på master dokument tab B',
			},
			{
				$type: '',
				culture: 'da-dk',
				segment: null,
				alias: 'blogTextStringUnderGroupUnderMasterTab',
				value: 'denne er under en anden gruppe i tab B',
			},
			{
				$type: '',
				culture: null,
				segment: null,
				alias: 'localBlogTabString',
				value: '1234567890',
			},
		],
		variants: [
			{
				$type: '',
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
	{
		urls: [],
		templateId: null,
		id: 'simple-document-id',
		contentTypeId: 'simple-document-type-id',
		variants: [
			{
				$type: '',
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

export const treeData: Array<DocumentTreeItemResponseModel> = [
	{
		$type: 'DocumentTreeItemViewModel',
		isProtected: false,
		isPublished: true,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		id: 'all-property-editors-document-id',
		isContainer: false,
		parentId: null,
		name: 'All property editors',
		type: 'document',
		icon: 'document',
		hasChildren: false,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		isProtected: false,
		isPublished: true,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		id: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		isContainer: false,
		parentId: null,
		name: 'Article in english',
		type: 'document',
		icon: 'document',
		hasChildren: true,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		isProtected: false,
		isPublished: false,
		isEdited: false,
		noAccess: false,
		isTrashed: false,
		id: 'fd56a0b5-01a0-4da2-b428-52773bfa9cc4',
		isContainer: false,
		parentId: 'c05da24d-7740-447b-9cdc-bd8ce2172e38',
		name: 'Blog post B',
		type: 'document',
		icon: 'document',
		hasChildren: false,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		name: 'Document 5',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		id: 'f6n7a5b2-e7c1-463a-956bc-6ck5b9bdf447',
		isContainer: false,
		parentId: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
	},
	{
		$type: 'DocumentTreeItemViewModel',
		name: 'Simple',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		id: 'simple-document-id',
		isContainer: false,
		parentId: null,
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
class UmbDocumentData extends UmbEntityData<DocumentResponseModel> {
	private treeData = treeData;

	constructor() {
		super(data);
	}

	// TODO: Can we do this smarter so we don't need to make this for each mock data:
	insert(item: DocumentResponseModel) {
		const result = super.insert(item);
		this.treeData.push(createDocumentTreeItem(result));
		return result;
	}

	update(id: string, item: DocumentResponseModel) {
		const result = super.save(id, item);
		this.treeData = this.treeData.map((x) => {
			if (x.id === result.id) {
				return createDocumentTreeItem(result);
			} else {
				return x;
			}
		});
		return result;
	}

	getTreeRoot(): PagedDocumentTreeItemResponseModel {
		const items = this.treeData.filter((item) => item.parentId === null);
		const treeItems = items.map((item) => item);
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedDocumentTreeItemResponseModel {
		const items = this.treeData.filter((item) => item.parentId === id);
		const treeItems = items.map((item) => item);
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<DocumentTreeItemResponseModel> {
		const items = this.treeData.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => item);
	}
}

export const umbDocumentData = new UmbDocumentData();
