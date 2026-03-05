import type { UmbMockDocumentModel } from '../../types/mock-data-set.types.js';
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
			icon: 'icon-home color-blue',
		},
		hasChildren: true,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-09-10 10:47:06',
				culture: 'en-US',
				segment: null,
				name: 'Homepage',
				createDate: '2023-02-20 15:09:43',
				updateDate: '2025-09-10 10:47:06',
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
				flags: [],
			},
			{
				state: 'Draft',
				publishDate: '2025-09-10 10:47:06',
				culture: 'da-dk',
				segment: null,
				name: 'Hjem',
				createDate: '2023-02-20 15:09:43',
				updateDate: '2025-09-10 10:47:06',
				id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},
	{
		ancestors: [],
		template: null,
		id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
		createDate: '2025-04-14 17:40:04',
		parent: null,
		documentType: {
			id: 'e36f0f5f-d7da-4ad0-a6eb-5bcbc3e80e16',
			icon: 'icon-flowerpot color-blue',
		},
		hasChildren: true,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-04-14 18:33:03',
				culture: null,
				segment: null,
				name: 'Dynamic Root Container',
				createDate: '2025-04-14 17:40:04',
				updateDate: '2025-04-14 18:33:03',
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},
	{
		ancestors: [],
		template: null,
		id: 'cd63170c-a200-42a1-9aa8-5253231be23a',
		createDate: '2025-05-13 16:23:19',
		parent: null,
		documentType: {
			id: '98b2e832-1910-43e5-9769-f25086294a73',
			icon: 'icon-gitbook color-blue',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-11-18 13:52:31.0602473',
				culture: null,
				segment: null,
				name: 'Block List Test',
				createDate: '2025-05-13 16:23:19',
				updateDate: '2025-11-18 13:52:31.0602473',
				id: 'cd63170c-a200-42a1-9aa8-5253231be23a',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: '511fef8a-5e2e-42b9-8312-0a6034881793',
							udi: null,
							key: '6c5dd5d0-1b3c-4e4d-8644-488528c565e9',
							values: [],
						},
						{
							contentTypeKey: '511fef8a-5e2e-42b9-8312-0a6034881793',
							udi: null,
							key: 'bb3e096b-6381-480f-adbc-4e236849cd53',
							values: [],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: '6c5dd5d0-1b3c-4e4d-8644-488528c565e9',
							culture: null,
							segment: null,
						},
						{
							contentKey: 'bb3e096b-6381-480f-adbc-4e236849cd53',
							culture: null,
							segment: null,
						},
					],
					Layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: '6c5dd5d0-1b3c-4e4d-8644-488528c565e9',
								settingsKey: null,
							},
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: 'bb3e096b-6381-480f-adbc-4e236849cd53',
								settingsKey: null,
							},
						],
					},
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList2',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: '511fef8a-5e2e-42b9-8312-0a6034881793',
							udi: null,
							key: 'fb882e38-c01d-4b11-af49-4e86db78f401',
							values: [],
						},
						{
							contentTypeKey: '511fef8a-5e2e-42b9-8312-0a6034881793',
							udi: null,
							key: '663b2a6e-c19a-4080-ad33-0944345a5077',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'textstring',
									value: 'Item 2',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'fb882e38-c01d-4b11-af49-4e86db78f401',
							culture: null,
							segment: null,
						},
						{
							contentKey: '663b2a6e-c19a-4080-ad33-0944345a5077',
							culture: null,
							segment: null,
						},
					],
					Layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: 'fb882e38-c01d-4b11-af49-4e86db78f401',
								settingsKey: null,
							},
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: '663b2a6e-c19a-4080-ad33-0944345a5077',
								settingsKey: null,
							},
						],
					},
				},
			},
			{
				editorAlias: 'Umbraco.BlockGrid',
				alias: 'blockGrid',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							udi: null,
							key: 'c5bc3a1d-6e9e-43e5-8b93-6d1614c743b2',
							values: [
								{
									editorAlias: 'Umbraco.RadioButtonList',
									culture: null,
									segment: null,
									alias: 'radioButtonList',
									value: 'Two',
								},
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'Item 1',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: '0299e033-5745-4020-8571-879b2096049a',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/73f02c6eda5e41d5bf0d37fba4c6ce5e',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'c5bc3a1d-6e9e-43e5-8b93-6d1614c743b2',
							culture: null,
							segment: null,
						},
						{
							contentKey: '0299e033-5745-4020-8571-879b2096049a',
							culture: null,
							segment: null,
						},
					],
					Layout: {
						'Umbraco.BlockGrid': [
							{
								columnSpan: 12,
								rowSpan: 1,
								areas: [],
								contentUdi: null,
								settingsUdi: null,
								contentKey: 'c5bc3a1d-6e9e-43e5-8b93-6d1614c743b2',
								settingsKey: null,
							},
							{
								columnSpan: 12,
								rowSpan: 1,
								areas: [],
								contentUdi: null,
								settingsUdi: null,
								contentKey: '0299e033-5745-4020-8571-879b2096049a',
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
		id: '6365b1cc-9529-439a-8889-be1a98639a84',
		createDate: '2025-02-05 12:13:00',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '5ece407b-ca9e-44fe-be4f-8c819c444cdd',
			icon: 'icon-app',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-01-26 19:33:23.5731025',
				culture: null,
				segment: null,
				name: 'Tiptap RTE æøå ${{7*7}}test page"><iframe src="https://evil.com"></iframe>',
				createDate: '2025-02-05 12:13:00',
				updateDate: '2026-01-26 19:33:23.5731025',
				id: '6365b1cc-9529-439a-8889-be1a98639a84',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.RichText',
				alias: 'tiptapRte',
				culture: null,
				segment: null,
				value: {
					markup:
						'<h2>Tiptap Rich Text Editor</h2><p>Recent <a target="" data-router-slot="disabled" href="/{localLink:39842212-489e-46ec-a63b-6eeff36c7156}" title="Block List" type="document">developments </a>in v15.3 and v15.4</p><p><img data-udi="umb://media/c784001ae3204adaa98a4a4506632441" src="" alt="nxnw_300x300.jpg" width="300" height="300"></p><p>Style select menu</p><p><umb-rte-block-inline data-content-key="498f42f0-a4f0-4918-b9c9-7f845583b8b6"></umb-rte-block-inline></p><p>Custom stylesheets</p><p><umb-rte-block-inline data-content-key="8e31afcc-1fc5-4c82-9d98-918dab30f31a"></umb-rte-block-inline></p><p>Font family menu</p><umb-rte-block data-content-key="708198c8-da12-4159-b0cc-1b1a5c3ceb17"></umb-rte-block><p>Font size menu</p><p>Anchors</p><p>Character map</p><p>Indent / Outdent</p><p>Text direction, right-to-left!</p><p>Text colour</p><p>Background colour</p><p>Table menu</p><p>Generic markup, allows for <code>&lt;div&gt;</code> and <code>&lt;span&gt;</code> tags</p><p>Global attributes, e.g. <span id="foo">IDs</span>, <span class="bar">class</span>, <span style="font-family: monospace;">style</span>, <span data-foo="bar">data-* attributes</span></p>',
					blocks: {
						contentData: [
							{
								contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
								udi: null,
								key: '498f42f0-a4f0-4918-b9c9-7f845583b8b6',
								values: [
									{
										editorAlias: 'Umbraco.TextBox',
										culture: null,
										segment: null,
										alias: 'title',
										value: 'Test',
									},
									{
										editorAlias: 'Umbraco.RadioButtonList',
										culture: null,
										segment: null,
										alias: 'radioButtonList',
										value: 'One',
									},
								],
							},
							{
								contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
								udi: null,
								key: '8e31afcc-1fc5-4c82-9d98-918dab30f31a',
								values: [
									{
										editorAlias: 'Umbraco.RadioButtonList',
										culture: null,
										segment: null,
										alias: 'radioButtonList',
										value: 'One',
									},
								],
							},
							{
								contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
								udi: null,
								key: '708198c8-da12-4159-b0cc-1b1a5c3ceb17',
								values: [],
							},
						],
						settingsData: [],
						expose: [
							{
								contentKey: '498f42f0-a4f0-4918-b9c9-7f845583b8b6',
								culture: null,
								segment: null,
							},
							{
								contentKey: 'be5d824e-4e6d-4028-b6d5-ae4fefcd33d4',
								culture: null,
								segment: null,
							},
							{
								contentKey: '6e561229-46a5-4dfa-b276-c3d012cb504f',
								culture: null,
								segment: null,
							},
							{
								contentKey: '8e31afcc-1fc5-4c82-9d98-918dab30f31a',
								culture: null,
								segment: null,
							},
							{
								contentKey: '708198c8-da12-4159-b0cc-1b1a5c3ceb17',
								culture: null,
								segment: null,
							},
						],
						Layout: {
							'Umbraco.RichText': [
								{
									contentUdi: null,
									settingsUdi: null,
									contentKey: '498f42f0-a4f0-4918-b9c9-7f845583b8b6',
									settingsKey: null,
								},
								{
									contentUdi: null,
									settingsUdi: null,
									contentKey: '8e31afcc-1fc5-4c82-9d98-918dab30f31a',
									settingsKey: null,
								},
								{
									contentUdi: null,
									settingsUdi: null,
									contentKey: '708198c8-da12-4159-b0cc-1b1a5c3ceb17',
									settingsKey: null,
								},
							],
						},
					},
				},
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
			},
		],
		template: null,
		id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
		createDate: '2025-04-14 17:46:52',
		parent: {
			id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
		},
		documentType: {
			id: '651c932b-232d-42b3-a52e-c1399fcff9bc',
			icon: 'icon-tags',
		},
		hasChildren: true,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-04-14 17:46:52',
				culture: null,
				segment: null,
				name: 'Categories',
				createDate: '2025-04-14 17:46:52',
				updateDate: '2025-04-14 17:46:52',
				id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
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
		id: '73f02c6e-da5e-41d5-bf0d-37fba4c6ce5e',
		createDate: '2025-02-13 15:15:14',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '442ba583-4725-4652-b19a-8aa2e6529e94',
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-01-22 12:45:30.0509181',
				culture: 'en-US',
				segment: null,
				name: 'Test Page',
				createDate: '2025-02-13 15:15:14',
				updateDate: '2026-01-22 12:45:30.0509181',
				id: '73f02c6e-da5e-41d5-bf0d-37fba4c6ce5e',
				flags: [],
			},
			{
				state: 'Published',
				publishDate: '2026-01-22 12:45:30.0509181',
				culture: 'da-dk',
				segment: null,
				name: 'Testside',
				createDate: '2025-02-13 15:15:14',
				updateDate: '2026-01-22 12:45:30.0509181',
				id: '73f02c6e-da5e-41d5-bf0d-37fba4c6ce5e',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.RichText',
				alias: 'testEditor',
				culture: null,
				segment: null,
				value: {
					markup:
						'<umb-rte-block data-content-key="1d98cc60-48e6-40d9-a886-b3a74955b4d5"></umb-rte-block><p></p><h1>Hello world</h1><figure figcaption=""><p><img src="?rmode=max&amp;width=500&amp;height=500&amp;hmac=98fa916544191ff181d550bd5b5559f9b092f29b178b824553b9fa52e5c563e6" alt="nxnw_300x300.jpg" width="500" height="500" data-udi="umb://media/c784001ae3204adaa98a4a4506632441"></p><figcaption>Lee Kelleher</figcaption></figure><p></p>',
					blocks: {
						contentData: [
							{
								contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
								udi: null,
								key: '1d98cc60-48e6-40d9-a886-b3a74955b4d5',
								values: [
									{
										editorAlias: 'Umbraco.TextBox',
										culture: null,
										segment: null,
										alias: 'title',
										value: 'Test',
									},
									{
										editorAlias: 'Umbraco.RadioButtonList',
										culture: null,
										segment: null,
										alias: 'radioButtonList',
										value: 'One',
									},
								],
							},
						],
						settingsData: [],
						expose: [
							{
								contentKey: '1d98cc60-48e6-40d9-a886-b3a74955b4d5',
								culture: null,
								segment: null,
							},
						],
						Layout: {
							'Umbraco.RichText': [
								{
									contentUdi: null,
									settingsUdi: null,
									contentKey: '1d98cc60-48e6-40d9-a886-b3a74955b4d5',
									settingsKey: null,
								},
							],
						},
					},
				},
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'headline',
				culture: null,
				segment: null,
				value: 'Draft 2',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'variantTextstring',
				culture: 'en-US',
				segment: null,
				value: 'umb://document/39842212489e46eca63b6eeff36c7156',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'variantTextstring',
				culture: 'da-dk',
				segment: null,
				value: 'umb://document/39842212489e46eca63b6eeff36c7156',
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
			},
		],
		template: null,
		id: '3189a39c-98b5-4be2-8d99-11d91dc7a572',
		createDate: '2025-04-14 18:38:25',
		parent: {
			id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
		},
		documentType: {
			id: '2657b3a0-3400-4fcd-b7b9-58d278461e89',
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-10-01 18:29:38.9600331',
				culture: null,
				segment: null,
				name: 'Dynamic Root Page',
				createDate: '2025-04-14 18:38:25',
				updateDate: '2025-10-01 18:29:38.9600331',
				id: '3189a39c-98b5-4be2-8d99-11d91dc7a572',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'categories',
				culture: null,
				segment: null,
				value: 'umb://document/2bbefe25f90e4b09b10d2fa9fc6580b7',
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
				publishDate: '2026-01-22 12:45:30.062553',
				culture: null,
				segment: null,
				name: 'Block Grid',
				createDate: '2023-04-19 09:00:36',
				updateDate: '2026-01-22 12:45:30.062553',
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
							udi: null,
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
							udi: null,
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
							udi: null,
							key: 'b3cf7aab-b1a3-4522-a089-2ba533ef29c0',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/23b1bf0ac56e4b0ca2a9a83d0d9708ef',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: '98c88784-0813-4149-9ce4-e0561db6dbb6',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/db79156b3d5b43d6ab32902dc423bec3',
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
						{
							contentKey: '98c88784-0813-4149-9ce4-e0561db6dbb6',
							culture: null,
							segment: null,
						},
					],
					Layout: {
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
								columnSpan: 12,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/4b97e38d176d4ffda51fff4e461f7b5d',
								settingsUdi: null,
								contentKey: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
								settingsKey: null,
							},
							{
								columnSpan: 12,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/b3cf7aabb1a34522a0892ba533ef29c0',
								settingsUdi: null,
								contentKey: 'b3cf7aab-b1a3-4522-a089-2ba533ef29c0',
								settingsKey: null,
							},
							{
								columnSpan: 12,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/98c88784081341499ce4e0561db6dbb6',
								settingsUdi: null,
								contentKey: '98c88784-0813-4149-9ce4-e0561db6dbb6',
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
							udi: null,
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
							udi: null,
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
							udi: null,
							key: '1d2c53ec-fa0a-46b7-bde6-79e9ada92a6d',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/db2a48d55883465fb1d7e012af2f16d0',
								},
							],
						},
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							udi: null,
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
							udi: null,
							key: '40b6c46b-181f-41ba-a168-a8ec1decc55e',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/119f5ef031cf4d599c982f3cbe2fa8df',
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
					Layout: {
						'Umbraco.BlockGrid': [
							{
								columnSpan: 12,
								rowSpan: 2,
								areas: [
									{
										key: '84186bf3-663d-48f1-9815-f2f95119a205',
										items: [
											{
												columnSpan: 6,
												rowSpan: 2,
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
													{
														key: '20a04dbe-f38d-4c6b-acf5-b38d2918d670',
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
								columnSpan: 12,
								rowSpan: 2,
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
								columnSpan: 12,
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
									{
										key: '20a04dbe-f38d-4c6b-acf5-b38d2918d670',
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
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
			},
		],
		template: null,
		id: '95237624-1f48-4830-a70c-ab771e310182',
		createDate: '2025-05-08 15:32:01',
		parent: {
			id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
		},
		documentType: {
			id: '2657b3a0-3400-4fcd-b7b9-58d278461e89',
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-10-01 18:29:44.0273559',
				culture: null,
				segment: null,
				name: 'Dynamic Root Page 2',
				createDate: '2025-05-08 15:32:01',
				updateDate: '2025-10-01 18:29:44.0273559',
				id: '95237624-1f48-4830-a70c-ab771e310182',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'categories',
				culture: null,
				segment: null,
				value: 'umb://document/1bfa2bde52824925bf2b33cd4c797a94',
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
				publishDate: '2026-01-22 12:45:30.0706187',
				culture: 'en-US',
				segment: null,
				name: 'Block List',
				createDate: '2023-04-17 14:03:51',
				updateDate: '2026-01-22 12:45:30.0706187',
				id: '39842212-489e-46ec-a63b-6eeff36c7156',
				flags: [],
			},
			{
				state: 'Published',
				publishDate: '2026-01-22 12:45:30.0706187',
				culture: 'da-dk',
				segment: null,
				name: 'Blokliste',
				createDate: '2023-04-17 14:03:51',
				updateDate: '2026-01-22 12:45:30.0706187',
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
							udi: null,
							key: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is Element One',
								},
								{
									editorAlias: 'Umbraco.RadioButtonList',
									culture: null,
									segment: null,
									alias: 'radioButtonList',
									value: 'One',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: '858902e7-2ec3-400c-a641-98f5ac3578d4',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/73f02c6eda5e41d5bf0d37fba4c6ce5e',
								},
								{
									editorAlias: 'Umbraco.MultiUrlPicker',
									culture: null,
									segment: null,
									alias: 'multiUrlPicker',
									value:
										'[{"name":"nxnw_300x300.jpg","target":null,"unique":null,"type":null,"udi":"umb://media/c784001ae3204adaa98a4a4506632441","url":null,"queryString":null},{"name":"Document Picker","target":null,"unique":null,"type":null,"udi":"umb://document/58e300ad868c4a8499152aef20ea681c","url":null,"queryString":null},{"name":"umbraco.com","target":null,"unique":null,"type":null,"udi":null,"url":"https://umbraco.com","queryString":null}]',
								},
								{
									editorAlias: 'Umbraco.MediaPicker3',
									culture: null,
									segment: null,
									alias: 'mediaPicker',
									value:
										'[{"key":"3289ef13-7801-4460-922c-0e5ec64abad8","mediaKey":"c784001a-e320-4ada-a98a-4a4506632441","mediaTypeAlias":"Image","crops":[],"focalPoint":null}]',
								},
								{
									editorAlias: 'Umbraco.RichText',
									culture: null,
									segment: null,
									alias: 'tiptapRte',
									value:
										'{"markup":"\\u003Cp\\u003E\\u003Cstrong\\u003EHello\\u003C/strong\\u003E \\u003Cem\\u003Eworld\\u003C/em\\u003E\\u003C/p\\u003E","blocks":null}',
								},
							],
						},
					],
					settingsData: [
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: '937af142-28d7-4a33-81d8-36232ab7f76d',
							values: [],
						},
					],
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
					Layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: 'umb://element/a08c8c6a8da246d087b7536985985b24',
								settingsUdi: 'umb://element/937af14228d74a3381d836232ab7f76d',
								contentKey: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
								settingsKey: '937af142-28d7-4a33-81d8-36232ab7f76d',
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
							udi: null,
							key: '1c648b5e-24d2-4ad2-831b-75007040de91',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is also Element One',
								},
								{
									editorAlias: 'Umbraco.RadioButtonList',
									culture: null,
									segment: null,
									alias: 'radioButtonList',
									value: 'One',
								},
							],
						},
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: 'fca96857-5899-4056-bb67-9467b497373f',
							values: [],
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
							contentKey: 'fca96857-5899-4056-bb67-9467b497373f',
							culture: null,
							segment: null,
						},
					],
					Layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: 'umb://element/1c648b5e24d24ad2831b75007040de91',
								settingsUdi: null,
								contentKey: '1c648b5e-24d2-4ad2-831b-75007040de91',
								settingsKey: null,
							},
							{
								contentUdi: 'umb://element/fca9685758994056bb679467b497373f',
								settingsUdi: null,
								contentKey: 'fca96857-5899-4056-bb67-9467b497373f',
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
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: '4d8ac611-0442-447a-9990-77243affa1df',
							values: [],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: '4d8ac611-0442-447a-9990-77243affa1df',
							culture: null,
							segment: null,
						},
					],
					Layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: null,
								settingsUdi: null,
								contentKey: '4d8ac611-0442-447a-9990-77243affa1df',
								settingsKey: null,
							},
						],
					},
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockListInlineMode',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
							udi: null,
							key: 'addd9172-f924-406b-8704-2d367a6918df',
							values: [
								{
									editorAlias: 'Umbraco.ContentPicker',
									culture: null,
									segment: null,
									alias: 'link',
									value: 'umb://document/db79156b3d5b43d6ab32902dc423bec3',
								},
								{
									editorAlias: 'Umbraco.MultiUrlPicker',
									culture: null,
									segment: null,
									alias: 'multiUrlPicker',
									value:
										'[{"name":null,"target":null,"unique":null,"type":null,"udi":"umb://media/25ff719c4e914c28a423c8c84cf8dece","url":null,"queryString":null}]',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'addd9172-f924-406b-8704-2d367a6918df',
							culture: null,
							segment: null,
						},
					],
					Layout: {
						'Umbraco.BlockList': [
							{
								contentUdi: 'umb://element/addd9172f924406b87042d367a6918df',
								settingsUdi: null,
								contentKey: 'addd9172-f924-406b-8704-2d367a6918df',
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
				publishDate: '2026-01-22 12:45:30.0801284',
				culture: null,
				segment: null,
				name: 'Checkbox List',
				createDate: '2023-02-27 08:32:56',
				updateDate: '2026-01-22 12:45:30.0801284',
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
				value: ['One', 'Five', 'Three'],
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
		id: '511898a2-4e17-4e63-a611-243821950843',
		createDate: '2025-08-05 15:59:21',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: '8f3cd603-85af-4784-bfb7-cf966cdd6ac7',
			icon: 'icon-brackets color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-01-22 12:45:30.0877511',
				culture: null,
				segment: null,
				name: 'Code Editor',
				createDate: '2025-08-05 15:59:21',
				updateDate: '2026-01-22 12:45:30.0877511',
				id: '511898a2-4e17-4e63-a611-243821950843',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.Plain.String',
				alias: 'codeEditorDefaultConfig',
				culture: null,
				segment: null,
				value:
					'class Person {\r\n    constructor(name) {\r\n        this.name = name;\r\n    }\r\n}\r\n\r\nclass Student extends Person {\r\n    constructor(name, id) {\r\n        super(name);\r\n        this.id = id;\r\n    }\r\n}\r\n\r\nconst bob = new Student("Robert", 12345);\r\nconsole.log(bob.name); // Robert',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.0940108',
				culture: null,
				segment: null,
				name: 'Color Picker',
				createDate: '2023-02-20 16:20:02',
				updateDate: '2026-01-22 12:45:30.0940108',
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
				value: '{\r\n  "label": "cc0000",\r\n  "value": "#cc0000"\r\n}',
			},
			{
				editorAlias: 'Umbraco.ColorPicker',
				alias: 'colorPickerLabels',
				culture: null,
				segment: null,
				value: '{\r\n  "value": "#cc0000",\r\n  "label": "Red"\r\n}',
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
				publishDate: '2026-01-22 12:45:30.1023688',
				culture: null,
				segment: null,
				name: 'Content Picker',
				createDate: '2023-02-20 16:22:48',
				updateDate: '2026-01-22 12:45:30.1023688',
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
				value:
					'umb://document/a3a37004139f4254ba563ed381b3007c,umb://document/15b092f066b540e5aa1b25b71b2bd81a,umb://document/4e02a6bf5ab64b558f06c6d24e892f8c,umb://document/a7823036048644f5af33deb6780e07e6,umb://document/80954b941d324edd9c01105561a7415d,umb://document/17149c1e44a84882a0886a1d84e0e86a,umb://document/9394af8fd30647789f032431eb8f5b6b',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerAllowedTypes',
				culture: null,
				segment: null,
				value: 'umb://document/db79156b3d5b43d6ab32902dc423bec3',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerFullyConfigured',
				culture: null,
				segment: null,
				value:
					'umb://document/23b1bf0ac56e4b0ca2a9a83d0d9708ef,umb://document/58e300ad868c4a8499152aef20ea681c,umb://document/0865b2abad7c48d4a8c6608986a0e942,umb://document/4babad2f967a49e19f92407e95ff9df9',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMinAndMax',
				culture: null,
				segment: null,
				value:
					'umb://document/2329915bfb6b4c2f91798c16ba125cea,umb://document/4babad2f967a49e19f92407e95ff9df9,umb://document/db2a48d55883465fb1d7e012af2f16d0,umb://document/119f5ef031cf4d599c982f3cbe2fa8df',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerStartNode',
				culture: null,
				segment: null,
				value: 'umb://document/0865b2abad7c48d4a8c6608986a0e942,umb://document/4babad2f967a49e19f92407e95ff9df9',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerXPathStartNode',
				culture: null,
				segment: null,
				value: 'umb://document/23b1bf0ac56e4b0ca2a9a83d0d9708ef',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMediaDefaultConfig',
				culture: null,
				segment: null,
				value: 'umb://media/f06adb918cdd408d83ddf7b833fc393c,umb://media/b44956af620a4e17bbce3987446fb2f1',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMediaFullyConfigured',
				culture: null,
				segment: null,
				value: 'umb://media/a0651d9814a94d92813336f59b248d31',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMembersDefaultConfig',
				culture: null,
				segment: null,
				value: 'umb://member/d74d2bd0f55a4a06beb8d8e931fc726b,umb://member/e93b25575fcb4495bbb39f5fd87055a8',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerMembersFullyConfigured',
				culture: null,
				segment: null,
				value: 'umb://member/e93b25575fcb4495bbb39f5fd87055a8,umb://member/d74d2bd0f55a4a06beb8d8e931fc726b',
			},
			{
				editorAlias: 'Umbraco.MultiNodeTreePicker',
				alias: 'multinodeTreepickerDynamicRoot',
				culture: null,
				segment: null,
				value: 'umb://document/9c2a36bce05642e9aa3155d85683b6b4',
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
				publishDate: '2026-01-22 12:45:30.1114532',
				culture: null,
				segment: null,
				name: 'DateTime Picker',
				createDate: '2023-02-20 16:20:15',
				updateDate: '2026-01-22 12:45:30.1114532',
				id: '0865b2ab-ad7c-48d4-a8c6-608986a0e942',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.DateTime',
				alias: 'dateTimePickerDatePlusTimeFormat',
				culture: null,
				segment: null,
				value: '2024-04-15 11:00:00',
			},
			{
				editorAlias: 'Umbraco.DateTime',
				alias: 'dateTimePickerOffsetTime',
				culture: null,
				segment: null,
				value: '2024-04-15 11:00:00',
			},
			{
				editorAlias: 'Umbraco.DateTime',
				alias: 'dateTimePickerTime',
				culture: null,
				segment: null,
				value: '2024-07-15 15:42:02',
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
				publishDate: '2026-01-22 12:45:30.1184843',
				culture: null,
				segment: null,
				name: 'Decimal',
				createDate: '2023-02-20 16:20:20',
				updateDate: '2026-01-22 12:45:30.1184843',
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
				value: '200.0',
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
				publishDate: '2026-01-22 12:45:30.1246698',
				culture: null,
				segment: null,
				name: 'Document Picker',
				createDate: '2023-02-20 16:20:08',
				updateDate: '2026-01-22 12:45:30.1246698',
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
				value: 'umb://document/4babad2f967a49e19f92407e95ff9df9',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerIgnoreUserStartNodes',
				culture: null,
				segment: null,
				value: 'umb://document/9394af8fd30647789f032431eb8f5b6b',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerShowOpenButton',
				culture: null,
				segment: null,
				value: 'umb://document/a7823036048644f5af33deb6780e07e6',
			},
			{
				editorAlias: 'Umbraco.ContentPicker',
				alias: 'contentPickerStartNode',
				culture: null,
				segment: null,
				value: 'umb://document/c680be850bb744299d4a73ffb83e427b',
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
				publishDate: '2026-01-22 12:45:30.1296363',
				culture: null,
				segment: null,
				name: 'Dropdown',
				createDate: '2023-02-20 16:20:24',
				updateDate: '2026-01-22 12:45:30.1296363',
				id: 'db2a48d5-5883-465f-b1d7-e012af2f16d0',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.DropDown.Flexible',
				alias: 'dropdownMultiValueRequired',
				culture: null,
				segment: null,
				value: '["One","Three","Five"]',
			},
			{
				editorAlias: 'Umbraco.DropDown.Flexible',
				alias: 'dropdownSingleValueRequired',
				culture: null,
				segment: null,
				value: '["Five"]',
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
				publishDate: '2026-01-22 12:45:30.1378781',
				culture: null,
				segment: null,
				name: 'Email Address',
				createDate: '2023-02-20 16:20:30',
				updateDate: '2026-01-22 12:45:30.1378781',
				id: '119f5ef0-31cf-4d59-9c98-2f3cbe2fa8df',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.1429528',
				culture: null,
				segment: null,
				name: 'Eye Dropper Color Picker',
				createDate: '2023-02-20 16:20:39',
				updateDate: '2026-01-22 12:45:30.1429528',
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
				value: '#13f1ba',
			},
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerAlpha',
				culture: null,
				segment: null,
				value: '#f014cb2b',
			},
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerPalette',
				culture: null,
				segment: null,
				value: '#bd10e0',
			},
			{
				editorAlias: 'Umbraco.ColorPicker.EyeDropper',
				alias: 'eyeDropperColorPickerFullyConfigured',
				culture: null,
				segment: null,
				value: '#f8e61b54',
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
				publishDate: '2026-01-22 12:45:30.1485295',
				culture: null,
				segment: null,
				name: 'File Upload',
				createDate: '2023-05-22 13:09:31',
				updateDate: '2026-01-22 12:45:30.1485295',
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
				value: '/media/mhqpjciq/deane-barker-things-you-should-know.pdf',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.1536211',
				culture: null,
				segment: null,
				name: 'Image Cropper',
				createDate: '2023-03-03 12:47:12',
				updateDate: '2026-01-22 12:45:30.1536211',
				id: '3702fd21-cad5-4eac-aa28-de44bf5a6246',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.ImageCropper',
				alias: 'imageCropperWithCrops',
				culture: null,
				segment: null,
				value: {
					focalPoint: {
						left: 0.2422222900390625,
						top: 0.3055555216471354,
					},
					crops: [
						{
							alias: 'one',
							width: 111,
							height: 111,
							coordinates: {
								x1: 0.051214949679824366,
								y1: 0.08109690332014464,
								x2: 0.5384405748923247,
								y2: 0.5085586212520044,
							},
						},
						{
							alias: 'two',
							width: 222,
							height: 222,
							coordinates: {
								x1: 0.5972945021257858,
								y1: 0,
								x2: 0.03287719647963386,
								y2: 0.6301716986054198,
							},
						},
						{
							alias: 'three',
							width: 333,
							height: 333,
							coordinates: {
								x1: 0,
								y1: 0.48028200560319445,
								x2: 0.6561165906079788,
								y2: 0.17583458500478433,
							},
						},
					],
					src: '/media/uqpbbihn/nxnw_300x300.jpg',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.1599783',
				culture: null,
				segment: null,
				name: 'Label',
				createDate: '2023-02-27 08:39:27',
				updateDate: '2026-01-22 12:45:30.1599783',
				id: '7bf4865b-de55-4f85-bd2c-9cb8e6e482c3',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.Label',
				alias: 'labelString',
				culture: null,
				segment: null,
				value:
					'<p><img alt="image" src="https://localhost:44339/media/dqaijrza/nxnw_300x300.jpg"></p>\n<p>This description has <strong>bold</strong>, <em>italic</em> and <del>strikethrough</del>.\nThis is a <a href="https://umbraco.com">link</a>.</p>\n<ul>\n<li>List item 1</li>\n<li>List item 2</li>\n<li>List item 3</li>\n</ul>\n<blockquote>\n<p>blockquote</p>\n</blockquote>\n<ol>\n<li>#general_add</li>\n<li>#general_choose</li>\n<li>#general_close</li>\n</ol>\n<h1>Heading 1</h1>\n<h2>Heading 2</h2>\n<h3>Heading 3</h3>\n<h4>Heading 4</h4>\n<h5>Heading 5</h5>\n<h6>Heading 6</h6>\n<p>Inline <code>code</code></p>\n<pre><code>// block\n// code\n</code></pre>\n<p><i>Custom</i> <b>HTML</b></p>',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.1664743',
				culture: null,
				segment: null,
				name: 'Markdown Editor',
				createDate: '2023-02-20 16:20:45',
				updateDate: '2026-01-22 12:45:30.1664743',
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
					'*This* is the _default_ value!\nSuper duper default value.\n\n- List item one\n- List item two\n\n## HEADING!\n\n![More text](https://localhost:44339/media/dqaijrza/nxnw_300x300.jpg)',
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
				publishDate: '2026-01-22 12:45:30.1733938',
				culture: null,
				segment: null,
				name: 'Media Picker',
				createDate: '2023-02-20 16:20:55',
				updateDate: '2026-01-22 12:45:30.1733938',
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
						key: 'af265a68-6915-4f55-b050-9ac3eb7cfa00',
						mediaKey: '30343150-b3a7-480f-ad7c-011bdae172a2',
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
						key: 'f5256289-0443-4e4a-9a3d-85991fe9f5ad',
						mediaKey: 'b44956af-620a-4e17-bbce-3987446fb2f1',
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
						key: '310e5944-58b0-4616-9916-a9441f720955',
						mediaKey: 'c784001a-e320-4ada-a98a-4a4506632441',
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
						key: '63d4bdc5-521d-4dc0-82fe-c848c85d4be3',
						mediaKey: 'b44956af-620a-4e17-bbce-3987446fb2f1',
						mediaTypeAlias: 'Image',
						crops: [
							{
								alias: 'one',
								width: 100,
								height: 100,
								coordinates: {
									x1: 0.5932413200718122,
									y1: 0.16286041356135988,
									x2: 0.12469649700723436,
									y2: 0.4610578694640045,
								},
							},
							{
								alias: 'two',
								width: 200,
								height: 200,
								coordinates: {
									x1: 0.5222311211535575,
									y1: 0.5392087889322541,
									x2: 0.2713843466692956,
									y2: 0.18561057906148476,
								},
							},
						],
						focalPoint: {
							left: 0.6981907894736842,
							top: 0.2894736842105263,
						},
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
							left: 0.6936994945429961,
							top: 0.29122129212277315,
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
						key: '51d1463e-20cd-4a6f-b9ab-695cea1e87e9',
						mediaKey: 'a0651d98-14a9-4d92-8133-36f59b248d31',
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
						key: '5a3ff2e3-8507-4359-8e4b-fa593d83a2f2',
						mediaKey: 'a0651d98-14a9-4d92-8133-36f59b248d31',
						mediaTypeAlias: 'Image',
						crops: [],
						focalPoint: null,
					},
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
				],
			},
			{
				editorAlias: 'Umbraco.MediaPicker3',
				alias: 'mediaPickerStartNode',
				culture: null,
				segment: null,
				value: [
					{
						key: 'c415cf20-f364-433f-bc2c-5a60051fbc4b',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.1820179',
				culture: null,
				segment: null,
				name: 'Member Group Picker',
				createDate: '2023-02-20 16:21:02',
				updateDate: '2026-01-22 12:45:30.1820179',
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
				value: '1105,1106',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.1884818',
				culture: null,
				segment: null,
				name: 'Member Picker',
				createDate: '2023-02-20 16:21:11',
				updateDate: '2026-01-22 12:45:30.1884818',
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
				value: 'umb://member/e93b25575fcb4495bbb39f5fd87055a8',
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
				publishDate: '2026-01-22 12:45:30.1949629',
				culture: null,
				segment: null,
				name: 'Multi URL Picker',
				createDate: '2023-02-20 16:22:41',
				updateDate: '2026-01-22 12:45:30.1949629',
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
						name: 'Decimal',
						target: null,
						udi: 'umb://document/4babad2f967a49e19f92407e95ff9df9',
						url: null,
						queryString: null,
					},
					{
						name: 'Color Picker',
						target: '_blank',
						udi: 'umb://document/23b1bf0ac56e4b0ca2a9a83d0d9708ef',
						url: null,
						queryString: null,
					},
					{
						name: 'Umbraco Dot Com',
						target: '_blank',
						udi: null,
						url: 'https://umbraco.com',
						queryString: null,
					},
					{
						name: 'RTE',
						target: null,
						udi: 'umb://document/88db0dc539c7485281a5c0c9f0740f68',
						url: null,
						queryString: '?v=1',
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
						target: null,
						udi: 'umb://document/2329915bfb6b4c2f91798c16ba125cea',
						url: null,
						queryString: null,
					},
					{
						name: 'Pexels Mark Stebnicki 2255924',
						target: null,
						udi: 'umb://media/a0651d9814a94d92813336f59b248d31',
						url: null,
						queryString: null,
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
						target: null,
						udi: 'umb://document/58e300ad868c4a8499152aef20ea681c',
						url: null,
						queryString: null,
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
						target: null,
						udi: 'umb://document/c680be850bb744299d4a73ffb83e427b',
						url: null,
						queryString: null,
					},
					{
						name: 'DateTime Picker',
						target: null,
						udi: 'umb://document/0865b2abad7c48d4a8c6608986a0e942',
						url: null,
						queryString: null,
					},
					{
						name: 'Pexels Dxt 73640',
						target: null,
						udi: 'umb://media/76c02ec86a824c4795da56f6628b58fb',
						url: null,
						queryString: null,
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
						target: '_blank',
						udi: null,
						url: 'https://umbraco.com',
						queryString: '?v=1',
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
						target: null,
						udi: 'umb://document/4babad2f967a49e19f92407e95ff9df9',
						url: null,
						queryString: null,
					},
					{
						name: 'Markdown Editor',
						target: null,
						udi: 'umb://document/9394af8fd30647789f032431eb8f5b6b',
						url: null,
						queryString: null,
					},
					{
						name: 'Pexels Engin Akyurt 1435904',
						target: null,
						udi: 'umb://media/b44956af620a4e17bbce3987446fb2f1',
						url: null,
						queryString: null,
					},
					{
						name: 'Umbraco Dot Com',
						target: '_blank',
						udi: null,
						url: 'https://umbraco.com',
						queryString: null,
					},
					{
						name: 'Multiple Textstring',
						target: null,
						udi: 'umb://document/17149c1e44a84882a0886a1d84e0e86a',
						url: null,
						queryString: null,
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
				publishDate: '2026-01-22 12:45:30.2032271',
				culture: null,
				segment: null,
				name: 'Multiple Textstring',
				createDate: '2023-02-20 16:22:56',
				updateDate: '2026-01-22 12:45:30.2032271',
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
				value: 'One\nTwo\nThree',
			},
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringFullyConfigured',
				culture: null,
				segment: null,
				value: 'One\nTwo\nTree\nFour\nFive\nSix\nSeven\nEight',
			},
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringMax',
				culture: null,
				segment: null,
				value: 'One\nTwo\nThree\nFour\nFive\nSix\nSeven\nEight\nNine\nTen',
			},
			{
				editorAlias: 'Umbraco.MultipleTextstring',
				alias: 'multipleTextstringMin',
				culture: null,
				segment: null,
				value: 'One\nTwo\nThree\nFour\nFive',
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
				publishDate: '2026-01-22 12:45:30.211188',
				culture: null,
				segment: null,
				name: 'Numeric',
				createDate: '2023-02-20 16:23:03',
				updateDate: '2026-01-22 12:45:30.211188',
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
				value: -13,
			},
			{
				editorAlias: 'Umbraco.Integer',
				alias: 'numericMinAndMax',
				culture: null,
				segment: null,
				value: 2,
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
				value: 69,
			},
			{
				editorAlias: 'Umbraco.Integer',
				alias: 'numericMisconfigured',
				culture: null,
				segment: null,
				value: 6,
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.2194655',
				culture: null,
				segment: null,
				name: 'Radio Button List',
				createDate: '2023-02-20 16:23:09',
				updateDate: '2026-01-22 12:45:30.2194655',
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
				value: 'Four',
			},
			{
				editorAlias: 'Umbraco.RadioButtonList',
				alias: 'radioButtonList2',
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
		id: '88db0dc5-39c7-4852-81a5-c0c9f0740f68',
		createDate: '2024-09-12 11:56:47',
		parent: {
			id: 'db79156b-3d5b-43d6-ab32-902dc423bec3',
		},
		documentType: {
			id: 'c07f8d38-302f-4e4a-bd84-d57d79a4af46',
			icon: 'icon-application-window color-green',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2026-01-22 12:45:30.2309734',
				culture: null,
				segment: null,
				name: 'Rich Text Editor Tiptap',
				createDate: '2024-09-12 11:56:47',
				updateDate: '2026-01-22 12:45:30.2309734',
				id: '88db0dc5-39c7-4852-81a5-c0c9f0740f68',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.RichText',
				alias: 'richTextEditorTiptapDefaultConfig',
				culture: null,
				segment: null,
				value: {
					markup:
						'<h1 class="foo" data-foo-bar="true" id="bar" style="color: red; text-align: center"><span style="color: blue;">Hello</span> world</h1><p>Test</p><umb-rte-block data-content-key="b4bea235-4c0e-4449-8e15-c21b9fc88ac5"></umb-rte-block><p>Test</p><p>Hello&nbsp;<umb-rte-block-inline data-content-key="1610e24c-619e-4f0d-a1b1-fd926da0eef7"></umb-rte-block-inline>&nbsp;world.</p><p>Test</p><p><img data-udi="umb://media/c784001ae3204adaa98a4a4506632441" src="?rmode=max&amp;width=500&amp;height=500&amp;hmac=98fa916544191ff181d550bd5b5559f9b092f29b178b824553b9fa52e5c563e6" alt="nxnw_300x300.jpg" width="500" height="500"></p><p><span class="foo" data-foo-bar="true" id="bar2">This is a</span> <code>span</code> <span class="foo" data-foo-bar="true" id="bar2">tag.</span></p><div>This is a <code>div</code> tag. <a id="test"></a></div><p><span style="color: rgb(249, 10, 10);">This </span><a target="_blank" data-router-slot="disabled" href="https://umbraco.com" rel="noopener noreferrer nofollow" type="external">paragraph</a> <span style="color: rgb(249, 10, 10);">contains </span><strong>bold</strong>, <em>italic</em> and <u>underlined</u> text. As well as <span style="font-family: serif; font-size: 10pt;">Serif</span>, <span style="font-family: sans-serif; font-size: 12pt;">Sans-serif</span>, <span style="font-family: monospace; font-size: 16pt;">Monospace</span>, <span style="font-family: cursive; font-size: 18pt;">Cursive</span> and <span style="font-family: fantasy; font-size: 24pt;">Fantasy</span>.</p><table style="min-width: 75px"><colgroup><col style="min-width: 25px"><col style="min-width: 25px"><col style="min-width: 25px"></colgroup><tbody><tr><td colspan="1" rowspan="1"><p></p></td><td colspan="1" rowspan="1"><p></p></td><td colspan="1" rowspan="1"><p></p></td></tr><tr><td colspan="1" rowspan="1"><p></p></td><td colspan="1" rowspan="1"><p></p></td><td colspan="1" rowspan="1"><p></p></td></tr><tr><td colspan="1" rowspan="1"><p></p></td><td colspan="1" rowspan="1"><p></p></td><td colspan="1" rowspan="1"><p></p></td></tr></tbody></table><p><span class="umb-embed-holder" data-embed-constrain="false" data-embed-height="240" data-embed-url="https://www.youtube.com/watch?v=J---aiyznGQ" data-embed-width="360"><iframe width="320" height="240" src="https://www.youtube.com/embed/J---aiyznGQ?feature=oembed" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen="" title="Keyboard Cat! - THE ORIGINAL!"></iframe></span></p><hr><p><a target="_blank" data-router-slot="disabled" href="https://www.youtube.com/" rel="noopener" title="This is an external link." type="external">This is an external link.</a></p><p><a target="" data-router-slot="disabled" data-anchor="?v=J---aiyznGQ" href="https://www.youtube.com/watch?v=J---aiyznGQ" title="This is an external link with a querystring." type="external">This is an external link with a querystring.</a></p><p><a target="" data-router-slot="disabled" data-anchor="#contents" href="https://www.youtube.com/#contents" title="This is an external link with an anchor." type="external">This is an external link with an anchor.</a></p><p><a target="" data-router-slot="disabled" href="/{localLink:464ca81d-30e0-4169-899a-0556303b878c}" title="This is an internal document link." type="document">This is an internal document link.</a></p><p><a target="" data-router-slot="disabled" data-anchor="#test" href="/{localLink:464ca81d-30e0-4169-899a-0556303b878c}#test" title="This is an internal document link with anchor hash." type="document">This is an internal document link with anchor hash.</a></p><p><a target="" data-router-slot="disabled" data-anchor="?v=1" href="/{localLink:464ca81d-30e0-4169-899a-0556303b878c}?v=1" title="This is an internal document link with querystring." type="document">This is an internal document link with querystring.</a></p><p><a target="" data-router-slot="disabled" href="/{localLink:88db0dc5-39c7-4852-81a5-c0c9f0740f68}" title="This is a link to self." type="document">This is a link to self.</a></p><p><a target="" data-router-slot="disabled" data-anchor="#test" href="/{localLink:88db0dc5-39c7-4852-81a5-c0c9f0740f68}#test" title="This is a link to self with anchor hash." type="document">This is a link to self with anchor hash.</a></p><p><a target="" data-router-slot="disabled" data-anchor="?v=1" href="/{localLink:88db0dc5-39c7-4852-81a5-c0c9f0740f68}?v=1" title="This is a link to self with querystring." type="document">This is a link to self with querystring.</a></p><p><a target="" data-router-slot="disabled" data-anchor="#test" href="#test" title="This is a link with anchor hash without URL." type="external">This is a link with anchor hash without URL.</a></p><p><a target="" data-router-slot="disabled" data-anchor="?v=1" href="/?v=1" title="This is a link with querystring without URL." type="external">This is a link with querystring without URL.</a></p><hr><p>This property has an alias of <em>codeEditor</em>, and a label of <strong>Code Editor</strong>.</p><ul id="bar3"><li><p class="foo">Todo 1</p></li><li><p class="bar">Todo 2</p></li><li><p>Todo 3</p></li></ul><p>Here is some inline markup: <strong>Umbraco</strong> <strong>Bellissima</strong>.</p><p>This is a loooooooooooooooooooooooooooooooooooong line.</p><hr><p>This description has <strong>bold</strong>, <em>italic</em> and <s>strikethrough</s>.<br>This is a <a target="_blank" data-router-slot="disabled" href="https://umbraco.com/" rel="noopener noreferrer nofollow" type="external">link</a>.</p><ul><li><p>List item 1</p></li><li><p>List item 2</p></li><li><p>List item 3</p></li></ul><blockquote><p>blockquote</p></blockquote><ol><li><p>Add</p></li><li><p>Choose</p></li><li><p>Close</p></li></ol><hr><h1>Heading 1</h1><h2>Heading 2</h2><h3>Heading 3</h3><h4>Heading 4</h4><h5>Heading 5</h5><h6>Heading 6</h6><p>Inline <code>code</code></p><pre><code>// block\n// code</code></pre><p><em>Custom</em> <strong>HTML</strong></p><hr><p>fsdf</p><p>sfd</p><p>dsf</p><p>dsf</p><p>dsf</p><table style="min-width: 25px"><colgroup><col style="min-width: 25px"></colgroup><tbody><tr><td colspan="1" rowspan="1"><p></p></td></tr></tbody></table><hr><p></p>',
					blocks: {
						contentData: [
							{
								contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
								udi: null,
								key: 'b4bea235-4c0e-4449-8e15-c21b9fc88ac5',
								values: [
									{
										editorAlias: 'Umbraco.RadioButtonList',
										culture: null,
										segment: null,
										alias: 'radioButtonList',
										value: 'One',
									},
									{
										editorAlias: 'Umbraco.TextBox',
										culture: null,
										segment: null,
										alias: 'title',
										value: 'Test',
									},
								],
							},
							{
								contentTypeKey: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
								udi: null,
								key: '1610e24c-619e-4f0d-a1b1-fd926da0eef7',
								values: [],
							},
						],
						settingsData: [
							{
								contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
								udi: null,
								key: '67e6270b-db36-44f5-a292-5fa2c6b368ba',
								values: [
									{
										editorAlias: 'Umbraco.TextBox',
										culture: null,
										segment: null,
										alias: 'title',
										value: 'Test Page',
									},
									{
										editorAlias: 'Umbraco.RadioButtonList',
										culture: null,
										segment: null,
										alias: 'radioButtonList',
										value: 'One',
									},
								],
							},
						],
						expose: [
							{
								contentKey: 'b4bea235-4c0e-4449-8e15-c21b9fc88ac5',
								culture: null,
								segment: null,
							},
							{
								contentKey: '1610e24c-619e-4f0d-a1b1-fd926da0eef7',
								culture: null,
								segment: null,
							},
						],
						Layout: {
							'Umbraco.RichText': [
								{
									contentUdi: null,
									settingsUdi: null,
									contentKey: 'b4bea235-4c0e-4449-8e15-c21b9fc88ac5',
									settingsKey: null,
								},
								{
									contentUdi: null,
									settingsUdi: null,
									contentKey: '1610e24c-619e-4f0d-a1b1-fd926da0eef7',
									settingsKey: '67e6270b-db36-44f5-a292-5fa2c6b368ba',
								},
							],
						},
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.2366959',
				culture: null,
				segment: null,
				name: 'Slider',
				createDate: '2023-02-20 16:23:23',
				updateDate: '2026-01-22 12:45:30.2366959',
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
				value: '37',
			},
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderInitialValue',
				culture: null,
				segment: null,
				value: '37',
			},
			{
				editorAlias: 'Umbraco.Slider',
				alias: 'sliderMinAndMax',
				culture: null,
				segment: null,
				value: '37',
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
				value: '37,64',
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
				publishDate: '2026-01-22 12:45:30.2421158',
				culture: null,
				segment: null,
				name: 'Tags',
				createDate: '2023-02-27 08:34:05',
				updateDate: '2026-01-22 12:45:30.2421158',
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
				value: ['hello', 'world', 'test'],
			},
			{
				editorAlias: 'Umbraco.Tags',
				alias: 'tagsCustomGroupCSVStorage',
				culture: null,
				segment: null,
				value: 'hello,world,test',
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
				state: 'Draft',
				publishDate: '2026-01-22 12:45:30.2479086',
				culture: null,
				segment: null,
				name: 'Textarea',
				createDate: '2023-02-20 16:23:29',
				updateDate: '2026-01-22 12:45:30.2479086',
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
				value:
					'Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars Something with max chars ',
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
				publishDate: '2026-01-22 12:45:30.2548777',
				culture: null,
				segment: null,
				name: 'Textbox',
				createDate: '2023-02-26 15:39:37',
				updateDate: '2026-01-22 12:45:30.2548777',
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
				value:
					'The config with max chars ksldjf klsdfkl sdjflk ksldjflk jskdjf jlskdjflk jsdkfj klsdjfklj lksadjfkluweioru jxdcvknjkldsj IWEI KJASDKLFJ LIASWEOPRI ',
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
				publishDate: '2026-01-21 18:16:00.706676',
				culture: null,
				segment: null,
				name: 'Toggle',
				createDate: '2023-02-20 16:23:36',
				updateDate: '2026-01-21 18:16:00.706676',
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
				state: 'Draft',
				publishDate: '2026-01-21 18:16:00.7109753',
				culture: 'en-US',
				segment: null,
				name: 'User Picker',
				createDate: '2023-02-27 08:34:21',
				updateDate: '2026-01-21 18:16:00.7109753',
				id: '06e4bd2f-98f7-48cc-85e9-2a4b8cd668e8',
				flags: [],
			},
			{
				state: 'Draft',
				publishDate: '2026-01-21 18:16:00.7109753',
				culture: 'da-dk',
				segment: null,
				name: 'Brugervælger',
				createDate: '2023-02-27 08:34:21',
				updateDate: '2026-01-21 18:16:00.7109753',
				id: '06e4bd2f-98f7-48cc-85e9-2a4b8cd668e8',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.UserPicker',
				alias: 'userPicker',
				culture: 'en-US',
				segment: null,
				value: 1,
			},
		],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
			},
			{
				id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
			},
		],
		template: null,
		id: '2bbefe25-f90e-4b09-b10d-2fa9fc6580b7',
		createDate: '2025-04-14 17:47:08',
		parent: {
			id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
		},
		documentType: {
			id: '5d53249e-330c-4527-a3fb-2ddab802305c',
			icon: 'icon-tag',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-04-14 17:47:08',
				culture: null,
				segment: null,
				name: 'Category 1',
				createDate: '2025-04-14 17:47:08',
				updateDate: '2025-04-14 17:47:08',
				id: '2bbefe25-f90e-4b09-b10d-2fa9fc6580b7',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
			},
			{
				id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
			},
		],
		template: null,
		id: '1bfa2bde-5282-4925-bf2b-33cd4c797a94',
		createDate: '2025-04-14 17:47:15',
		parent: {
			id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
		},
		documentType: {
			id: '5d53249e-330c-4527-a3fb-2ddab802305c',
			icon: 'icon-tag',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published',
				publishDate: '2025-04-14 17:47:15',
				culture: null,
				segment: null,
				name: 'Category 2',
				createDate: '2025-04-14 17:47:15',
				updateDate: '2025-04-14 17:47:15',
				id: '1bfa2bde-5282-4925-bf2b-33cd4c797a94',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},
	{
		ancestors: [
			{
				id: 'f8de6313-331a-45ef-a8a3-e0135055de6b',
			},
			{
				id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
			},
		],
		template: null,
		id: '2ab874a5-f77c-4254-94b9-5510491a9cd1',
		createDate: '2025-06-05 17:03:15',
		parent: {
			id: '04e23c98-30e1-46e8-bd93-5a38b1a6e90a',
		},
		documentType: {
			id: '5d53249e-330c-4527-a3fb-2ddab802305c',
			icon: 'icon-tag',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Draft',
				publishDate: '2025-06-05 17:03:15',
				culture: null,
				segment: null,
				name: 'Category 3',
				createDate: '2025-06-05 17:03:15',
				updateDate: '2025-06-05 17:03:15',
				id: '2ab874a5-f77c-4254-94b9-5510491a9cd1',
				flags: [],
			},
		],
		values: [],
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
