import type { UmbMockElementModel } from '../../mock-data-set.types.js';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';

const elementOneLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-element-one-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'b818bb55-31e1-4537-9c42-17471a176089',
		icon: 'icon-attachment color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Element One (Library)',
	variants: [
		{
			state: UmbElementVariantState.PUBLISHED,
			culture: null,
			name: 'Element One (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-element-one',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: null,
			segment: null,
			value: 'Reusable Element One',
		},
	],
	flags: [],
	noAccess: false,
};

const elementTwoLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-element-two-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
		icon: 'icon-blueprint color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Element Two (Library)',
	variants: [
		{
			state: UmbElementVariantState.PUBLISHED,
			culture: null,
			name: 'Element Two (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-element-two',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.ContentPicker',
			alias: 'link',
			culture: null,
			segment: null,
			value: '17cd53f2-93b3-4e34-ade2-916e7a6639ed',
		},
	],
	flags: [],
	noAccess: false,
};

const variantLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-variant-element-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
		icon: 'icon-science color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Variant Element (Library)',
	variants: [
		{
			state: UmbElementVariantState.PUBLISHED,
			culture: 'en-US',
			name: 'Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-variant-element-en-us',
			flags: [],
		},
		{
			state: UmbElementVariantState.NOT_CREATED,
			culture: 'da',
			name: 'Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: null,
			id: 'library-variant-element-da',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'en-US',
			segment: null,
			value: 'Reusable Variant Element (English only)',
		},
	],
	flags: [],
	noAccess: false,
};

const draftLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-variant-element-draft-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
		icon: 'icon-science color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Draft Variant Element (Library)',
	variants: [
		{
			state: UmbElementVariantState.DRAFT,
			culture: 'en-US',
			name: 'Draft Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: null,
			id: 'library-variant-element-draft-en-us',
			flags: [],
		},
		{
			state: UmbElementVariantState.DRAFT,
			culture: 'da',
			name: 'Draft Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: null,
			id: 'library-variant-element-draft-da',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'en-US',
			segment: null,
			value: 'Draft in both cultures',
		},
	],
	flags: [],
	noAccess: false,
};

const pendingChangesLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-variant-element-pending-changes-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
		icon: 'icon-science color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Pending Changes Variant Element (Library)',
	variants: [
		{
			state: UmbElementVariantState.PUBLISHED_PENDING_CHANGES,
			culture: 'en-US',
			name: 'Pending Changes Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-variant-element-pending-changes-en-us',
			flags: [],
		},
		{
			state: UmbElementVariantState.PUBLISHED_PENDING_CHANGES,
			culture: 'da',
			name: 'Pending Changes Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-variant-element-pending-changes-da',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'en-US',
			segment: null,
			value: 'Published with pending changes (English)',
		},
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'da',
			segment: null,
			value: 'Udgivet med afventende ændringer (dansk)',
		},
	],
	flags: [],
	noAccess: false,
};

const publishedBothLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-variant-element-published-both-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
		icon: 'icon-science color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Published Variant Element (Library)',
	variants: [
		{
			state: UmbElementVariantState.PUBLISHED,
			culture: 'en-US',
			name: 'Published Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-variant-element-published-both-en-us',
			flags: [],
		},
		{
			state: UmbElementVariantState.PUBLISHED,
			culture: 'da',
			name: 'Published Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-variant-element-published-both-da',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'en-US',
			segment: null,
			value: 'Published in both cultures (English)',
		},
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'da',
			segment: null,
			value: 'Udgivet på begge sprog (dansk)',
		},
	],
	flags: [],
	noAccess: false,
};

const missingDaLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-variant-element-missing-da-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
		icon: 'icon-science color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'Missing Danish Variant Element (Library)',
	variants: [
		{
			state: UmbElementVariantState.PUBLISHED,
			culture: 'en-US',
			name: 'Missing Danish Variant Element (Library)',
			createDate: '2024-01-15T10:00:00.000Z',
			updateDate: '2024-01-15T10:00:00.000Z',
			publishDate: '2024-01-15T10:00:00.000Z',
			id: 'library-variant-element-missing-da-en-us',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'en-US',
			segment: null,
			value: 'No Danish variant exists',
		},
	],
	flags: [],
	noAccess: false,
};

const emptyVariantsLibraryElement: UmbMockElementModel = {
	ancestors: [],
	id: 'library-variant-element-empty-variants-id',
	createDate: '2024-01-15T10:00:00.000Z',
	parent: null,
	documentType: {
		id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
		icon: 'icon-science color-deep-purple',
	},
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	name: 'No Variants Element (Library)',
	variants: [],
	values: [
		{
			editorAlias: 'Umbraco.TextBox',
			alias: 'title',
			culture: 'en-US',
			segment: null,
			value: 'Element with an empty variants array',
		},
	],
	flags: [],
	noAccess: false,
};

export const data: Array<UmbMockElementModel> = [
	elementOneLibraryElement,
	elementTwoLibraryElement,
	variantLibraryElement,
	draftLibraryElement,
	pendingChangesLibraryElement,
	publishedBothLibraryElement,
	missingDaLibraryElement,
	emptyVariantsLibraryElement,
];
