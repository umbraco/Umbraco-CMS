import type { UmbMockMediaModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockMediaModel> = [
	{
		hasChildren: false,
		id: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.UploadField',
				alias: 'mediaPicker',
				value: {
					src: '/umbraco/backoffice/assets/installer-illustration.svg',
				},
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'The daily life at Umbraco HQ',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'Flipped Car',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: 'f2f81a40-c989-4b6b-84e2-057cecd3grd4',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: true,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.UploadField',
				alias: 'mediaPicker',
				value: {
					src: '/umbraco/backoffice/assets/installer-illustration.svg',
				},
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'The daily life at Umbraco HQ',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'Permissions - No Access',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '69431027-8867-45bf-a93b-72bbdabfb177',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
		},
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'Umbracoffee',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-72bbdabfb177',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			collection: { id: 'dt-collectionView' },
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'People',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-5224dabfb177',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			collection: { id: 'dt-collectionView' },
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'John Smith',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabfb177',
		createDate: '2023-02-06T15:32:05.350038',
		parent: { id: '69461027-8867-45bf-a93b-72bbdabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
		},
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'Jane Doe',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabf2147',
		createDate: '2023-02-06T15:32:05.350038',
		parent: { id: '69461027-8867-45bf-a93b-72bbdabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
		},
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'John Doe',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '694hdj27-8867-45s7-a93b-7uibdabf2147',
		createDate: '2023-02-06T15:32:05.350038',
		parent: { id: '69461027-8867-45bf-a93b-5224dabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
		},
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'A very nice hat',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '694hdj27-1237-45s7-a93b-7uibdabfas47',
		createDate: '2023-02-06T15:32:05.350038',
		parent: { id: '69461027-8867-45bf-a93b-5224dabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
		},
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'mediaType1Property1',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'Fancy old chair',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: 'forbidden',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.UploadField',
				alias: 'mediaPicker',
				value: {
					src: '/umbraco/backoffice/assets/installer-illustration.svg',
				},
			},
		],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'Forbidden Media',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '9b4b5e4a-1f2c-4d3e-8a71-6c0d5e2f1a01',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-5-id',
			icon: 'icon-document',
		},
		// No upload value on purpose: these stand in for media the imaging endpoint cannot preview, which is
		// where the file extension is the only hint about what the item actually is.
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'annual-report.pdf',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '9b4b5e4a-1f2c-4d3e-8a71-6c0d5e2f1a02',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-2-id',
			icon: 'icon-audio-lines',
		},
		// No upload value on purpose: these stand in for media the imaging endpoint cannot preview, which is
		// where the file extension is the only hint about what the item actually is.
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'soundtrack.mp3',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '9b4b5e4a-1f2c-4d3e-8a71-6c0d5e2f1a03',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-5-id',
			icon: 'icon-document',
		},
		// No upload value on purpose: these stand in for media the imaging endpoint cannot preview, which is
		// where the file extension is the only hint about what the item actually is.
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'assets-bundle.zip',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '9b4b5e4a-1f2c-4d3e-8a71-6c0d5e2f1a04',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-4-id',
			icon: 'icon-video',
		},
		// No upload value on purpose: these stand in for media the imaging endpoint cannot preview, which is
		// where the file extension is the only hint about what the item actually is.
		values: [],
		variants: [
			{
				publishDate: '2023-02-06T15:31:51.354764',
				culture: null,
				segment: null,
				name: 'promo-clip.mov',
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
		flags: [],
	},
];
