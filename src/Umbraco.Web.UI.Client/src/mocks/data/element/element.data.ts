import type {
	ElementItemResponseModel,
	ElementResponseModel,
	ElementTreeItemResponseModel,
	ElementVariantResponseModel,
	DocumentTypeReferenceResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockElementModel = Omit<ElementResponseModel, 'documentType'> &
	Omit<ElementTreeItemResponseModel, 'documentType' | 'variants'> &
	Omit<ElementItemResponseModel, 'documentType' | 'variants'> & {
		ancestors: Array<{ id: string }>;
		createDate: string;
		documentType: DocumentTypeReferenceResponseModel | null;
		variants: Array<ElementVariantResponseModel>;
	};

export const data: Array<UmbMockElementModel> = [
	{
		ancestors: [],
		id: 'simple-element-id',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		documentType: {
			id: '4f68ba66-6fb2-4778-83b8-6ab4ca3a7c5c',
			icon: 'icon-lab',
		},
		hasChildren: false,
		isTrashed: false,
		isFolder: false,
		name: 'Simple Element',
		variants: [
			{
				state: DocumentVariantStateModel.PUBLISHED,
				culture: null,
				segment: null,
				name: 'Simple Element',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				publishDate: '2024-01-15T10:00:00.000Z',
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'elementProperty',
				culture: null,
				segment: null,
				value: 'Simple Element Title',
			},
		],
		flags: [],
		noAccess: false,
	},
	{
		ancestors: [],
		id: 'element-folder-id',
		createDate: '2024-01-14T09:00:00.000Z',
		parent: null,
		documentType: null,
		hasChildren: true,
		isTrashed: false,
		isFolder: true,
		name: 'Element Folder',
		variants: [],
		values: [],
		flags: [],
		noAccess: false,
	},
	{
		ancestors: [{ id: 'element-folder-id' }],
		id: 'element-in-folder-id',
		createDate: '2024-01-16T14:00:00.000Z',
		parent: { id: 'element-folder-id' },
		documentType: {
			id: '4f68ba66-6fb2-4778-83b8-6ab4ca3a7c5c',
			icon: 'icon-lab',
		},
		hasChildren: false,
		isTrashed: false,
		isFolder: false,
		name: 'Element In Folder',
		variants: [
			{
				state: DocumentVariantStateModel.PUBLISHED,
				culture: null,
				segment: null,
				name: 'Element In Folder',
				createDate: '2024-01-16T14:00:00.000Z',
				updateDate: '2024-01-16T14:00:00.000Z',
				publishDate: '2024-01-16T14:00:00.000Z',
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'elementProperty',
				culture: null,
				segment: null,
				value: 'This is an element inside a folder.',
			},
		],
		flags: [],
		noAccess: false,
	},
	{
		ancestors: [],
		id: 'element-subfolder-1-id',
		createDate: '2024-01-14T09:00:00.000Z',
		parent: { id: 'element-folder-id' },
		documentType: null,
		hasChildren: true,
		isTrashed: false,
		isFolder: true,
		name: 'Element Subfolder 1',
		variants: [],
		values: [],
		flags: [],
		noAccess: false,
	},
	{
		ancestors: [{ id: 'element-folder-id' }, { id: 'element-subfolder-1-id' }],
		id: 'element-in-subfolder-1-id',
		createDate: '2024-01-16T14:00:00.000Z',
		parent: { id: 'element-subfolder-1-id' },
		documentType: {
			id: '4f68ba66-6fb2-4778-83b8-6ab4ca3a7c5c',
			icon: 'icon-lab',
		},
		hasChildren: false,
		isTrashed: false,
		isFolder: false,
		name: 'Element In Subfolder 1',
		variants: [
			{
				state: DocumentVariantStateModel.PUBLISHED,
				culture: null,
				segment: null,
				name: 'Element In Subfolder 1',
				createDate: '2024-01-16T14:00:00.000Z',
				updateDate: '2024-01-16T14:00:00.000Z',
				publishDate: '2024-01-16T14:00:00.000Z',
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'elementProperty',
				culture: null,
				segment: null,
				value: 'This is an element inside a subfolder 1.',
			},
		],
		flags: [],
		noAccess: false,
	},
];
