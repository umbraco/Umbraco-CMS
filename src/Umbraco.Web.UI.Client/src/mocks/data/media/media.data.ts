import type {
	MediaItemResponseModel,
	MediaResponseModel,
	MediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

type UmbMockMediaModelHack = MediaResponseModel & MediaTreeItemResponseModel & MediaItemResponseModel;

export interface UmbMockMediaModel extends Omit<UmbMockMediaModelHack, 'type'> {}

export const data: Array<UmbMockMediaModel> = [
	{
		hasChildren: false,
		id: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [
			{
				alias: 'myMediaHeadline',
				value: 'The daily life at Umbraco HQ',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'Flipped Car',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
	{
		hasChildren: false,
		id: '69431027-8867-45bf-a93b-72bbdabfb177',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'Umbracoffee',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
	{
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-72bbdabfb177',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'People',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
	{
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-5224dabfb177',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'John Smith',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
	{
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabfb177',
		parent: { id: '69461027-8867-45bf-a93b-72bbdabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'Jane Doe',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
	{
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabf2147',
		parent: { id: '69461027-8867-45bf-a93b-72bbdabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [{
			publishDate: '2023-02-06T15:31:51.354764',
			culture: 'en-us',
			segment: null,
			name: 'John Doe',
			createDate: '2023-02-06T15:31:46.876902',
			updateDate: '2023-02-06T15:31:51.354764',
		},],
		urls: [],
	},
	{
		hasChildren: false,
		id: '694hdj27-8867-45s7-a93b-7uibdabf2147',
		parent: { id: '69461027-8867-45bf-a93b-5224dabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'A very nice hat',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
	{
		hasChildren: false,
		id: '694hdj27-1237-45s7-a93b-7uibdabfas47',
		parent: { id: '69461027-8867-45bf-a93b-5224dabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: 'en-us',
				segment: null,
				name: 'Fancy old chair',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		urls: [],
	},
];
