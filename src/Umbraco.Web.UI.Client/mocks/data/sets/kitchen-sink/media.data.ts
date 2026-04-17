import type { UmbMockMediaModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockMediaModel> = [
	{
		hasChildren: true,
		id: '5deac19f-5ca8-4b8c-a784-26593cec8d51',
		createDate: '2023-02-20 15:33:36',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d',
			icon: 'icon-folder',
			collection: {
				id: '3a0156c4-3b8c-4803-bdc1-6871faa83fff',
			},
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-20 15:33:36',
				culture: null,
				segment: null,
				name: 'Folder One',
				createDate: '2023-02-20 15:33:36',
				updateDate: '2023-02-20 15:33:36',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: '76c02ec8-6a82-4c47-95da-56f6628b58fb',
		createDate: '2023-02-20 16:27:34',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'cc07b313-0843-4aa8-bbda-871c8da728c8',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'umbracoFile',
				value: {
					focalPoint: {
						left: 0.5,
						top: 0.5,
					},
					crops: [],
					src: '/umbraco/backoffice/assets/umb-pattern-pink.png',
				},
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoWidth',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoHeight',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoBytes',
				value: '4314',
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoExtension',
				value: 'png',
			},
		],
		variants: [
			{
				publishDate: '2026-04-16 10:47:31.6924139',
				culture: null,
				segment: null,
				name: 'Placeholder 73640',
				createDate: '2023-02-20 16:27:34',
				updateDate: '2026-04-16 10:47:31.6924139',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: 'ee9c1205-4121-4610-b0b3-a522dfd3461d',
		createDate: '2024-02-22 09:00:58',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'cc07b313-0843-4aa8-bbda-871c8da728c8',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'umbracoFile',
				value: {
					focalPoint: {
						left: 0.5,
						top: 0.5,
					},
					crops: [],
					src: '/umbraco/backoffice/assets/umb-pattern--blue.png',
				},
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoWidth',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoHeight',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoBytes',
				value: '3179',
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoExtension',
				value: 'png',
			},
		],
		variants: [
			{
				publishDate: '2026-04-16 10:47:38.7387294',
				culture: null,
				segment: null,
				name: 'Placeholder 5085407',
				createDate: '2024-02-22 09:00:58',
				updateDate: '2026-04-16 10:47:38.7387294',
			},
		],
		flags: [],
	},
	{
		hasChildren: true,
		id: '95a8b7fc-cfec-4fb4-bce2-2dd1bc4818b4',
		createDate: '2023-02-20 15:33:40',
		parent: {
			id: '5deac19f-5ca8-4b8c-a784-26593cec8d51',
		},
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d',
			icon: 'icon-folder',
			collection: {
				id: '3a0156c4-3b8c-4803-bdc1-6871faa83fff',
			},
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-20 15:33:40',
				culture: null,
				segment: null,
				name: 'Folder One Child',
				createDate: '2023-02-20 15:33:40',
				updateDate: '2023-02-20 15:33:40',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: 'b44956af-620a-4e17-bbce-3987446fb2f1',
		createDate: '2023-02-20 16:27:38',
		parent: {
			id: '5deac19f-5ca8-4b8c-a784-26593cec8d51',
		},
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'cc07b313-0843-4aa8-bbda-871c8da728c8',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'umbracoFile',
				value: {
					focalPoint: {
						left: 0.5,
						top: 0.5,
					},
					crops: [],
					src: '/umbraco/backoffice/assets/umb-pattern-pink.png',
				},
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoWidth',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoHeight',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoBytes',
				value: '4314',
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoExtension',
				value: 'png',
			},
		],
		variants: [
			{
				publishDate: '2026-04-16 10:47:47.7991377',
				culture: null,
				segment: null,
				name: 'Placeholder 1435904',
				createDate: '2023-02-20 16:27:38',
				updateDate: '2026-04-16 10:47:47.7991377',
			},
		],
		flags: [],
	},
	{
		hasChildren: true,
		id: 'bb5e18f3-c2e8-4800-9d1f-5584d5173a5d',
		createDate: '2023-02-20 15:33:49',
		parent: {
			id: '95a8b7fc-cfec-4fb4-bce2-2dd1bc4818b4',
		},
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d',
			icon: 'icon-folder',
			collection: {
				id: '3a0156c4-3b8c-4803-bdc1-6871faa83fff',
			},
		},
		values: [],
		variants: [
			{
				publishDate: '2023-02-20 15:33:49',
				culture: null,
				segment: null,
				name: 'Folder One Grandchild',
				createDate: '2023-02-20 15:33:49',
				updateDate: '2023-02-20 15:33:49',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: 'f06adb91-8cdd-408d-83dd-f7b833fc393c',
		createDate: '2023-02-20 16:27:44',
		parent: {
			id: '95a8b7fc-cfec-4fb4-bce2-2dd1bc4818b4',
		},
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'cc07b313-0843-4aa8-bbda-871c8da728c8',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'umbracoFile',
				value: {
					focalPoint: {
						left: 0.5,
						top: 0.5,
					},
					crops: [],
					src: '/umbraco/backoffice/assets/umb-pattern-blue.png',
				},
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoWidth',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoHeight',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoBytes',
				value: '3179',
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoExtension',
				value: 'png',
			},
		],
		variants: [
			{
				publishDate: '2026-04-16 10:47:55.4627635',
				culture: null,
				segment: null,
				name: 'Placeholder 143133',
				createDate: '2023-02-20 16:27:44',
				updateDate: '2026-04-16 10:47:55.4627635',
			},
		],
		flags: [],
	},
	{
		hasChildren: false,
		id: 'a0651d98-14a9-4d92-8133-36f59b248d31',
		createDate: '2023-02-20 16:27:47',
		parent: {
			id: 'bb5e18f3-c2e8-4800-9d1f-5584d5173a5d',
		},
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'cc07b313-0843-4aa8-bbda-871c8da728c8',
			icon: 'icon-picture',
		},
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'umbracoFile',
				value: {
					focalPoint: {
						left: 0.5,
						top: 0.5,
					},
					crops: [],
					src: '/umbraco/backoffice/assets/umb-pattern-pink.png',
				},
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoWidth',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoHeight',
				value: 768,
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoBytes',
				value: '4314',
			},
			{
				editorAlias: 'Umbraco.Label',
				alias: 'umbracoExtension',
				value: 'png',
			},
		],
		variants: [
			{
				publishDate: '2026-04-16 10:48:03.3230371',
				culture: null,
				segment: null,
				name: 'Placeholder 2255924',
				createDate: '2023-02-20 16:27:47',
				updateDate: '2026-04-16 10:48:03.3230371',
			},
		],
		flags: [],
	},
];
