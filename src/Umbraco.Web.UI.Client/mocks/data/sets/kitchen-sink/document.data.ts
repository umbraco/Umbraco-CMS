import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string state to enum
/**
 *
 * @param state
 */
function mapState(state: string): DocumentVariantStateModel {
	switch (state) {
		case 'Published':
			return DocumentVariantStateModel.PUBLISHED;
		case 'Draft':
			return DocumentVariantStateModel.DRAFT;
		case 'NotCreated':
			return DocumentVariantStateModel.NOT_CREATED;
		case 'PublishedPendingChanges':
			return DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES;
		default:
			return DocumentVariantStateModel.DRAFT;
	}
}

const rawData = [
	{
		ancestors: [],
		template: null,
		id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		createDate: '2023-02-20 15:09:43',
		parent: null,
		documentType: {
			id: '7184285e-9709-4e13-8c72-1fe52f024b28',
			icon: 'icon-home color-black',
		},
		hasChildren: true,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 10:02:13.006998',
				culture: null,
				segment: null,
				name: 'Home',
				createDate: '2023-02-20 15:09:43',
				updateDate: '2026-04-16 10:02:13.006998',
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'test',
				culture: null,
				segment: null,
				value: '1234567890',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '17cd53f2-93b3-4e34-ade2-916e7a6639ed',
		createDate: '2023-04-19 09:00:36',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '1addd0ad-0e34-4386-801b-79cf7beb8cf1',
			icon: 'icon-grid color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 12:30:12.7971658',
				culture: null,
				segment: null,
				name: 'Block Grid',
				createDate: '2023-04-19 09:00:36',
				updateDate: '2026-04-16 12:30:12.7971658',
				id: '17cd53f2-93b3-4e34-ade2-916e7a6639ed',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockGrid',
				alias: 'blockGridDefaultConfig',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'Element One, full width',
								},
							],
						},
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'Element one, half width (left)',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							key: 'b3cf7aab-b1a3-4522-a089-2ba533ef29c0',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
							culture: null,
							segment: null,
						},
						{
							contentKey: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
							culture: null,
							segment: null,
						},
						{
							contentKey: 'b3cf7aab-b1a3-4522-a089-2ba533ef29c0',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.BlockGrid': [
							{
								columnSpan: 12,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/73f797f1353848f39ab857b7b6528ff4',
								settingsUdi: null,
								contentKey: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
								settingsKey: null,
							},
							{
								columnSpan: 6,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/4b97e38d176d4ffda51fff4e461f7b5d',
								settingsUdi: null,
								contentKey: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
								settingsKey: null,
							},
							{
								columnSpan: 6,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/b3cf7aabb1a34522a0892ba533ef29c0',
								settingsUdi: null,
								contentKey: 'b3cf7aab-b1a3-4522-a089-2ba533ef29c0',
								settingsKey: null,
							},
						],
					},
				},
			},
			{
				editorAlias: 'Umbraco.BlockGrid',
				alias: 'blockGridWithAreas',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: 'ab05aced-2c4b-4e1a-b96a-c1d65914f0f5',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'Element One, full width, with areas',
								},
							],
						},
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: 'ca3f479c-d506-49fd-b4d0-a16e30ccc44a',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'Element one, left area',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							key: '1d2c53ec-fa0a-46b7-bde6-79e9ada92a6d',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'db2a48d5-5883-465f-b1d7-e012af2f16d0',
								},
							],
						},
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: '95a7699f-b563-4c0c-9eed-d63648247ba3',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'Element one, half width (left)',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							key: '40b6c46b-181f-41ba-a168-a8ec1decc55e',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: '119f5ef0-31cf-4d59-9c98-2f3cbe2fa8df',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'ab05aced-2c4b-4e1a-b96a-c1d65914f0f5',
							culture: null,
							segment: null,
						},
						{
							contentKey: 'ca3f479c-d506-49fd-b4d0-a16e30ccc44a',
							culture: null,
							segment: null,
						},
						{
							contentKey: '1d2c53ec-fa0a-46b7-bde6-79e9ada92a6d',
							culture: null,
							segment: null,
						},
						{
							contentKey: '95a7699f-b563-4c0c-9eed-d63648247ba3',
							culture: null,
							segment: null,
						},
						{
							contentKey: '40b6c46b-181f-41ba-a168-a8ec1decc55e',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.BlockGrid': [
							{
								columnSpan: 12,
								rowSpan: 1,
								areas: [
									{
										key: '84186bf3-663d-48f1-9815-f2f95119a205',
										items: [
											{
												columnSpan: 6,
												rowSpan: 1,
												areas: [
													{
														key: '84186bf3-663d-48f1-9815-f2f95119a205',
														items: [],
													},
													{
														key: 'ce99380c-d6a2-4ffb-bdc4-59cc27a0735a',
														items: [],
													},
												],
												contentUdi: 'umb://element/ca3f479cd50649fdb4d0a16e30ccc44a',
												settingsUdi: null,
												contentKey: 'ca3f479c-d506-49fd-b4d0-a16e30ccc44a',
												settingsKey: null,
											},
										],
									},
									{
										key: 'ce99380c-d6a2-4ffb-bdc4-59cc27a0735a',
										items: [
											{
												columnSpan: 6,
												rowSpan: 1,
												areas: [
													{
														key: 'c92e520a-1aa2-48cb-b7c9-45d586b6dc70',
														items: [],
													},
													{
														key: '01a9b7db-7360-45bb-999c-edc438473f96',
														items: [],
													},
												],
												contentUdi: 'umb://element/1d2c53ecfa0a46b7bde679e9ada92a6d',
												settingsUdi: null,
												contentKey: '1d2c53ec-fa0a-46b7-bde6-79e9ada92a6d',
												settingsKey: null,
											},
										],
									},
								],
								contentUdi: 'umb://element/ab05aced2c4b4e1ab96ac1d65914f0f5',
								settingsUdi: null,
								contentKey: 'ab05aced-2c4b-4e1a-b96a-c1d65914f0f5',
								settingsKey: null,
							},
							{
								columnSpan: 6,
								rowSpan: 1,
								areas: [
									{
										key: '84186bf3-663d-48f1-9815-f2f95119a205',
										items: [],
									},
									{
										key: 'ce99380c-d6a2-4ffb-bdc4-59cc27a0735a',
										items: [],
									},
								],
								contentUdi: 'umb://element/95a7699fb5634c0c9eedd63648247ba3',
								settingsUdi: null,
								contentKey: '95a7699f-b563-4c0c-9eed-d63648247ba3',
								settingsKey: null,
							},
							{
								columnSpan: 6,
								rowSpan: 1,
								areas: [
									{
										key: 'c92e520a-1aa2-48cb-b7c9-45d586b6dc70',
										items: [],
									},
									{
										key: '01a9b7db-7360-45bb-999c-edc438473f96',
										items: [],
									},
								],
								contentUdi: 'umb://element/40b6c46b181f41baa168a8ec1decc55e',
								settingsUdi: null,
								contentKey: '40b6c46b-181f-41ba-a168-a8ec1decc55e',
								settingsKey: null,
							},
						],
					},
				},
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '39842212-489e-46ec-a63b-6eeff36c7156',
		createDate: '2023-04-17 14:03:51',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '61c6b912-8fe8-4e10-a07b-4f777b99489b',
			icon: 'icon-bulleted-list color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 12:30:41.8293415',
				culture: null,
				segment: null,
				name: 'Block List',
				createDate: '2023-04-17 14:03:51',
				updateDate: '2026-04-16 12:30:41.8293415',
				id: '39842212-489e-46ec-a63b-6eeff36c7156',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockListDefaultConfig',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is Element One',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							key: '858902e7-2ec3-400c-a641-98f5ac3578d4',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: '9dfcda46-88bf-4d69-bb2d-e94667051727',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
							culture: null,
							segment: null,
						},
						{
							contentKey: '858902e7-2ec3-400c-a641-98f5ac3578d4',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: 'umb://element/a08c8c6a8da246d087b7536985985b24',
								settingsUdi: null,
								contentKey: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
								settingsKey: null,
							},
							{
								contentUdi: 'umb://element/858902e72ec3400ca64198f5ac3578d4',
								settingsUdi: null,
								contentKey: '858902e7-2ec3-400c-a641-98f5ac3578d4',
								settingsKey: null,
							},
						],
					},
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockListMinAndMax',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: '1c648b5e-24d2-4ad2-831b-75007040de91',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is also Element One',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							key: '1b1f9f70-1bdb-4fda-97ca-da5ebb326616',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: '3702fd21-cad5-4eac-aa28-de44bf5a6246',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: '1c648b5e-24d2-4ad2-831b-75007040de91',
							culture: null,
							segment: null,
						},
						{
							contentKey: '1b1f9f70-1bdb-4fda-97ca-da5ebb326616',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: 'umb://element/1c648b5e24d24ad2831b75007040de91',
								settingsUdi: null,
								contentKey: '1c648b5e-24d2-4ad2-831b-75007040de91',
								settingsKey: null,
							},
							{
								contentUdi: 'umb://element/1b1f9f701bdb4fda97cada5ebb326616',
								settingsUdi: null,
								contentKey: '1b1f9f70-1bdb-4fda-97ca-da5ebb326616',
								settingsKey: null,
							},
						],
					},
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockListSingleTypeOnly',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: 'ee35ca6f-3ff3-4845-9766-f7c12508b32a',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is Element One',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'ee35ca6f-3ff3-4845-9766-f7c12508b32a',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: 'ee35ca6f-3ff3-4845-9766-f7c12508b32a',
								settingsKey: null,
							},
						],
					},
				},
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'd98b0eaf-8a5d-4644-a2cc-861f94e56df1',
		createDate: '2026-04-16 11:10:14.571705',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '9309d592-ebc7-4f72-a1bd-ebdabca4c643',
			icon: 'icon-shape-square color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:36.9975709',
				culture: null,
				segment: null,
				name: 'Block Single',
				createDate: '2026-04-16 11:10:14.571705',
				updateDate: '2026-04-16 11:10:36.9975709',
				id: 'd98b0eaf-8a5d-4644-a2cc-861f94e56df1',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.SingleBlock',
				alias: 'blockSingleDefaultConfig',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: 'e5a17c47-ace3-4775-b10d-f74664fb0294',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is Element One',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'e5a17c47-ace3-4775-b10d-f74664fb0294',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.SingleBlock': [
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: 'e5a17c47-ace3-4775-b10d-f74664fb0294',
								settingsKey: null,
							},
						],
					},
				},
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '9dfcda46-88bf-4d69-bb2d-e94667051727',
		createDate: '2023-02-27 08:32:56',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'b85bb884-ed5e-4f0b-8b10-8067090e8ada',
			icon: 'icon-bulleted-list color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0016192',
				culture: null,
				segment: null,
				name: 'Checkbox List',
				createDate: '2023-02-27 08:32:56',
				updateDate: '2026-04-16 11:10:37.0016192',
				id: '9dfcda46-88bf-4d69-bb2d-e94667051727',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.CheckBoxList',
				alias: 'checkboxList',
				culture: null,
				segment: null,
				value: ['One', 'Three', 'Five'],
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef',
		createDate: '2023-02-20 16:20:02',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '015bc281-7410-40e2-81b5-b8f7c963bd61',
			icon: 'icon-colorpicker color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.005186',
				culture: null,
				segment: null,
				name: 'Color Picker',
				createDate: '2023-02-20 16:20:02',
				updateDate: '2026-04-16 11:10:37.005186',
				id: '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.ColorPicker',
				alias: 'colorPickerNoLabels',
				culture: null,
				segment: null,
				value: { value: 'cc0000', label: 'cc0000' },
			},
			{
				editorAlias: 'Umbraco.ColorPicker',
				alias: 'colorPickerLabels',
				culture: null,
				segment: null,
				value: { value: 'cc0000', label: 'Red' },
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '58e300ad-868c-4a84-9915-2aef20ea681c',
		createDate: '2023-02-20 16:20:08',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '13c10f78-bf14-411d-9444-751e4bd1b178',
			icon: 'icon-autofill color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0088468',
				culture: null,
				segment: null,
				name: 'Content Picker',
				createDate: '2023-02-20 16:20:08',
				updateDate: '2026-04-16 11:10:37.0088468',
				id: '58e300ad-868c-4a84-9915-2aef20ea681c',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerDefaultConfig',
				culture: null,
				segment: null,
				value: 'db2a48d5-5883-465f-b1d7-e012af2f16d0',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerIgnoreUserStartNodes',
				culture: null,
				segment: null,
				value: '9394af8f-d306-4778-9f03-2431eb8f5b6b',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerShowOpenButton',
				culture: null,
				segment: null,
				value: 'a7823036-0486-44f5-af33-deb6780e07e6',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerStartNode',
				culture: null,
				segment: null,
				value: 'c680be85-0bb7-4429-9d4a-73ffb83e427b',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '0865b2ab-ad7c-48d4-a8c6-608986a0e942',
		createDate: '2023-02-20 16:20:15',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '41f34bb7-fd63-442f-8dcb-142df4246310',
			icon: 'icon-time color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0133238',
				culture: null,
				segment: null,
				name: 'DateTime Picker',
				createDate: '2023-02-20 16:20:15',
				updateDate: '2026-04-16 11:10:37.0133238',
				id: '0865b2ab-ad7c-48d4-a8c6-608986a0e942',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.DateTime',
				alias: 'dateTimePickerDateFormat',
				culture: null,
				segment: null,
				value: '2023-02-01 00:00:00',
			},
			{
				editorAlias: 'Umbraco.DateTime',
				alias: 'dateTimePickerDatePlusTimeFormat',
				culture: null,
				segment: null,
				value: '2023-02-01 12:34:00',
			},
			{
				editorAlias: 'Umbraco.DateTime',
				alias: 'dateTimePickerOffsetTime',
				culture: null,
				segment: null,
				value: '2023-02-01 01:23:00',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '4babad2f-967a-49e1-9f92-407e95ff9df9',
		createDate: '2023-02-20 16:20:20',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '9cff8f66-0e13-4617-ab9b-9f845ecc5e24',
			icon: 'icon-autofill color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0176169',
				culture: null,
				segment: null,
				name: 'Decimal',
				createDate: '2023-02-20 16:20:20',
				updateDate: '2026-04-16 11:10:37.0176169',
				id: '4babad2f-967a-49e1-9f92-407e95ff9df9',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.Decimal',
				alias: 'decimalDefaultConfig',
				culture: null,
				segment: null,
				value: '12.345',
			},
			{
				editorAlias: 'Umbraco.Decimal',
				alias: 'decimalFullyConfigured',
				culture: null,
				segment: null,
				value: '13.5',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'db2a48d5-5883-465f-b1d7-e012af2f16d0',
		createDate: '2023-02-20 16:20:24',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '0180d16d-6a87-4631-9802-4e1b1f180bd4',
			icon: 'icon-indent color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0253179',
				culture: null,
				segment: null,
				name: 'Dropdown',
				createDate: '2023-02-20 16:20:24',
				updateDate: '2026-04-16 11:10:37.0253179',
				id: 'db2a48d5-5883-465f-b1d7-e012af2f16d0',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.DropDown.Flexible',
				alias: 'dropdownMultiValue',
				culture: null,
				segment: null,
				value: '["One","Three"]',
			},
			{
				editorAlias: 'Umbraco.DropDown.Flexible',
				alias: 'dropdownSingleValue',
				culture: null,
				segment: null,
				value: '["Two"]',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '119f5ef0-31cf-4d59-9c98-2f3cbe2fa8df',
		createDate: '2023-02-20 16:20:30',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'fb88c3ab-40ee-4822-a63e-0edd97ad13f8',
			icon: 'icon-message color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0287162',
				culture: null,
				segment: null,
				name: 'Email Address',
				createDate: '2023-02-20 16:20:30',
				updateDate: '2026-04-16 11:10:37.0287162',
				id: '119f5ef0-31cf-4d59-9c98-2f3cbe2fa8df',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.EmailAddress',
				alias: 'emailAddress',
				culture: null,
				segment: null,
				value: 'noreply@umbraco.com',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '2329915b-fb6b-4c2f-9179-8c16ba125cea',
		createDate: '2023-02-20 16:20:39',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '8856d507-76e0-47c7-8564-56467e717053',
			icon: 'icon-colorpicker color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0319518',
				culture: null,
				segment: null,
				name: 'Eye Dropper Color Picker',
				createDate: '2023-02-20 16:20:39',
				updateDate: '2026-04-16 11:10:37.0319518',
				id: '2329915b-fb6b-4c2f-9179-8c16ba125cea',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerDefaultConfig',
				culture: null,
				segment: null,
				value: '#d82525',
			},
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerAlpha',
				culture: null,
				segment: null,
				value: 'rgba(106, 185, 66, 0.255)',
			},
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerPalette',
				culture: null,
				segment: null,
				value: '#2986cc',
			},
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerFullyConfigured',
				culture: null,
				segment: null,
				value: 'rgba(194, 123, 160, 0.517)',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'aa92afd0-8a54-4864-887c-7b36daee7e6c',
		createDate: '2023-05-22 13:09:31',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '11b48beb-3fd0-4b72-800e-364f6e833dc7',
			icon: 'icon-download-alt color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0355009',
				culture: null,
				segment: null,
				name: 'File Upload',
				createDate: '2023-05-22 13:09:31',
				updateDate: '2026-04-16 11:10:37.0355009',
				id: 'aa92afd0-8a54-4864-887c-7b36daee7e6c',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.UploadField',
				alias: 'fileUploadDefaultConfig',
				culture: null,
				segment: null,
				value: '/media/mhqpjciq/238-anticipate.png',
			},
			{
				editorAlias: 'Umbraco.UploadField',
				alias: 'fileUploadSpecificFileTypes',
				culture: null,
				segment: null,
				value: '/media/v21hywos/244-clutch-save.png',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '3702fd21-cad5-4eac-aa28-de44bf5a6246',
		createDate: '2023-03-03 12:47:12',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '2a773487-9de7-403c-9207-54f4ace7f215',
			icon: 'icon-crop color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0387687',
				culture: null,
				segment: null,
				name: 'Image Cropper',
				createDate: '2023-03-03 12:47:12',
				updateDate: '2026-04-16 11:10:37.0387687',
				id: '3702fd21-cad5-4eac-aa28-de44bf5a6246',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'imageCropperWithoutCrops',
				culture: null,
				segment: null,
				value: {
					src: '/umbraco/backoffice/assets/umb-pattern-pink.png',
				},
			},
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'imageCropperWithCrops',
				culture: null,
				segment: null,
				value: {
					src: '/umbraco/backoffice/assets/umb-pattern-pink.png',
					focalPoint: {
						left: 0.7736573381473065,
						top: 0.6536672841836085,
					},
					crops: [
						{
							alias: 'two',
							coordinates: {
								x1: 0.3658886079650509,
								y1: 0.22162740899357605,
								x2: 0.370014318516034,
								y2: 0.3822269807280513,
							},
						},
						{
							alias: 'three',
							coordinates: {
								x1: 0,
								y1: 0,
								x2: 0.7858576342156423,
								y2: 0.6787864513234635,
							},
						},
					],
				},
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '7bf4865b-de55-4f85-bd2c-9cb8e6e482c3',
		createDate: '2023-02-27 08:39:27',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '25dd3762-cfdd-43cd-b0a5-8f094f8a7fd2',
			icon: 'icon-readonly color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0419725',
				culture: null,
				segment: null,
				name: 'Label',
				createDate: '2023-02-27 08:39:27',
				updateDate: '2026-04-16 11:10:37.0419725',
				id: '7bf4865b-de55-4f85-bd2c-9cb8e6e482c3',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '9394af8f-d306-4778-9f03-2431eb8f5b6b',
		createDate: '2023-02-20 16:20:45',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '99431793-6f52-48c7-af53-6bf04668aca2',
			icon: 'icon-code color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0450044',
				culture: null,
				segment: null,
				name: 'Markdown Editor',
				createDate: '2023-02-20 16:20:45',
				updateDate: '2026-04-16 11:10:37.0450044',
				id: '9394af8f-d306-4778-9f03-2431eb8f5b6b',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MarkdownEditor',
				alias: 'markdownEditorDefaultConfig',
				culture: null,
				segment: null,
				value: 'Hello, Markdown Editor',
			},
			{
				editorAlias: 'Umbraco.MarkdownEditor',
				alias: 'markdownEditorFullyConfigured',
				culture: null,
				segment: null,
				value:
					'*This* is the _default_ value!\nSuper duper default value.\n\n- List item one\n- List item two\n\n## HEADING!\n\nMore text',
			},
			{
				editorAlias: 'Umbraco.MarkdownEditor',
				alias: 'markdownEditorDefaultValue',
				culture: null,
				segment: null,
				value: '*This* is the _default_ value!\n\n- List item one\n- List item two',
			},
			{
				editorAlias: 'Umbraco.MarkdownEditor',
				alias: 'markdownEditorLargeOverlaySize',
				culture: null,
				segment: null,
				value: 'Not sure what the overlay is used for..?',
			},
			{
				editorAlias: 'Umbraco.MarkdownEditor',
				alias: 'markdownEditorPreview',
				culture: null,
				segment: null,
				value: 'Gimme all your preview, all your text and paragraphs too!',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '15b092f0-66b5-40e5-aa1b-25b71b2bd81a',
		createDate: '2023-02-20 16:20:55',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '7b61b708-aa42-4978-a86c-f20fd4749a58',
			icon: 'icon-umb-media color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0487086',
				culture: null,
				segment: null,
				name: 'Media Picker',
				createDate: '2023-02-20 16:20:55',
				updateDate: '2026-04-16 11:10:37.0487086',
				id: '15b092f0-66b5-40e5-aa1b-25b71b2bd81a',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerDefaultConfig',
				culture: null,
				segment: null,
				value: [
					{
						key: '399fa24d-8d49-43cc-904c-cc0e45cab5ad',
						mediaKey: '76c02ec8-6a82-4c47-95da-56f6628b58fb',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerAcceptedTypes',
				culture: null,
				segment: null,
				value: [
					{
						key: '25244956-8944-4ed3-84af-3a4a1a8cac70',
						mediaKey: 'ee9c1205-4121-4610-b0b3-a522dfd3461d',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerAmount',
				culture: null,
				segment: null,
				value: [
					{
						key: '2950c6ee-05ea-4e39-9d86-d39c95c210b4',
						mediaKey: 'b44956af-620a-4e17-bbce-3987446fb2f1',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
					{
						key: 'b98a2107-3201-4bef-99a0-d52bb14f0afa',
						mediaKey: 'ee9c1205-4121-4610-b0b3-a522dfd3461d',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerCrops',
				culture: null,
				segment: null,
				value: [
					{
						key: '7e21fd4b-b667-4616-b114-797f06e0c064',
						mediaKey: 'ee9c1205-4121-4610-b0b3-a522dfd3461d',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerFocalPoint',
				culture: null,
				segment: null,
				value: [
					{
						key: 'f6bdc277-0f37-4ecd-aaab-234d23aaf479',
						mediaKey: '76c02ec8-6a82-4c47-95da-56f6628b58fb',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: {
							left: 0.292,
							top: 0.7642514616309971,
						},
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerIgnoreUserStartNodes',
				culture: null,
				segment: null,
				value: [
					{
						key: '96fd3a7f-9702-4974-bca8-7a093cbe3aa1',
						mediaKey: 'ee9c1205-4121-4610-b0b3-a522dfd3461d',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerPickMultipleItems',
				culture: null,
				segment: null,
				value: [
					{
						key: '419a89d7-ec47-40b8-b81f-a3bc823e19ce',
						mediaKey: 'b44956af-620a-4e17-bbce-3987446fb2f1',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
					{
						key: 'ac063d60-dd4e-446f-b6b7-1098c2aa8054',
						mediaKey: 'f06adb91-8cdd-408d-83dd-f7b833fc393c',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerPickMultipleItemsWithAmount',
				culture: null,
				segment: null,
				value: [
					{
						key: '44b4c537-6748-4776-bbd0-59c46fc2b5e5',
						mediaKey: 'f06adb91-8cdd-408d-83dd-f7b833fc393c',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
					{
						key: '5e16dfdd-3c44-4113-aedd-d255318c16dd',
						mediaKey: '76c02ec8-6a82-4c47-95da-56f6628b58fb',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
					{
						key: 'c1160266-8f15-4deb-8ca6-6044331920ba',
						mediaKey: 'ee9c1205-4121-4610-b0b3-a522dfd3461d',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerStartNode',
				culture: null,
				segment: null,
				value: [
					{
						key: '7655dcde-f1af-436f-abd0-515f116f01be',
						mediaKey: 'b44956af-620a-4e17-bbce-3987446fb2f1',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerFullyConfigured',
				culture: null,
				segment: null,
				value: [
					{
						key: 'ff80f629-6d88-42dc-8cf9-486ff7c4dc5b',
						mediaKey: 'f06adb91-8cdd-408d-83dd-f7b833fc393c',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
					{
						key: '8862d180-8fef-4aea-a8fe-42f7c0838384',
						mediaKey: 'a0651d98-14a9-4d92-8133-36f59b248d31',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
				],
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '4e02a6bf-5ab6-4b55-8f06-c6d24e892f8c',
		createDate: '2023-02-20 16:21:02',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'f7c73e80-e8f4-4ef6-a710-168d89991c7d',
			icon: 'icon-users-alt color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0530249',
				culture: null,
				segment: null,
				name: 'Member Group Picker',
				createDate: '2023-02-20 16:21:02',
				updateDate: '2026-04-16 11:10:37.0530249',
				id: '4e02a6bf-5ab6-4b55-8f06-c6d24e892f8c',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MemberGroupPicker',
				alias: 'memberGroupPicker',
				culture: null,
				segment: null,
				value: '4bff0fe9-6cf4-47cd-a87e-cd4a3a860c86,015dd839-aace-4372-8238-5ec353c3a4d7',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'a7823036-0486-44f5-af33-deb6780e07e6',
		createDate: '2023-02-20 16:21:11',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '48a02560-7ce9-4be4-96e7-e4041cc19622',
			icon: 'icon-user color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0560881',
				culture: null,
				segment: null,
				name: 'Member Picker',
				createDate: '2023-02-20 16:21:11',
				updateDate: '2026-04-16 11:10:37.0560881',
				id: 'a7823036-0486-44f5-af33-deb6780e07e6',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MemberPicker',
				alias: 'memberPicker',
				culture: null,
				segment: null,
				value: 'e93b2557-5fcb-4495-bbb3-9f5fd87055a8',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '80954b94-1d32-4edd-9c01-105561a7415d',
		createDate: '2023-02-20 16:22:41',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '727b819b-af42-443f-a752-c4c5cfd69313',
			icon: 'icon-link color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0591605',
				culture: null,
				segment: null,
				name: 'Multi URL Picker',
				createDate: '2023-02-20 16:22:41',
				updateDate: '2026-04-16 11:10:37.0591605',
				id: '80954b94-1d32-4edd-9c01-105561a7415d',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MultiUrlPicker',
				alias: 'multiUrlPickerDefaultConfig',
				culture: null,
				segment: null,
				value: [
					{
						name: 'Color Picker',
						target: '_blank',
						type: 'document',
						unique: '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef',
					},
					{
						name: 'Decimal',
						type: 'document',
						unique: '4babad2f-967a-49e1-9f92-407e95ff9df9',
					},
					{
						name: 'Umbraco Dot Com',
						type: 'external',
						url: 'https://umbraco.com',
					},
				],
			},
			{
				editorAlias: 'Umbraco.MultiUrlPicker',
				alias: 'multiUrlPickerFullyConfigured',
				culture: null,
				segment: null,
				value: [
					{
						name: 'Eye Dropper Color Picker',
						type: 'document',
						unique: '2329915b-fb6b-4c2f-9179-8c16ba125cea',
					},
					{
						name: 'Placeholder 2255924',
						type: 'media',
						unique: 'a0651d98-14a9-4d92-8133-6f59b248d31',
					},
				],
			},
			{
				editorAlias: 'Umbraco.MultiUrlPicker',
				alias: 'multiUrlPickerHideAnchorQueryString',
				culture: null,
				segment: null,
				value: [
					{
						name: 'Content Picker',
						type: 'document',
						unique: '58e300ad-868c-4a84-9915-2aef20ea681c',
					},
				],
			},
			{
				editorAlias: 'Umbraco.MultiUrlPicker',
				alias: 'multiUrlPickerIgnoreUserStartNodes',
				culture: null,
				segment: null,
				value: [
					{
						name: 'Numeric',
						type: 'document',
						unique: 'c680be85-0bb7-4429-9d4a-73ffb83e427b',
					},
					{
						name: 'DateTime Picker',
						type: 'document',
						unique: '0865b2ab-ad7c-48d4-a8c6-608986a0e942',
					},
					{
						name: 'Placeholder 73640',
						type: 'media',
						unique: '76c02ec8-6a82-4c47-95da-56f6628b58fb',
					},
				],
			},
			{
				editorAlias: 'Umbraco.MultiUrlPicker',
				alias: 'multiUrlPickerLargeOverlaySize',
				culture: null,
				segment: null,
				value: [
					{
						name: 'Umbraco Dot Com',
						type: 'external',
						target: '_blank',
						url: 'https://umbraco.com',
					},
				],
			},
			{
				editorAlias: 'Umbraco.MultiUrlPicker',
				alias: 'multiUrlPickerMinAndMax',
				culture: null,
				segment: null,
				value: [
					{
						name: 'Decimal',
						type: 'document',
						unique: '4babad2f-967a-49e1-9f92-407e95ff9df9',
					},
					{
						name: 'Markdown Editor',
						type: 'document',
						unique: '9394af8f-d306-4778-9f03-2431eb8f5b6b',
					},
					{
						name: 'Placeholder 1435904',
						type: 'media',
						unique: 'b44956af-620a-4e17-bbce-3987446fb2f1',
					},
					{
						name: 'Umbraco Dot Com',
						type: 'external',
						target: '_blank',
						url: 'https://umbraco.com',
					},
					{
						name: 'Multiple Textstring',
						type: 'document',
						unique: '17149c1e-44a8-4882-a088-6a1d84e0e86a',
					},
				],
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'a3a37004-139f-4254-ba56-3ed381b3007c',
		createDate: '2023-02-20 16:22:48',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '7025ee6c-8d6c-4abd-8e32-2cab5fde6f90',
			icon: 'icon-page-add color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0628801',
				culture: null,
				segment: null,
				name: 'Multinode Treepicker',
				createDate: '2023-02-20 16:22:48',
				updateDate: '2026-04-16 11:10:37.0628801',
				id: 'a3a37004-139f-4254-ba56-3ed381b3007c',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerDefaultConfig',
				culture: null,
				segment: null,
				value: [
					{ type: 'document', unique: '9394af8f-d306-4778-9f03-2431eb8f5b6b' },
					{ type: 'document', unique: '15b092f0-66b5-40e5-aa1b-25b71b2bd81a' },
					{ type: 'document', unique: '4e02a6bf-5ab6-4b55-8f06-c6d24e892f8c' },
					{ type: 'document', unique: 'a7823036-0486-44f5-af33-deb6780e07e6' },
					{ type: 'document', unique: '80954b94-1d32-4edd-9c01-105561a7415d' },
					{ type: 'document', unique: 'a3a37004-139f-4254-ba56-3ed381b3007c' },
					{ type: 'document', unique: '17149c1e-44a8-4882-a088-6a1d84e0e86a' },
				],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerAllowedTypes',
				culture: null,
				segment: null,
				value: [{ type: 'document', unique: 'db79156b-3d5b-43d6-ab32-902dc423bec3' }],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerFullyConfigured',
				culture: null,
				segment: null,
				value: [
					{ type: 'document', unique: '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef' },
					{ type: 'document', unique: '58e300ad-868c-4a84-9915-2aef20ea681c' },
					{ type: 'document', unique: '0865b2ab-ad7c-48d4-a8c6-608986a0e942' },
					{ type: 'document', unique: '4babad2f-967a-49e1-9f92-407e95ff9df9' },
				],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMinAndMax',
				culture: null,
				segment: null,
				value: [
					{ type: 'document', unique: '4babad2f-967a-49e1-9f92-407e95ff9df9' },
					{ type: 'document', unique: 'db2a48d5-5883-465f-b1d7-e012af2f16d0' },
					{ type: 'document', unique: '119f5ef0-31cf-4d59-9c98-2f3cbe2fa8df' },
					{ type: 'document', unique: '2329915b-fb6b-4c2f-9179-8c16ba125cea' },
				],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerStartNode',
				culture: null,
				segment: null,
				value: [
					{ type: 'document', unique: '0865b2ab-ad7c-48d4-a8c6-608986a0e942' },
					{ type: 'document', unique: '4babad2f-967a-49e1-9f92-407e95ff9df9' },
				],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerXPathStartNode',
				culture: null,
				segment: null,
				value: [{ type: 'document', unique: '23b1bf0a-c56e-4b0c-a2a9-a83d0d9708ef' }],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMediaDefaultConfig',
				culture: null,
				segment: null,
				value: [
					{ type: 'media', unique: 'b44956af-620a-4e17-bbce-3987446fb2f1' },
					{ type: 'media', unique: 'f06adb91-8cdd-408d-83dd-f7b833fc393c' },
				],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMediaFullyConfigured',
				culture: null,
				segment: null,
				value: [{ type: 'media', unique: 'a0651d98-14a9-4d92-8133-336f59b248d31' }],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMembersDefaultConfig',
				culture: null,
				segment: null,
				value: [
					{ type: 'member', unique: 'e93b2557-5fcb-4495-bbb3-9f5fd87055a8' },
					{ type: 'member', unique: 'd74d2bd0-f55a-4a06-beb8-d8e931fc726b' },
				],
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMembersFullyConfigured',
				culture: null,
				segment: null,
				value: [{ type: 'member', unique: 'e93b2557-5fcb-4495-bbb3-9f5fd87055a8' }],
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '17149c1e-44a8-4882-a088-6a1d84e0e86a',
		createDate: '2023-02-20 16:22:56',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'cc827fc0-e385-494b-88f6-d4abb47b7081',
			icon: 'icon-ordered-list color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0670868',
				culture: null,
				segment: null,
				name: 'Multiple Textstring',
				createDate: '2023-02-20 16:22:56',
				updateDate: '2026-04-16 11:10:37.0670868',
				id: '17149c1e-44a8-4882-a088-6a1d84e0e86a',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringDefaultConfig',
				culture: null,
				segment: null,
				value: ['One', 'Two', 'Three'],
			},
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringFullyConfigured',
				culture: null,
				segment: null,
				value: ['One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight'],
			},
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringMax',
				culture: null,
				segment: null,
				value: ['One', 'Two', 'Three', 'Four', 'Five', 'Six'],
			},
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringMin',
				culture: null,
				segment: null,
				value: ['One', 'Two', 'Three', 'Four', 'Five'],
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'c680be85-0bb7-4429-9d4a-73ffb83e427b',
		createDate: '2023-02-20 16:23:03',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'f984a2dc-01c0-4974-a860-b41dfeacf2b5',
			icon: 'icon-autofill color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0705705',
				culture: null,
				segment: null,
				name: 'Numeric',
				createDate: '2023-02-20 16:23:03',
				updateDate: '2026-04-16 11:10:37.0705705',
				id: 'c680be85-0bb7-4429-9d4a-73ffb83e427b',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.Integer',
				alias: 'numericDefaultConfig',
				culture: null,
				segment: null,
				value: 123,
			},
			{
				editorAlias: 'Umbraco.Integer',
				alias: 'numericMinAndMax',
				culture: null,
				segment: null,
				value: 15,
			},
			{
				editorAlias: 'Umbraco.Integer',
				alias: 'numericStepSize',
				culture: null,
				segment: null,
				value: 24,
			},
			{
				editorAlias: 'Umbraco.Integer',
				alias: 'numericFullyConfigured',
				culture: null,
				segment: null,
				value: 52,
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'a51f23dc-4684-465c-8f7d-7c6bb07ff000',
		createDate: '2023-02-20 16:23:09',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '7b52f09a-3034-43d6-a83e-5f9fadfcc87d',
			icon: 'icon-target color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0740563',
				culture: null,
				segment: null,
				name: 'Radio Button List',
				createDate: '2023-02-20 16:23:09',
				updateDate: '2026-04-16 11:10:37.0740563',
				id: 'a51f23dc-4684-465c-8f7d-7c6bb07ff000',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.RadioButtonList',
				alias: 'radioButtonList',
				culture: null,
				segment: null,
				value: 'Two',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '464ca81d-30e0-4169-899a-0556303b878c',
		createDate: '2023-02-20 16:23:17',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'fd62fafc-9cfd-470a-a260-93af5d1ed641',
			icon: 'icon-browser-window color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0771127',
				culture: null,
				segment: null,
				name: 'Rich Text Editor',
				createDate: '2023-02-20 16:23:17',
				updateDate: '2026-04-16 11:10:37.0771127',
				id: '464ca81d-30e0-4169-899a-0556303b878c',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.RichText',
				alias: 'richTextEditorDefaultConfig',
				culture: null,
				segment: null,
				value: {
					markup: '<p>Default RTE content</p>\n<p>And a new line</p>\n<p>How about that!</p>',
					blocks: {
						contentData: [],
						settingsData: [],
						expose: [],
						layout: {},
					},
				},
			},
			{
				editorAlias: 'Umbraco.RichText',
				alias: 'richTextEditorDimensions',
				culture: null,
				segment: null,
				value: {
					markup:
						'<p>More RTE content</p>\n<p>Running out of <span style="text-decoration: underline;">ideas</span> here</p>\n<p> </p>',
					blocks: {
						contentData: [],
						settingsData: [],
						expose: [],
						layout: {},
					},
				},
			},
			{
				editorAlias: 'Umbraco.RichText',
				alias: 'richTextEditorFullyConfigured',
				culture: null,
				segment: null,
				value: {
					markup:
						'<p>Yikes so many buttons!</p><ul><li><p>Button</p></li><li><p>Button</p></li><li><p>More button!</p></li></ul><p><img data-udi="umb://media/f06adb918cdd408d83ddf7b833fc393c" src="" alt="" width="426" height="284" style="display: block; margin-left: auto; margin-right: auto;"></p><p>&nbsp;</p>',
					blocks: {
						contentData: [],
						settingsData: [],
						expose: [],
						layout: {},
					},
				},
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '8d291b82-6356-4e0d-b8dc-927c51bffe93',
		createDate: '2023-02-20 16:23:23',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '23c4c503-bcdf-46a5-9ff9-fb78d9dba4ae',
			icon: 'icon-navigation-horizontal color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0806808',
				culture: null,
				segment: null,
				name: 'Slider',
				createDate: '2023-02-20 16:23:23',
				updateDate: '2026-04-16 11:10:37.0806808',
				id: '8d291b82-6356-4e0d-b8dc-927c51bffe93',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderDefaultConfig',
				culture: null,
				segment: null,
				value: '41',
			},
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderInitialValue',
				culture: null,
				segment: null,
				value: '41',
			},
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderMinAndMax',
				culture: null,
				segment: null,
				value: '33',
			},
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderStepIncrements',
				culture: null,
				segment: null,
				value: '40',
			},
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderFullyConfigured',
				culture: null,
				segment: null,
				value: '39,53',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '2812b39b-b014-4ba1-9410-bb5b3f17ca04',
		createDate: '2023-02-27 08:34:05',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '6dcde803-d22e-4fcf-85a3-3a03be080d3a',
			icon: 'icon-tags color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0843752',
				culture: null,
				segment: null,
				name: 'Tags',
				createDate: '2023-02-27 08:34:05',
				updateDate: '2026-04-16 11:10:37.0843752',
				id: '2812b39b-b014-4ba1-9410-bb5b3f17ca04',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.Tags',
				alias: 'tagsDefaultGroupJSONStorage',
				culture: null,
				segment: null,
				value: ['DefaultOne', 'DefaultTwo', 'DefaultThree'],
			},
			{
				editorAlias: 'Umbraco.Tags',
				alias: 'tagsCustomGroupCSVStorage',
				culture: null,
				segment: null,
				value: 'CustomOne,CustomTwo,CustomThree',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '9d856f1f-c37c-4e93-aa71-447aeb8fc47b',
		createDate: '2023-02-20 16:23:29',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'af83a333-d5f9-4467-9013-9eaa8112a571',
			icon: 'icon-application-window-alt color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0876156',
				culture: null,
				segment: null,
				name: 'Textarea',
				createDate: '2023-02-20 16:23:29',
				updateDate: '2026-04-16 11:10:37.0876156',
				id: '9d856f1f-c37c-4e93-aa71-447aeb8fc47b',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextArea',
				alias: 'textareaDefaultConfig',
				culture: null,
				segment: null,
				value: 'Something default config.\nNewline.\nNewline again.',
			},
			{
				editorAlias: 'Umbraco.TextArea',
				alias: 'textareaFullyConfigured',
				culture: null,
				segment: null,
				value: 'Fully configured text area here.',
			},
			{
				editorAlias: 'Umbraco.TextArea',
				alias: 'textareaMaxChars',
				culture: null,
				segment: null,
				value: 'Something with max chars',
			},
			{
				editorAlias: 'Umbraco.TextArea',
				alias: 'textareaRows',
				culture: null,
				segment: null,
				value: 'Something with a lotta rows',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: 'df020cda-560b-42e4-9bba-3eed49ae0be6',
		createDate: '2023-02-26 15:39:37',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '6717ef28-57a2-4cb4-80fe-ddc7a76da5f4',
			icon: 'icon-autofill color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0910204',
				culture: null,
				segment: null,
				name: 'Textbox',
				createDate: '2023-02-26 15:39:37',
				updateDate: '2026-04-16 11:10:37.0910204',
				id: 'df020cda-560b-42e4-9bba-3eed49ae0be6',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'textboxDefaultConfig',
				culture: null,
				segment: null,
				value: 'The default config',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'textboxMaxChars',
				culture: null,
				segment: null,
				value: 'The config with max chars',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '57257cd6-8100-4fbf-a734-f4147ce30701',
		createDate: '2023-02-20 16:23:36',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '373eaceb-e41e-4dd2-ae3f-b73fd11cf182',
			icon: 'icon-checkbox color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.0943281',
				culture: null,
				segment: null,
				name: 'Toggle',
				createDate: '2023-02-20 16:23:36',
				updateDate: '2026-04-16 11:10:37.0943281',
				id: '57257cd6-8100-4fbf-a734-f4147ce30701',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TrueFalse',
				alias: 'toggleDefaultConfig',
				culture: null,
				segment: null,
				value: 0,
			},
			{
				editorAlias: 'Umbraco.TrueFalse',
				alias: 'toggleFullyConfigured',
				culture: null,
				segment: null,
				value: 1,
			},
			{
				editorAlias: 'Umbraco.TrueFalse',
				alias: 'toggleInitialState',
				culture: null,
				segment: null,
				value: 1,
			},
			{
				editorAlias: 'Umbraco.TrueFalse',
				alias: 'toggleLabels',
				culture: null,
				segment: null,
				value: 0,
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
			},
		],
		template: null,
		id: '06e4bd2f-98f7-48cc-85e9-2a4b8cd668e8',
		createDate: '2023-02-27 08:34:21',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'dc965257-84c2-4f27-b452-55e8b0f91a96',
			icon: 'icon-user color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-04-16 11:10:37.09819',
				culture: null,
				segment: null,
				name: 'User Picker',
				createDate: '2023-02-27 08:34:21',
				updateDate: '2026-04-16 11:10:37.09819',
				id: '06e4bd2f-98f7-48cc-85e9-2a4b8cd668e8',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.UserPicker',
				alias: 'userPicker',
				culture: null,
				segment: null,
				value: '1e70f841-c261-413b-abb2-2d68cdb96094',
			},
		],
		flags: [],
	},
];

export const data: Array<UmbMockDocumentModel> = rawData.map((doc) => ({
	...doc,
	variants: doc.variants.map((v) => ({
		...v,
		state: mapState(v.state),
	})),
}));
