import type { UmbMockDataTypeModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockDataTypeModel> = [
	{
		"name": "Test Data Types",
		"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a",
		"parent": null,
		"editorAlias": "",
		"editorUiAlias": "",
		"hasChildren": true,
		"isFolder": true,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Color Picker - No Labels",
		"id": "a62e05d6-f7f8-4929-b35b-2e3068692eb6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ColorPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ColorPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "useLabel",
				"value": false
			},
			{
				"alias": "items",
				"value": [
					{
						"value": "000000",
						"label": "000000"
					},
					{
						"value": "cc0000",
						"label": "cc0000"
					},
					{
						"value": "ffffff",
						"label": ""
					}
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4720905+00:00"
			}
		]
	},
	{
		"name": "Color Picker - Labels",
		"id": "c4fb2e7f-c707-41c4-994f-b224d0b66612",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ColorPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ColorPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "useLabel",
				"value": true
			},
			{
				"alias": "items",
				"value": [
					{
						"value": "000000",
						"label": "Black"
					},
					{
						"value": "cc0000",
						"label": "Red"
					},
					{
						"value": "ffffff",
						"label": "White"
					}
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4736977+00:00"
			}
		]
	},
	{
		"name": "List View - Members",
		"id": "aa2c52a0-ce87-4e65-a47c-7df09358585d",
		"parent": null,
		"editorAlias": "Umbraco.ListView",
		"editorUiAlias": "Umb.PropertyEditorUi.Collection",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "pageSize",
				"value": 10
			},
			{
				"alias": "orderBy",
				"value": "username"
			},
			{
				"alias": "orderDirection",
				"value": "asc"
			},
			{
				"alias": "includeProperties",
				"value": [
					{
						"alias": "username",
						"header": "User name",
						"nameTemplate": null,
						"isSystem": true
					},
					{
						"alias": "email",
						"header": "Email",
						"nameTemplate": null,
						"isSystem": true
					},
					{
						"alias": "updateDate",
						"header": "Last edited",
						"nameTemplate": null,
						"isSystem": true
					}
				]
			},
			{
				"alias": "layouts",
				"value": [
					{
						"name": "List",
						"collectionView": "Umb.CollectionView.Member.Table",
						"icon": "icon-list",
						"isSystem": true,
						"selected": true
					},
					{
						"name": "Grid",
						"collectionView": "Umb.CollectionView.Member.Grid",
						"icon": "icon-thumbnails-small",
						"isSystem": true,
						"selected": true
					}
				]
			},
			{
				"alias": "bulkActionPermissions",
				"value": {
					"allowBulkPublish": true,
					"allowBulkUnpublish": true,
					"allowBulkCopy": true,
					"allowBulkMove": true,
					"allowBulkDelete": true
				}
			},
			{
				"alias": "icon",
				"value": "icon-badge color-black"
			},
			{
				"alias": "showContentFirst",
				"value": false
			},
			{
				"alias": "useInfiniteEditor",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4327608+00:00"
			}
		]
	},
	{
		"name": "List View - Media",
		"id": "3a0156c4-3b8c-4803-bdc1-6871faa83fff",
		"parent": null,
		"editorAlias": "Umbraco.ListView",
		"editorUiAlias": "Umb.PropertyEditorUi.Collection",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "pageSize",
				"value": 100
			},
			{
				"alias": "orderBy",
				"value": "sortOrder"
			},
			{
				"alias": "orderDirection",
				"value": "desc"
			},
			{
				"alias": "layouts",
				"value": [
					{
						"name": "Table",
						"collectionView": "Umb.CollectionView.Media.Table",
						"icon": "icon-table color-undefined",
						"isSystem": true,
						"selected": true
					},
					{
						"name": "Grid",
						"collectionView": "Umb.CollectionView.Media.Grid",
						"icon": "icon-grid color-undefined",
						"isSystem": true,
						"selected": true
					}
				]
			},
			{
				"alias": "includeProperties",
				"value": [
					{
						"alias": "updateDate",
						"header": "Last edited",
						"nameTemplate": null,
						"isSystem": true
					},
					{
						"alias": "updater",
						"header": "Updated by",
						"isSystem": 1
					},
					{
						"alias": "sortOrder",
						"header": "#general_sort",
						"isSystem": 1,
						"nameTemplate": "~{=value}~"
					},
					{
						"alias": "umbracoBytes",
						"header": "#general_size",
						"isSystem": 0,
						"nameTemplate": "{=value | bytes}"
					},
					{
						"alias": "umbracoFile",
						"header": "Image",
						"isSystem": 0,
						"nameTemplate": ""
					}
				]
			},
			{
				"alias": "bulkActionPermissions",
				"value": {
					"allowBulkPublish": false,
					"allowBulkUnpublish": false,
					"allowBulkCopy": false,
					"allowBulkMove": false,
					"allowBulkDelete": false
				}
			},
			{
				"alias": "icon",
				"value": "icon-photo-album color-text"
			},
			{
				"alias": "showContentFirst",
				"value": false
			},
			{
				"alias": "useInfiniteEditor",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.451882+00:00"
			},
			{
				"alias": "tabName",
				"value": "Photos"
			}
		]
	},
	{
		"name": "List View - Content",
		"id": "c0808dd3-8133-4e4b-8ce8-e2bea84a96a4",
		"parent": null,
		"editorAlias": "Umbraco.ListView",
		"editorUiAlias": "Umb.PropertyEditorUi.Collection",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "pageSize",
				"value": 10
			},
			{
				"alias": "orderBy",
				"value": "sortOrder"
			},
			{
				"alias": "orderDirection",
				"value": "asc"
			},
			{
				"alias": "layouts",
				"value": [
					{
						"icon": "icon-crown color-red",
						"name": "Tabel",
						"collectionView": "Umb.CollectionView.Document.Table"
					},
					{
						"icon": "icon-grid",
						"name": "Cards",
						"collectionView": "Umb.CollectionView.Document.Grid"
					}
				]
			},
			{
				"alias": "includeProperties",
				"value": [
					{
						"alias": "variantTextstring",
						"header": "Variant Textstring",
						"isSystem": 0,
						"nameTemplate": "{umbContentName:value}"
					},
					{
						"alias": "sortOrder",
						"header": "#general_sort",
						"isSystem": 1,
						"nameTemplate": "~{=value}~"
					},
					{
						"alias": "updateDate",
						"header": "Last updated",
						"nameTemplate": "${ value }",
						"isSystem": true
					},
					{
						"alias": "creator",
						"header": "Created by",
						"nameTemplate": "**{=value | uppercase | truncate:4:true }**",
						"isSystem": true
					},
					{
						"alias": "updater",
						"header": "Updated by",
						"isSystem": 1,
						"nameTemplate": "{#general_choose | uppercase2} _**{=value | lowercase}**_"
					},
					{
						"alias": "colorPickerNoLabels",
						"header": "Color Picker",
						"isSystem": 0
					},
					{
						"alias": "multinodeTreepickerDefaultConfig",
						"header": "Content Picker",
						"isSystem": 0
					}
				]
			},
			{
				"alias": "bulkActionPermissions",
				"value": {
					"allowBulkPublish": false,
					"allowBulkUnpublish": false,
					"allowBulkCopy": false,
					"allowBulkMove": false,
					"allowBulkDelete": false
				}
			},
			{
				"alias": "icon",
				"value": "icon-globe color-green"
			},
			{
				"alias": "showContentFirst",
				"value": false
			},
			{
				"alias": "useInfiniteEditor",
				"value": true
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4532962+00:00"
			},
			{
				"alias": "tabName",
				"value": ""
			}
		]
	},
	{
		"name": "Numeric",
		"id": "2e6d3631-066e-44b8-aec4-96f09099b2b5",
		"parent": null,
		"editorAlias": "Umbraco.Integer",
		"editorUiAlias": "Umb.PropertyEditorUi.Integer",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "True/false",
		"id": "92897bc6-a5f3-4ffe-ae27-f2e7e33dda49",
		"parent": null,
		"editorAlias": "Umbraco.TrueFalse",
		"editorUiAlias": "Umb.PropertyEditorUi.Toggle",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Checkbox list",
		"id": "fbaf13a8-4036-41f2-93a3-974f678c312a",
		"parent": null,
		"editorAlias": "Umbraco.CheckBoxList",
		"editorUiAlias": "Umb.PropertyEditorUi.CheckBoxList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Dropdown multiple",
		"id": "f38f0ac7-1d27-439c-9f3f-089cd8825a53",
		"parent": null,
		"editorAlias": "Umbraco.DropDown.Flexible",
		"editorUiAlias": "Umb.PropertyEditorUi.Dropdown",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			}
		]
	},
	{
		"name": "Date Picker",
		"id": "5046194e-4237-453c-a547-15db3a07c4e1",
		"parent": null,
		"editorAlias": "Umbraco.DateTime",
		"editorUiAlias": "Umb.PropertyEditorUi.DatePicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "format",
				"value": "YYYY-MM-DD"
			}
		]
	},
	{
		"name": "Radiobox",
		"id": "bb5f57c9-ce2b-4bb9-b697-4caca783a805",
		"parent": null,
		"editorAlias": "Umbraco.RadioButtonList",
		"editorUiAlias": "Umb.PropertyEditorUi.RadioButtonList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Dropdown",
		"id": "0b6a45e7-44ba-430d-9da5-4e46060b9e03",
		"parent": null,
		"editorAlias": "Umbraco.DropDown.Flexible",
		"editorUiAlias": "Umb.PropertyEditorUi.Dropdown",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			}
		]
	},
	{
		"name": "Approved Color",
		"id": "0225af17-b302-49cb-9176-b9f35cab9c17",
		"parent": null,
		"editorAlias": "Umbraco.ColorPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ColorPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Date Picker with time",
		"id": "e4d66c0f-b935-4200-81f0-025f7256b89a",
		"parent": null,
		"editorAlias": "Umbraco.DateTime",
		"editorUiAlias": "Umb.PropertyEditorUi.DatePicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "format",
				"value": "YYYY-MM-DD HH:mm:ss"
			}
		]
	},
	{
		"name": "Tags",
		"id": "b6b73142-b9c1-4bf8-a16d-e1c23320b549",
		"parent": null,
		"editorAlias": "Umbraco.Tags",
		"editorUiAlias": "Umb.PropertyEditorUi.Tags",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "group",
				"value": "default"
			},
			{
				"alias": "storageType",
				"value": "Json"
			}
		]
	},
	{
		"name": "Image Cropper",
		"id": "1df9f033-e6d4-451f-b8d2-e0cbc50a836f",
		"parent": null,
		"editorAlias": "Umbraco.ImageCropper",
		"editorUiAlias": "Umb.PropertyEditorUi.ImageCropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "crops",
				"value": [
					{
						"alias": "one",
						"width": 111,
						"height": 111
					},
					{
						"alias": "two",
						"width": 222,
						"height": 222
					},
					{
						"alias": "three",
						"width": 333,
						"height": 333
					}
				]
			}
		]
	},
	{
		"name": "Content Picker",
		"id": "fd1e0da5-5606-4862-b679-5d0cf3a52a59",
		"parent": null,
		"editorAlias": "Umbraco.ContentPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.DocumentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Member Picker",
		"id": "1ea2e01f-ebd8-4ce1-8d71-6b1149e63548",
		"parent": null,
		"editorAlias": "Umbraco.MemberPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MemberPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Media Picker (legacy)",
		"id": "135d60e0-64d9-49ed-ab08-893c9ba44ae5",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "STRING"
			}
		]
	},
	{
		"name": "Multiple Media Picker (legacy)",
		"id": "9dbbcbbb-2327-434a-b355-af1b84e5010a",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiPicker",
				"value": 1
			},
			{
				"alias": "umbracoDataValueType",
				"value": "STRING"
			}
		]
	},
	{
		"name": "Multi URL Picker",
		"id": "b4e3535a-1753-47e2-8568-602cf8cfee6f",
		"parent": null,
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			}
		]
	},
	{
		"name": "Media Picker",
		"id": "4309a3ea-0d78-4329-a06c-c80b036af19a",
		"parent": null,
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 0,
					"max": 1
				}
			}
		]
	},
	{
		"name": "Multiple Media Picker",
		"id": "1b661f40-2242-4b44-b9cb-3990ee2b13c0",
		"parent": null,
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			}
		]
	},
	{
		"name": "Image Media Picker",
		"id": "ad9f0cf2-bda2-45d5-9ea1-a63cfc873fd3",
		"parent": null,
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "filter",
				"value": "cc07b313-0843-4aa8-bbda-871c8da728c8"
			},
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 0,
					"max": 1
				}
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4673522+00:00"
			}
		]
	},
	{
		"name": "Multiple Image Media Picker",
		"id": "0e63d883-b62b-4799-88c3-157f82e83ecc",
		"parent": null,
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "filter",
				"value": "cc07b313-0843-4aa8-bbda-871c8da728c8"
			},
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4687558+00:00"
			}
		]
	},
	{
		"name": "Document Picker - Default Config",
		"id": "1bd0d68f-8fe9-4906-bb5e-e33eafa83aa3",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ContentPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.DocumentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Document Picker - Show Open Button",
		"id": "8aa44228-5263-4395-9588-9ba401d9e0a1",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ContentPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.DocumentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showOpenButton",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Richtext editor",
		"id": "ca90c950-0aff-4e72-b976-a30b1ac57dad",
		"parent": null,
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.SourceEditor"
						],
						[
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic",
							"Umb.Tiptap.Toolbar.Underline"
						],
						[
							"Umb.Tiptap.Toolbar.TextAlignLeft",
							"Umb.Tiptap.Toolbar.TextAlignCenter",
							"Umb.Tiptap.Toolbar.TextAlignRight"
						],
						[
							"Umb.Tiptap.Toolbar.BulletList",
							"Umb.Tiptap.Toolbar.OrderedList"
						],
						[
							"Umb.Tiptap.Toolbar.Blockquote",
							"Umb.Tiptap.Toolbar.HorizontalRule"
						],
						[
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.Unlink"
						],
						[
							"Umb.Tiptap.Toolbar.MediaPicker",
							"Umb.Tiptap.Toolbar.EmbeddedMedia"
						]
					]
				]
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode",
					"Umb.Tiptap.Block"
				]
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "overlaySize",
				"value": "medium"
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089"
					}
				]
			}
		]
	},
	{
		"name": "Document Picker - Start Node",
		"id": "adcccf89-532f-4a50-83c8-e742035a12a3",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ContentPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.DocumentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "startNodeId",
				"value": "db79156b-3d5b-43d6-ab32-902dc423bec3"
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4875028+00:00"
			}
		]
	},
	{
		"name": "Document Picker - Ignore User Start Nodes",
		"id": "b01b3451-7875-4ac9-a772-b2f23b865af3",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ContentPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.DocumentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			}
		]
	},
	{
		"name": "DateTime Picker - Date Format",
		"id": "59bbc6f4-9515-4264-bad5-66762e50c3d6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DateTime",
		"editorUiAlias": "Umb.PropertyEditorUi.DatePicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "format",
				"value": "YYYY-MM-DD"
			},
			{
				"alias": "offsetTime",
				"value": false
			}
		]
	},
	{
		"name": "DateTime Picker - Date + Time Format",
		"id": "e8602b3f-8b89-4f91-8c94-d2fd7df534e6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DateTime",
		"editorUiAlias": "Umb.PropertyEditorUi.DatePicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "format",
				"value": "YYYY-MM-DD HH:mm:ss"
			},
			{
				"alias": "offsetTime",
				"value": false
			}
		]
	},
	{
		"name": "DateTime Picker - Offset Time",
		"id": "3773d64f-495d-4e88-9ec4-7eb58c334687",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DateTime",
		"editorUiAlias": "Umb.PropertyEditorUi.DatePicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "format",
				"value": "YYYY-MM-DD HH:mm:ss"
			},
			{
				"alias": "offsetTime",
				"value": true
			}
		]
	},
	{
		"name": "Decimal - Default Config",
		"id": "bed85b43-5d17-4676-9cf7-56bd193a053b",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Decimal",
		"editorUiAlias": "Umb.PropertyEditorUi.Decimal",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "max",
				"value": 100
			},
			{
				"alias": "step",
				"value": 0.01
			}
		]
	},
	{
		"name": "Decimal - Fully Configured",
		"id": "9ca94289-e14d-460a-bfd1-cb958ed90da5",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Decimal",
		"editorUiAlias": "Umb.PropertyEditorUi.Decimal",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "min",
				"value": 10.5
			},
			{
				"alias": "step",
				"value": 0.5
			},
			{
				"alias": "max",
				"value": 100
			},
			{
				"alias": "placeholder",
				"value": "Enter a decimal..."
			}
		]
	},
	{
		"name": "Dropdown - Single Value",
		"id": "3c1f48e0-6eec-44f3-8072-3e22d442a0a0",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DropDown.Flexible",
		"editorUiAlias": "Umb.PropertyEditorUi.Dropdown",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "items",
				"value": [
					"One",
					"Two",
					"Three",
					"Four",
					"Five",
					"Six",
					"Seven",
					"Eight",
					"Nine",
					"Ten Ten Ten Ten Ten Ten Ten Ten Ten"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4984291+00:00"
			}
		]
	},
	{
		"name": "Dropdown - Multi Value",
		"id": "779051c2-7bb7-4ab4-82ac-698faa8286aa",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DropDown.Flexible",
		"editorUiAlias": "Umb.PropertyEditorUi.Dropdown",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "items",
				"value": [
					"One",
					"Two",
					"Three",
					"Four",
					"Five",
					"Six",
					"Seven",
					"Eight",
					"Nine",
					"Ten Ten Ten Ten Ten Ten Ten Ten Ten"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4992805+00:00"
			}
		]
	},
	{
		"name": "Eye Dropper Color Picker - Default Config",
		"id": "b9484af9-4a64-4aa0-a200-2cfd344c1aa5",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ColorPicker.EyeDropper",
		"editorUiAlias": "Umb.PropertyEditorUi.EyeDropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showAlpha",
				"value": false
			},
			{
				"alias": "showPalette",
				"value": false
			}
		]
	},
	{
		"name": "Eye Dropper Color Picker - Alpha",
		"id": "36c5e4da-fdf5-4ad7-8a1a-30bcaf453cc8",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ColorPicker.EyeDropper",
		"editorUiAlias": "Umb.PropertyEditorUi.EyeDropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showAlpha",
				"value": true
			},
			{
				"alias": "showPalette",
				"value": false
			}
		]
	},
	{
		"name": "Eye Dropper Color Picker - Palette",
		"id": "dcc7e763-4715-47ad-9215-a626e33a4a9f",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ColorPicker.EyeDropper",
		"editorUiAlias": "Umb.PropertyEditorUi.EyeDropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showAlpha",
				"value": false
			},
			{
				"alias": "showPalette",
				"value": true
			}
		]
	},
	{
		"name": "Eye Dropper Color Picker - Fully Configured",
		"id": "fb266db1-3e18-4cf2-9cf6-6f5096b12076",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ColorPicker.EyeDropper",
		"editorUiAlias": "Umb.PropertyEditorUi.EyeDropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "showAlpha",
				"value": true
			},
			{
				"alias": "showPalette",
				"value": true
			}
		]
	},
	{
		"name": "Markdown Editor - Default Config",
		"id": "4f14fe46-522a-4994-ad67-451f78c5d8f6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MarkdownEditor",
		"editorUiAlias": "Umb.PropertyEditorUi.MarkdownEditor",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "preview",
				"value": false
			},
			{
				"alias": "overlaySize",
				"value": "small"
			}
		]
	},
	{
		"name": "Markdown Editor - Preview",
		"id": "a5b1b0d5-f905-413f-8e00-aeb8bf4c0999",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MarkdownEditor",
		"editorUiAlias": "Umb.PropertyEditorUi.MarkdownEditor",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "preview",
				"value": true
			},
			{
				"alias": "overlaySize",
				"value": "small"
			}
		]
	},
	{
		"name": "Markdown Editor - Default Value",
		"id": "e56554ed-95a4-43c4-96b4-0122569f4193",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MarkdownEditor",
		"editorUiAlias": "Umb.PropertyEditorUi.MarkdownEditor",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "preview",
				"value": false
			},
			{
				"alias": "defaultValue",
				"value": "*This* is the _default_ value!\n\n- List item one\n- List item two"
			},
			{
				"alias": "overlaySize",
				"value": "small"
			}
		]
	},
	{
		"name": "Markdown Editor - Large Overlay Size",
		"id": "9450742c-31ce-4db2-85d5-c85238473c39",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MarkdownEditor",
		"editorUiAlias": "Umb.PropertyEditorUi.MarkdownEditor",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "preview",
				"value": false
			},
			{
				"alias": "overlaySize",
				"value": "large"
			}
		]
	},
	{
		"name": "Markdown Editor - Fully Configured",
		"id": "3af82553-4d1e-42c4-9e93-880de599c7b3",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MarkdownEditor",
		"editorUiAlias": "Umb.PropertyEditorUi.MarkdownEditor",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "preview",
				"value": true
			},
			{
				"alias": "defaultValue",
				"value": "*This* is the _default_ value!\nSuper duper default value.\n\n- List item one\n- List item two\n\n## HEADING!\n\nMore text"
			},
			{
				"alias": "overlaySize",
				"value": "medium"
			}
		]
	},
	{
		"name": "Media Picker - Default Config",
		"id": "87543f25-f2dc-41b4-b861-75159b7baff9",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Media Picker - Accepted Types",
		"id": "ccff0509-5a13-4e86-a341-9b91868922ae",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "filter",
				"value": "cc07b313-0843-4aa8-bbda-871c8da728c8"
			},
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5008226+00:00"
			}
		]
	},
	{
		"name": "Media Picker - Pick Multiple Items",
		"id": "fec81092-5db6-45b2-8050-09dcfead2901",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Media Picker - Amount",
		"id": "ff53e2ac-d54b-4c78-a5cc-2bc050605854",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 2,
					"max": 10
				}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Media Picker - Pick Multiple Items, With Amount",
		"id": "efe5ab7e-e78a-4b3c-8fe2-3c711e073a5d",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 2,
					"max": 10
				}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Media Picker - Start Node",
		"id": "c686c86b-8941-4656-8f24-eea0afb0abb9",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 1,
					"max": 50
				}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5015767+00:00"
			},
			{
				"alias": "filter",
				"value": "cc07b313-0843-4aa8-bbda-871c8da728c8"
			},
			{
				"alias": "startNodeId",
				"value": "5deac19f-5ca8-4b8c-a784-26593cec8d51"
			}
		]
	},
	{
		"name": "Media Picker - Focal Point",
		"id": "a6089297-6132-4f18-83da-bf11acb5e8e2",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Media Picker - Crops",
		"id": "0e33a2de-d6a3-4853-8a68-7f56667f1d75",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": true
			},
			{
				"alias": "crops",
				"value": [
					{
						"alias": "one",
						"label": "One",
						"width": 100,
						"height": 100
					},
					{
						"alias": "two",
						"label": "Two",
						"width": 200,
						"height": 200
					}
				]
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Media Picker - Ignore User Start Nodes",
		"id": "7c67bee1-7206-40c3-a8f3-cdad500c021b",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			}
		]
	},
	{
		"name": "Textstring",
		"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
		"parent": null,
		"editorAlias": "Umbraco.TextBox",
		"editorUiAlias": "Umb.PropertyEditorUi.TextBox",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Media Picker - Fully Configured",
		"id": "30f7ee63-69da-4b1f-a8f8-4c5ed882be17",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MediaPicker3",
		"editorUiAlias": "Umb.PropertyEditorUi.MediaPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "filter",
				"value": "cc07b313-0843-4aa8-bbda-871c8da728c8,a43e3414-9599-4230-a7d3-943a21b20122"
			},
			{
				"alias": "multiple",
				"value": true
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 2,
					"max": 10
				}
			},
			{
				"alias": "startNodeId",
				"value": "5deac19f-5ca8-4b8c-a784-26593cec8d51"
			},
			{
				"alias": "enableLocalFocalPoint",
				"value": true
			},
			{
				"alias": "crops",
				"value": [
					{
						"alias": "one",
						"label": "One",
						"width": 100,
						"height": 100
					},
					{
						"alias": "two",
						"label": "Two",
						"width": 200,
						"height": 200
					}
				]
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5023121+00:00"
			}
		]
	},
	{
		"name": "Textarea",
		"id": "c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3",
		"parent": null,
		"editorAlias": "Umbraco.TextArea",
		"editorUiAlias": "Umb.PropertyEditorUi.TextArea",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minHeight",
				"value": 0
			},
			{
				"alias": "maxHeight",
				"value": 0
			}
		]
	},
	{
		"name": "Member Group Picker",
		"id": "2ac54465-7f8c-481e-926b-6fcc8bef1dc3",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MemberGroupPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MemberGroupPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Upload File",
		"id": "84c6b441-31df-4ffe-b67e-67d5bc3ae65a",
		"parent": null,
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Member Picker (default)",
		"id": "2555acc6-6adf-4cc3-b0bd-86a2dfdcc7b1",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MemberPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MemberPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Upload Video",
		"id": "70575fe7-9812-4396-bbe1-c81a76db71b5",
		"parent": null,
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "fileExtensions",
				"value": [
					"mp4",
					"webm",
					"ogv",
					"mpeg"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4233679+00:00"
			}
		]
	},
	{
		"name": "Label (string)",
		"id": "f0bc4bfb-b499-40d6-ba86-058885a5178c",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "STRING"
			},
			{
				"alias": "labelTemplate",
				"value": ""
			}
		]
	},
	{
		"name": "Multi URL Picker - Default Config",
		"id": "f455a80c-7f39-4fbb-b212-cf829dd28f7b",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "overlaySize",
				"value": "small"
			},
			{
				"alias": "hideAnchor",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Upload Audio",
		"id": "8f430dd6-4e96-447e-9dc0-cb552c8cd1f3",
		"parent": null,
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "fileExtensions",
				"value": [
					"mp3",
					"weba",
					"oga",
					"opus",
					"wav"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4227255+00:00"
			}
		]
	},
	{
		"name": "Label (bigint)",
		"id": "930861bf-e262-4ead-a704-f99453565708",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "BIGINT"
			},
			{
				"alias": "labelTemplate",
				"value": "{=value | bytes}"
			}
		]
	},
	{
		"name": "Label (integer)",
		"id": "8e7f995c-bd81-4627-9932-c40e568ec788",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "INT"
			},
			{
				"alias": "labelTemplate",
				"value": "{=value}px"
			}
		]
	},
	{
		"name": "Multi URL Picker - Min And Max",
		"id": "5efb7a21-cbfa-452b-a7f0-9b6e467f651c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 5
			},
			{
				"alias": "maxNumber",
				"value": 10
			},
			{
				"alias": "overlaySize",
				"value": "full"
			},
			{
				"alias": "hideAnchor",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Upload Article",
		"id": "bc1e266c-dac4-4164-bf08-8a1ec6a7143d",
		"parent": null,
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "fileExtensions",
				"value": [
					"pdf",
					"docx",
					"doc"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4219669+00:00"
			}
		]
	},
	{
		"name": "Label (datetime)",
		"id": "0e9794eb-f9b5-4f20-a788-93acd233a7e4",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "DATETIME"
			}
		]
	},
	{
		"name": "Multi URL Picker - Large Overlay Size",
		"id": "d6d131e2-822f-437a-a1bc-57ddf55e9a5f",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "overlaySize",
				"value": "large"
			},
			{
				"alias": "hideAnchor",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "Upload Vector Graphics",
		"id": "215cb418-2153-4429-9aef-8c0f0041191b",
		"parent": null,
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "fileExtensions",
				"value": [
					"svg"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.4173378+00:00"
			}
		]
	},
	{
		"name": "Label (time)",
		"id": "a97cec69-9b71-4c30-8b12-ec398860d7e8",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "TIME"
			}
		]
	},
	{
		"name": "Multi URL Picker - Hide Anchor Query String",
		"id": "f088d56e-9efd-4c4d-8264-accbbe647181",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "overlaySize",
				"value": "small"
			},
			{
				"alias": "hideAnchor",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			}
		]
	},
	{
		"name": "[_] Tiptap Rich Text Editor 2",
		"id": "720e2055-413e-4f92-8187-f9ea44953531",
		"parent": null,
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "overlaySize",
				"value": "medium"
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.WordCount",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode"
				]
			},
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.SourceEditor"
						],
						[
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic"
						],
						[
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.Unlink"
						],
						[
							"Umb.Tiptap.Toolbar.BlockPicker"
						]
					]
				]
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089",
						"displayInline": true
					},
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"label": "Element Two",
						"editorSize": "large"
					}
				]
			},
			{
				"alias": "stylesheets",
				"value": [
					"/tiptap-demo.css"
				]
			}
		]
	},
	{
		"name": "Label (decimal)",
		"id": "8f1ef1e1-9de4-40d3-a072-6673f631ca64",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "DECIMAL"
			}
		]
	},
	{
		"name": "Multi URL Picker - Ignore User Start Nodes",
		"id": "50a7ce3b-ba5a-4f20-8264-361d2194a15c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "overlaySize",
				"value": "small"
			},
			{
				"alias": "hideAnchor",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			}
		]
	},
	{
		"name": "[_] Content Picker",
		"id": "d984ed89-80cf-4f54-8002-d5665a1d3035",
		"parent": null,
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": true
			},
			{
				"alias": "startNode",
				"value": {
					"type": "member"
				}
			},
			{
				"alias": "filter",
				"value": ""
			}
		]
	},
	{
		"name": "Label (bytes)",
		"id": "ba5bdbe6-ab3e-46a8-82b3-2c45f10bc47f",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "BIGINT"
			},
			{
				"alias": "labelTemplate",
				"value": "{=value | bytes}"
			}
		]
	},
	{
		"name": "Multi URL Picker - Fully Configured",
		"id": "68bee672-9317-40a4-860c-32240d4a2926",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiUrlPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.MultiUrlPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 2
			},
			{
				"alias": "maxNumber",
				"value": 10
			},
			{
				"alias": "overlaySize",
				"value": "medium"
			},
			{
				"alias": "hideAnchor",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			}
		]
	},
	{
		"name": "[_] Tiptap Rich Text Editor",
		"id": "3661aaf8-6e63-4005-aaa5-ea2245f36c7d",
		"parent": null,
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.SourceEditor",
							"Umb.Tiptap.Toolbar.Undo",
							"Umb.Tiptap.Toolbar.Redo"
						],
						[
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic",
							"Umb.Tiptap.Toolbar.Underline",
							"Umb.Tiptap.Toolbar.Strike"
						],
						[
							"Umb.Tiptap.Toolbar.TextAlignLeft",
							"Umb.Tiptap.Toolbar.TextAlignCenter",
							"Umb.Tiptap.Toolbar.TextAlignRight",
							"Umb.Tiptap.Toolbar.TextAlignJustify"
						],
						[
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.FontFamily",
							"Umb.Tiptap.Toolbar.FontSize"
						],
						[
							"Umb.Tiptap.Toolbar.CharacterMap",
							"Umb.Tiptap.Toolbar.ClearFormatting"
						],
						[
							"Umb.Tiptap.Toolbar.Anchor",
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.Unlink"
						],
						[
							"Umb.Tiptap.Toolbar.TextColorForeground",
							"Umb.Tiptap.Toolbar.TextColorBackground"
						]
					],
					[
						[
							"Umb.Tiptap.Toolbar.Heading1",
							"Umb.Tiptap.Toolbar.Heading2",
							"Umb.Tiptap.Toolbar.Heading3",
							"Umb.Tiptap.Toolbar.Heading4",
							"Umb.Tiptap.Toolbar.Heading5",
							"Umb.Tiptap.Toolbar.Heading6",
							"Umb.Tiptap.Toolbar.Heading6"
						],
						[
							"Umb.Tiptap.Toolbar.BulletList",
							"Umb.Tiptap.Toolbar.OrderedList",
							"Umb.Tiptap.Toolbar.Blockquote"
						],
						[
							"Umb.Tiptap.Toolbar.Subscript",
							"Umb.Tiptap.Toolbar.Superscript"
						],
						[
							"Umb.Tiptap.Toolbar.CodeBlock",
							"Umb.Tiptap.Toolbar.HorizontalRule"
						],
						[
							"Umb.Tiptap.Toolbar.TextDirectionRtl",
							"Umb.Tiptap.Toolbar.TextDirectionLtr"
						],
						[
							"Umb.Tiptap.Toolbar.TextIndent",
							"Umb.Tiptap.Toolbar.TextOutdent",
							"Umb.Tiptap.Toolbar.TextOutdent"
						],
						[
							"Umb.Tiptap.Toolbar.Table",
							"Umb.Tiptap.Toolbar.MediaPicker",
							"Umb.Tiptap.Toolbar.EmbeddedMedia",
							"Umb.Tiptap.Toolbar.BlockPicker"
						]
					]
				]
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "overlaySize",
				"value": "medium"
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.WordCount",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.TrailingNode",
					"Umb.Tiptap.Table"
				]
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089"
					}
				]
			},
			{
				"alias": "dimensions",
				"value": {
					"width": 0,
					"height": 0
				}
			}
		]
	},
	{
		"name": "[Dynamic Root Test] Category Picker",
		"id": "304ce88f-306e-4ee5-a21b-79f638e4eb15",
		"parent": null,
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"dynamicRoot": {
						"originAlias": "Root",
						"querySteps": [
							{
								"unique": "a506599f-50a2-42c2-895e-e16cedaad6c0",
								"alias": "NearestDescendantOrSelf",
								"anyOfDocTypeKeys": [
									"651c932b-232d-42b3-a52e-c1399fcff9bc"
								]
							}
						]
					}
				}
			},
			{
				"alias": "filter",
				"value": "5d53249e-330c-4527-a3fb-2ddab802305c"
			}
		]
	},
	{
		"name": "Label (pixels)",
		"id": "5eb57825-e15e-4fc7-8e37-fca65cdafbde",
		"parent": null,
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "INT"
			},
			{
				"alias": "labelTemplate",
				"value": "{=value}px"
			}
		]
	},
	{
		"name": "Content Picker - Default Config",
		"id": "fe2a2728-c6bc-450b-9e63-a68d60638b7e",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": null
				}
			},
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5059189+00:00"
			}
		]
	},
	{
		"name": "Content Picker - Min And Max",
		"id": "07af09f9-819e-404b-b012-226e8978f5c0",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": null,
					"dynamicRoot": null
				}
			},
			{
				"alias": "minNumber",
				"value": 2
			},
			{
				"alias": "maxNumber",
				"value": 5
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5137914+00:00"
			}
		]
	},
	{
		"name": "Content Picker - Allowed Types",
		"id": "58269615-9b07-481d-aa44-0613b9157f3e",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": null,
					"dynamicRoot": null
				}
			},
			{
				"alias": "filter",
				"value": "7184285e-9709-4e13-8c72-1fe52f024b28"
			},
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.514565+00:00"
			}
		]
	},
	{
		"name": "[_] Block List Document Type - Block List",
		"id": "29c12c36-f409-4ecd-8976-0bbe01157ebf",
		"parent": null,
		"editorAlias": "Umbraco.BlockList",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "511fef8a-5e2e-42b9-8312-0a6034881793",
						"thumbnail": [
							"/App_Plugins/lkeTest/temp2.jpg",
							"/App_Plugins/lkeTest/temp.jpg",
							"/App_Plugins/lkeTest/umbraco-package.json"
						]
					}
				]
			},
			{
				"alias": "useInlineEditingAsDefault",
				"value": true
			}
		]
	},
	{
		"name": "ProperyEditorInBlockName",
		"id": "0a97b6b8-8509-4367-b550-9ff870763289",
		"parent": null,
		"editorAlias": "Umbraco.DropDown.Flexible",
		"editorUiAlias": "Umb.PropertyEditorUi.Dropdown",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "multiple",
				"value": false
			},
			{
				"alias": "items",
				"value": [
					"testOption1",
					"testOption2"
				]
			}
		]
	},
	{
		"name": "Content Picker - Fully Configured",
		"id": "98e2775d-37f6-4625-98f6-1a4cd4f7dac8",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": "db79156b-3d5b-43d6-ab32-902dc423bec3",
					"dynamicRoot": null
				}
			},
			{
				"alias": "filter",
				"value": "015bc281-7410-40e2-81b5-b8f7c963bd61,41f34bb7-fd63-442f-8dcb-142df4246310,13c10f78-bf14-411d-9444-751e4bd1b178,9cff8f66-0e13-4617-ab9b-9f845ecc5e24"
			},
			{
				"alias": "minNumber",
				"value": 2
			},
			{
				"alias": "maxNumber",
				"value": 5
			},
			{
				"alias": "showOpenButton",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5158583+00:00"
			}
		]
	},
	{
		"name": "[_] Block List Document Type - Block List (copy)",
		"id": "aacdffb3-dd78-4427-af92-38100fad60cc",
		"parent": null,
		"editorAlias": "Umbraco.BlockList",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "511fef8a-5e2e-42b9-8312-0a6034881793",
						"editorSize": "medium"
					}
				]
			},
			{
				"alias": "useInlineEditingAsDefault",
				"value": false
			}
		]
	},
	{
		"name": "Content Picker - Start Node",
		"id": "75800052-b5c4-4a82-a6ed-a7972129bcf6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": "db79156b-3d5b-43d6-ab32-902dc423bec3",
					"dynamicRoot": null
				}
			},
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5170384+00:00"
			}
		]
	},
	{
		"name": "Content Picker - Media - Default Config",
		"id": "52d20340-cf21-4256-a136-55f91dbf353a",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "media",
					"id": null,
					"dynamicRoot": null
				}
			},
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.517664+00:00"
			}
		]
	},
	{
		"name": "[_] Entity Data Picker",
		"id": "68d8f7be-d93d-4fbe-951c-cad6c301ca76",
		"parent": null,
		"editorAlias": "Umbraco.Plain.Json",
		"editorUiAlias": "Umb.PropertyEditorUi.Plain.Json",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbEditorDataSource",
				"value": "Umb.PropertyEditorDataSource.UserPicker"
			},
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": "04e23c98-30e1-46e8-bd93-5a38b1a6e90a",
					"dynamicRoot": {
						"originKey": "04e23c98-30e1-46e8-bd93-5a38b1a6e90a",
						"originAlias": "ByKey"
					}
				}
			}
		]
	},
	{
		"name": "Content Picker - Media - Fully Configured",
		"id": "67782cbb-ee87-42cf-810a-7aab4b1cbcbc",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "media",
					"id": "5deac19f-5ca8-4b8c-a784-26593cec8d51",
					"dynamicRoot": null
				}
			},
			{
				"alias": "filter",
				"value": "cc07b313-0843-4aa8-bbda-871c8da728c8,a43e3414-9599-4230-a7d3-943a21b20122"
			},
			{
				"alias": "minNumber",
				"value": 1
			},
			{
				"alias": "maxNumber",
				"value": 10
			},
			{
				"alias": "showOpenButton",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5183084+00:00"
			}
		]
	},
	{
		"name": "Content Picker - XPath Start Node",
		"id": "612a8051-191c-4cbc-827f-aae9ec364fbc",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"id": null,
					"dynamicRoot": null
				}
			},
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5189195+00:00"
			}
		]
	},
	{
		"name": "Content Picker - Members - Default Config",
		"id": "9f1be990-9b28-4817-a662-841875071769",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "member",
					"id": null,
					"dynamicRoot": null
				}
			},
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "showOpenButton",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5195446+00:00"
			}
		]
	},
	{
		"name": "Content Picker - Members - Fully Configured",
		"id": "f6d4679e-2090-4acb-b7c8-564d908c657c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "startNode",
				"value": {
					"type": "member",
					"id": null,
					"dynamicRoot": null
				}
			},
			{
				"alias": "filter",
				"value": "d59be02f-1df9-4228-aa1e-01917d806cda"
			},
			{
				"alias": "minNumber",
				"value": 2
			},
			{
				"alias": "maxNumber",
				"value": 5
			},
			{
				"alias": "showOpenButton",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.520467+00:00"
			}
		]
	},
	{
		"name": "Numeric - Default Config",
		"id": "fb9142d1-ce19-40dc-84cf-5f81edb11928",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Integer",
		"editorUiAlias": "Umb.PropertyEditorUi.Integer",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Numeric - Min And Max",
		"id": "1b026e4b-1486-44e1-91c5-62898e813036",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Integer",
		"editorUiAlias": "Umb.PropertyEditorUi.Integer",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "min",
				"value": 10
			},
			{
				"alias": "max",
				"value": 100
			}
		]
	},
	{
		"name": "Numeric - Step Size",
		"id": "5638868a-49df-44b5-8c05-42ca95d8476b",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Integer",
		"editorUiAlias": "Umb.PropertyEditorUi.Integer",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "step",
				"value": 4
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "mode",
				"value": "Classic"
			},
			{
				"alias": "toolbar",
				"value": [
					"styles",
					"bold",
					"italic",
					"alignleft",
					"aligncenter",
					"alignright",
					"bullist",
					"numlist",
					"outdent",
					"indent",
					"sourcecode",
					"link",
					"umbmediapicker",
					"umbembeddialog"
				]
			}
		]
	},
	{
		"name": "Numeric - Fully Configured",
		"id": "4e7ffcd3-e8c6-450f-b5dc-c1bdcd234292",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Integer",
		"editorUiAlias": "Umb.PropertyEditorUi.Integer",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "min",
				"value": 10
			},
			{
				"alias": "step",
				"value": 6
			},
			{
				"alias": "max",
				"value": 70
			}
		]
	},
	{
		"name": "Radio Button List",
		"id": "2d3a109a-de8d-4c12-af67-4f7a116cebe5",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RadioButtonList",
		"editorUiAlias": "Umb.PropertyEditorUi.RadioButtonList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "items",
				"value": [
					"One",
					"Two",
					"Three"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5212223+00:00"
			}
		]
	},
	{
		"name": "Multiple Textstring - Default Config",
		"id": "a9c636c7-d500-4ce5-bfb1-2d508fe79d7c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultipleTextstring",
		"editorUiAlias": "Umb.PropertyEditorUi.MultipleTextString",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.522158+00:00"
			},
			{
				"alias": "min",
				"value": 5
			},
			{
				"alias": "max",
				"value": 2
			}
		]
	},
	{
		"name": "Multiple Textstring - Min",
		"id": "26ecc485-c84f-4806-9445-3996da82d0bb",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultipleTextstring",
		"editorUiAlias": "Umb.PropertyEditorUi.MultipleTextString",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5227415+00:00"
			},
			{
				"alias": "min",
				"value": 10
			},
			{
				"alias": "max",
				"value": 0
			}
		]
	},
	{
		"name": "Multiple Textstring - Max",
		"id": "b88cc0ae-9216-45de-83c2-5e92de3ae153",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultipleTextstring",
		"editorUiAlias": "Umb.PropertyEditorUi.MultipleTextString",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5233042+00:00"
			},
			{
				"alias": "min",
				"value": 0
			},
			{
				"alias": "max",
				"value": 10
			}
		]
	},
	{
		"name": "Multiple Textstring - Fully Configured",
		"id": "78350acf-b981-4a55-96f8-a91001c73eef",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultipleTextstring",
		"editorUiAlias": "Umb.PropertyEditorUi.MultipleTextString",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5238592+00:00"
			},
			{
				"alias": "min",
				"value": 5
			},
			{
				"alias": "max",
				"value": 50
			}
		]
	},
	{
		"name": "Rich Text Editor - Default Config",
		"id": "e9f410d1-1f37-401d-b3b3-4678e4aab5fa",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5334155+00:00"
			},
			{
				"alias": "overlaySize",
				"value": "small"
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode"
				]
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic",
							"Umb.Tiptap.Toolbar.TextAlignLeft",
							"Umb.Tiptap.Toolbar.TextAlignCenter",
							"Umb.Tiptap.Toolbar.TextAlignRight",
							"Umb.Tiptap.Toolbar.BulletList",
							"Umb.Tiptap.Toolbar.OrderedList",
							"Umb.Tiptap.Toolbar.TextOutdent",
							"Umb.Tiptap.Toolbar.TextIndent",
							"Umb.Tiptap.Toolbar.Anchor",
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.Unlink",
							"Umb.Tiptap.Toolbar.SourceEditor",
							"Umb.Tiptap.Toolbar.MediaPicker",
							"Umb.Tiptap.Toolbar.EmbeddedMedia"
						]
					]
				]
			},
			{
				"alias": "maxImageSize",
				"value": 500
			}
		]
	},
	{
		"name": "Rich Text Editor - Stylesheets",
		"id": "ae438007-05c6-4692-afc8-b86201a08882",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.534367+00:00"
			},
			{
				"alias": "overlaySize",
				"value": "small"
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode"
				]
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "stylesheets",
				"value": [
					"/fonts.css",
					"/rte-styles.css"
				]
			},
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic"
						]
					]
				]
			}
		]
	},
	{
		"name": "Rich Text Editor - Dimensions",
		"id": "b005391c-4735-4967-9c16-939dc846acac",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.536146+00:00"
			},
			{
				"alias": "overlaySize",
				"value": "small"
			},
			{
				"alias": "hideLabel",
				"value": false
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": false
			},
			{
				"alias": "mode",
				"value": "Classic"
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "dimensions",
				"value": {
					"width": 800,
					"height": 400
				}
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode"
				]
			}
		]
	},
	{
		"name": "Rich Text Editor - Fully Configured",
		"id": "dcede488-62e2-49ba-97a0-59c60ae09992",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "dimensions",
				"value": {
					"height": 500,
					"width": 800
				}
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode"
				]
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			},
			{
				"alias": "maxImageSize",
				"value": 0
			},
			{
				"alias": "mediaParentId",
				"value": "5deac19f-5ca8-4b8c-a784-26593cec8d51"
			},
			{
				"alias": "overlaySize",
				"value": "large"
			},
			{
				"alias": "stylesheets",
				"value": [
					"/css/fonts.css",
					"/css/rte-styles.css"
				]
			},
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.Undo",
							"Umb.Tiptap.Toolbar.Redo",
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.FontFamily",
							"Umb.Tiptap.Toolbar.FontSize",
							"Umb.Tiptap.Toolbar.TextColorForeground",
							"Umb.Tiptap.Toolbar.TextColorBackground",
							"Umb.Tiptap.Toolbar.Blockquote",
							"Umb.Tiptap.Toolbar.ClearFormatting",
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic",
							"Umb.Tiptap.Toolbar.Underline",
							"Umb.Tiptap.Toolbar.Strike",
							"Umb.Tiptap.Toolbar.TextAlignLeft",
							"Umb.Tiptap.Toolbar.TextAlignCenter",
							"Umb.Tiptap.Toolbar.TextAlignRight",
							"Umb.Tiptap.Toolbar.TextAlignJustify",
							"Umb.Tiptap.Toolbar.BulletList",
							"Umb.Tiptap.Toolbar.OrderedList",
							"Umb.Tiptap.Toolbar.TextOutdent",
							"Umb.Tiptap.Toolbar.TextIndent",
							"Umb.Tiptap.Toolbar.Anchor",
							"Umb.Tiptap.Toolbar.Table",
							"Umb.Tiptap.Toolbar.HorizontalRule",
							"Umb.Tiptap.Toolbar.Subscript",
							"Umb.Tiptap.Toolbar.Superscript",
							"Umb.Tiptap.Toolbar.CharacterMap",
							"Umb.Tiptap.Toolbar.TextDirectionRtl",
							"Umb.Tiptap.Toolbar.TextDirectionLtr",
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.Unlink",
							"Umb.Tiptap.Toolbar.SourceEditor",
							"Umb.Tiptap.Toolbar.MediaPicker",
							"Umb.Tiptap.Toolbar.EmbeddedMedia",
							"Umb.Tiptap.Toolbar.BlockPicker"
						]
					]
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5389322+00:00"
			}
		]
	},
	{
		"name": "Slider - Default Config",
		"id": "afc36215-ae90-4290-bf1a-af6ce0ca3c67",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Slider",
		"editorUiAlias": "Umb.PropertyEditorUi.Slider",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "enableRange",
				"value": false
			},
			{
				"alias": "initVal1",
				"value": 0
			},
			{
				"alias": "initVal2",
				"value": 0
			},
			{
				"alias": "minVal",
				"value": 0
			},
			{
				"alias": "maxVal",
				"value": 100
			},
			{
				"alias": "step",
				"value": 0
			}
		]
	},
	{
		"name": "Slider - Min And Max",
		"id": "bf4179df-26ec-4c84-9607-0071785730dc",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Slider",
		"editorUiAlias": "Umb.PropertyEditorUi.Slider",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "enableRange",
				"value": false
			},
			{
				"alias": "initVal1",
				"value": 0
			},
			{
				"alias": "initVal2",
				"value": 0
			},
			{
				"alias": "minVal",
				"value": 5
			},
			{
				"alias": "maxVal",
				"value": 100
			},
			{
				"alias": "step",
				"value": 0
			}
		]
	},
	{
		"name": "Slider - Step Increments",
		"id": "17d1054c-423a-4f89-a917-cbc055af6fa9",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Slider",
		"editorUiAlias": "Umb.PropertyEditorUi.Slider",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "enableRange",
				"value": false
			},
			{
				"alias": "initVal1",
				"value": 0
			},
			{
				"alias": "initVal2",
				"value": 0
			},
			{
				"alias": "minVal",
				"value": 0
			},
			{
				"alias": "maxVal",
				"value": 100
			},
			{
				"alias": "step",
				"value": 10
			}
		]
	},
	{
		"name": "Slider - Initial Value",
		"id": "1158cb90-ad8e-4c62-9ed3-085691d61a39",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Slider",
		"editorUiAlias": "Umb.PropertyEditorUi.Slider",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "enableRange",
				"value": false
			},
			{
				"alias": "initVal1",
				"value": 6
			},
			{
				"alias": "initVal2",
				"value": 0
			},
			{
				"alias": "minVal",
				"value": 0
			},
			{
				"alias": "maxVal",
				"value": 100
			},
			{
				"alias": "step",
				"value": 0
			}
		]
	},
	{
		"name": "Slider - Fully Configured",
		"id": "dae71dda-3838-459e-b018-c432952288b0",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Slider",
		"editorUiAlias": "Umb.PropertyEditorUi.Slider",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "enableRange",
				"value": true
			},
			{
				"alias": "initVal1",
				"value": 10
			},
			{
				"alias": "initVal2",
				"value": 30
			},
			{
				"alias": "minVal",
				"value": 5
			},
			{
				"alias": "maxVal",
				"value": 100
			},
			{
				"alias": "step",
				"value": 1
			}
		]
	},
	{
		"name": "Textarea - Default Config",
		"id": "4a0ca16a-71bc-471b-b002-5a4b1999d2fa",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextArea",
		"editorUiAlias": "Umb.PropertyEditorUi.TextArea",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "rows",
				"value": 10
			}
		]
	},
	{
		"name": "Textarea - Max Chars",
		"id": "55dcc374-b218-450d-90c3-db3d163dde02",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextArea",
		"editorUiAlias": "Umb.PropertyEditorUi.TextArea",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "maxChars",
				"value": 100
			},
			{
				"alias": "rows",
				"value": 0
			},
			{
				"alias": "maxHeight",
				"value": 250
			}
		]
	},
	{
		"name": "Textarea - Rows",
		"id": "0afa9c4b-e2f2-450b-bb13-2e2684a5fe65",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextArea",
		"editorUiAlias": "Umb.PropertyEditorUi.TextArea",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "rows",
				"value": 60
			}
		]
	},
	{
		"name": "Textarea - Fully Configured",
		"id": "0fa454ae-8671-4c69-a754-7ee738bab707",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextArea",
		"editorUiAlias": "Umb.PropertyEditorUi.TextArea",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "maxChars",
				"value": 200
			},
			{
				"alias": "rows",
				"value": 40
			}
		]
	},
	{
		"name": "Textbox - Default Config",
		"id": "3a9b04b9-e96a-46d0-bd84-2c13fc36b70c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextBox",
		"editorUiAlias": "Umb.PropertyEditorUi.TextBox",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Textbox - Max Chars",
		"id": "75c0a6a9-61a4-49e2-a8a9-5f19b72760e5",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextBox",
		"editorUiAlias": "Umb.PropertyEditorUi.TextBox",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "maxChars",
				"value": 150
			}
		]
	},
	{
		"name": "Toggle - Default Config",
		"id": "b6c1b8fc-7aaf-4177-b916-44cda751436d",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TrueFalse",
		"editorUiAlias": "Umb.PropertyEditorUi.Toggle",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "default",
				"value": false
			},
			{
				"alias": "showLabels",
				"value": false
			}
		]
	},
	{
		"name": "Toggle - Initial State",
		"id": "768f8670-982d-4911-886c-6f97fedf022a",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TrueFalse",
		"editorUiAlias": "Umb.PropertyEditorUi.Toggle",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "default",
				"value": true
			},
			{
				"alias": "showLabels",
				"value": false
			}
		]
	},
	{
		"name": "Toggle - Labels",
		"id": "1522cae7-487c-427c-9bd1-aecc5eb7fab9",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TrueFalse",
		"editorUiAlias": "Umb.PropertyEditorUi.Toggle",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "default",
				"value": false
			},
			{
				"alias": "showLabels",
				"value": true
			},
			{
				"alias": "labelOn",
				"value": "Toggle is ON"
			},
			{
				"alias": "labelOff",
				"value": "Toggle is OFF"
			}
		]
	},
	{
		"name": "Toggle - Fully Configured",
		"id": "afb3d794-8214-4888-8030-b3def1e5fe27",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TrueFalse",
		"editorUiAlias": "Umb.PropertyEditorUi.Toggle",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "default",
				"value": true
			},
			{
				"alias": "showLabels",
				"value": true
			},
			{
				"alias": "labelOn",
				"value": "Toggle is ON"
			},
			{
				"alias": "labelOff",
				"value": "Toggle is OFF"
			}
		]
	},
	{
		"name": "User Picker",
		"id": "3387e5da-4e32-43dc-b4dc-840fcbc468f9",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.UserPicker",
		"editorUiAlias": "Umb.PropertyEditorUi.UserPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Tags - Default Group, JSON Storage",
		"id": "965dc042-216d-4b0b-84ff-fb0050aed8e8",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Tags",
		"editorUiAlias": "Umb.PropertyEditorUi.Tags",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "group",
				"value": "default"
			},
			{
				"alias": "storageType",
				"value": "Json"
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5404707+00:00"
			},
			{
				"alias": "delimiter",
				"value": "\u0000"
			}
		]
	},
	{
		"name": "Tags - Custom Group, CSV Storage",
		"id": "c6d5b717-ad81-431f-81e4-b289801eb802",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Tags",
		"editorUiAlias": "Umb.PropertyEditorUi.Tags",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "group",
				"value": "custom"
			},
			{
				"alias": "storageType",
				"value": "Csv"
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5420056+00:00"
			},
			{
				"alias": "delimiter",
				"value": "\u0000"
			}
		]
	},
	{
		"name": "Label - String",
		"id": "b8164bfc-ae5d-4eee-b80e-bdf00745abba",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "STRING"
			}
		]
	},
	{
		"name": "Label - Decimal",
		"id": "3145614f-1e5e-47d2-a587-d7eb2d937a8f",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "DECIMAL"
			}
		]
	},
	{
		"name": "Label - DateTime",
		"id": "d9e26ead-b55f-4b24-96be-3da10ce87241",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "DATETIME"
			}
		]
	},
	{
		"name": "Label - Time",
		"id": "3c78f54b-0812-4f7c-a483-29c54583bf9f",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "TIME"
			}
		]
	},
	{
		"name": "Label - Integer",
		"id": "0169a2ba-63b5-442d-af00-98d54bf959d9",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "INT"
			}
		]
	},
	{
		"name": "Label - Big Integer",
		"id": "59171d58-e368-42dd-88a9-a3504561442c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "BIGINT"
			}
		]
	},
	{
		"name": "Label - Long String",
		"id": "b76769c6-9662-4848-afce-2c06a7464bf8",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Label",
		"editorUiAlias": "Umb.PropertyEditorUi.Label",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "umbracoDataValueType",
				"value": "TEXT"
			}
		]
	},
	{
		"name": "Image Cropper -  Without Crops",
		"id": "eae1607c-15e9-47f3-bc03-88a9480dcee6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ImageCropper",
		"editorUiAlias": "Umb.PropertyEditorUi.ImageCropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "Image Cropper - With Crops",
		"id": "444a843c-93fa-4589-9e66-949cca3f0e84",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.ImageCropper",
		"editorUiAlias": "Umb.PropertyEditorUi.ImageCropper",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "crops",
				"value": [
					{
						"alias": "one",
						"width": 111,
						"height": 111
					},
					{
						"alias": "two",
						"width": 222,
						"height": 222
					},
					{
						"alias": "three",
						"width": 333,
						"height": 333
					}
				]
			}
		]
	},
	{
		"name": "Block List - Default Config",
		"id": "f955664b-9ab0-4f76-b9d6-5742c44a073c",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockList",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089",
						"label": "{= title | uppercase }",
						"editorSize": "medium",
						"forceHideContentEditorInOverlay": false,
						"settingsElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e"
					},
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"editorSize": "medium",
						"forceHideContentEditorInOverlay": false,
						"label": "Document: {umbContentName: link}; Media: {umbContentName: mediaPicker}; URL: {umbLink: multiUrlPicker}; ${tiptapRte.markup}"
					}
				]
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "useSingleBlockMode",
				"value": false
			},
			{
				"alias": "useLiveEditing",
				"value": false
			},
			{
				"alias": "useInlineEditingAsDefault",
				"value": false
			}
		]
	},
	{
		"name": "Block List - Single Type Only",
		"id": "135b9faf-a464-4e5a-a02f-6d17b1d807cf",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockList",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "validationLimit",
				"value": {
					"max": 1
				}
			},
			{
				"alias": "useSingleBlockMode",
				"value": true
			},
			{
				"alias": "useLiveEditing",
				"value": false
			},
			{
				"alias": "useInlineEditingAsDefault",
				"value": false
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"label": "Element **Two** Groß æøå"
					}
				]
			}
		]
	},
	{
		"name": "Block List - Min And Max",
		"id": "44aae492-649c-4895-8c0a-01d29adf995f",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockList",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"editorSize": "medium",
						"forceHideContentEditorInOverlay": false
					},
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089",
						"label": "Element **One** Groß æøå"
					}
				]
			},
			{
				"alias": "validationLimit",
				"value": {
					"min": 2,
					"max": 10
				}
			},
			{
				"alias": "useSingleBlockMode",
				"value": false
			},
			{
				"alias": "useLiveEditing",
				"value": false
			},
			{
				"alias": "useInlineEditingAsDefault",
				"value": false
			}
		]
	},
	{
		"name": "Block Grid - Default Config",
		"id": "5555758f-d1c7-4480-a8ee-391fba1ac2ca",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockGrid",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockGrid",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"columnSpanOptions": [
							{
								"columnSpan": 6
							},
							{
								"columnSpan": 12
							}
						],
						"rowMinSpan": 1,
						"rowMaxSpan": 1,
						"allowAtRoot": true,
						"allowInAreas": true,
						"areas": [],
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089",
						"label": "{=title}",
						"editorSize": "medium",
						"inlineEditing": true,
						"forceHideContentEditorInOverlay": false,
						"thumbnail": "/wwwroot/media/0fvhcbw5/nxnw_300x300.jpg",
						"backgroundColor": "#ff0000",
						"iconColor": "#000000"
					},
					{
						"columnSpanOptions": [
							{
								"columnSpan": 6
							},
							{
								"columnSpan": 12
							}
						],
						"rowMinSpan": 1,
						"rowMaxSpan": 1,
						"allowAtRoot": true,
						"allowInAreas": true,
						"areas": [],
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"label": "Document: {umbContentName: link};",
						"editorSize": "medium",
						"inlineEditing": false,
						"forceHideContentEditorInOverlay": false,
						"backgroundColor": "#ffff00",
						"iconColor": "#0000ff"
					}
				]
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "useLiveEditing",
				"value": false
			}
		]
	},
	{
		"name": "Block Grid - With Areas",
		"id": "45ea7c93-cb3e-44d8-ada3-4382dfdc69f1",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockGrid",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockGrid",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"columnSpanOptions": [
							{
								"columnSpan": 10
							},
							{
								"columnSpan": 11
							},
							{
								"columnSpan": 9
							},
							{
								"columnSpan": 7
							},
							{
								"columnSpan": 5
							},
							{
								"columnSpan": 4
							},
							{
								"columnSpan": 3
							},
							{
								"columnSpan": 2
							},
							{
								"columnSpan": 1
							},
							{
								"columnSpan": 6
							},
							{
								"columnSpan": 8
							},
							{
								"columnSpan": 12
							}
						],
						"rowMinSpan": 2,
						"rowMaxSpan": 4,
						"allowAtRoot": true,
						"allowInAreas": true,
						"areas": [
							{
								"key": "ce99380c-d6a2-4ffb-bdc4-59cc27a0735a",
								"alias": "right",
								"columnSpan": 6,
								"rowSpan": 1,
								"minAllowed": 0,
								"specifiedAllowance": []
							},
							{
								"key": "84186bf3-663d-48f1-9815-f2f95119a205",
								"alias": "left",
								"columnSpan": 6,
								"rowSpan": 1,
								"minAllowed": 0,
								"specifiedAllowance": []
							}
						],
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089",
						"label": "{=title}",
						"editorSize": "medium",
						"inlineEditing": false,
						"forceHideContentEditorInOverlay": false
					},
					{
						"columnSpanOptions": [
							{
								"columnSpan": 6
							},
							{
								"columnSpan": 12
							}
						],
						"rowMinSpan": 1,
						"rowMaxSpan": 1,
						"allowAtRoot": true,
						"allowInAreas": false,
						"areas": [
							{
								"key": "c92e520a-1aa2-48cb-b7c9-45d586b6dc70",
								"alias": "left",
								"columnSpan": 6,
								"rowSpan": 1,
								"minAllowed": 0,
								"specifiedAllowance": []
							},
							{
								"key": "01a9b7db-7360-45bb-999c-edc438473f96",
								"alias": "right",
								"columnSpan": 6,
								"rowSpan": 1,
								"minAllowed": 0,
								"specifiedAllowance": []
							},
							{
								"key": "20a04dbe-f38d-4c6b-acf5-b38d2918d670",
								"alias": "center",
								"columnSpan": 12,
								"rowSpan": 1,
								"minAllowed": 0,
								"specifiedAllowance": []
							}
						],
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"label": "{=title}",
						"editorSize": "medium",
						"inlineEditing": false,
						"forceHideContentEditorInOverlay": false
					}
				]
			},
			{
				"alias": "validationLimit",
				"value": {}
			},
			{
				"alias": "useLiveEditing",
				"value": false
			},
			{
				"alias": "gridColumns",
				"value": 12
			}
		]
	},
	{
		"name": "File Upload - Default Config",
		"id": "2ece7647-e59f-44d1-952e-c28cf763a375",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": []
	},
	{
		"name": "File Upload - Specific File Types",
		"id": "9510982e-4e51-48d7-bf37-13a189d72658",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.UploadField",
		"editorUiAlias": "Umb.PropertyEditorUi.UploadField",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "fileExtensions",
				"value": [
					"jpg",
					"png"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5429215+00:00"
			}
		]
	},
	{
		"name": "Checkbox List - Configured",
		"id": "dfa2595b-165c-48a7-b6ff-820914484c12",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.CheckBoxList",
		"editorUiAlias": "Umb.PropertyEditorUi.CheckBoxList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "items",
				"value": [
					"One",
					"Two",
					"Three",
					"Four",
					"Five"
				]
			},
			{
				"alias": "umbMigrationV14",
				"value": "2024-03-06T13:46:53.5398797+00:00"
			}
		]
	},
	{
		"name": "Combobox - Default Config",
		"id": "88c96046-c994-4017-96bd-2b7e62d0dd5a",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DropDown.Flexible",
		"editorUiAlias": "Umb.PropertyEditorUi.Dropdown",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "items",
				"value": [
					"One",
					"Two",
					"Three",
					"Four"
				]
			},
			{
				"alias": "multiple",
				"value": true
			}
		]
	},
	{
		"name": "Block Grid - Empty",
		"id": "a6bf4d6e-23fb-4fe7-9e58-4cae964da4aa",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockGrid",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockGrid",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "validationLimit",
				"value": {
					"min": 1,
					"max": 1
				}
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089",
						"label": "Hello",
						"allowAtRoot": true
					}
				]
			}
		]
	},
	{
		"name": "DateTime Picker - Time",
		"id": "85db23e2-d38d-4e19-a2ed-2e82a836f027",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.DateTime",
		"editorUiAlias": "Umb.PropertyEditorUi.DatePicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "format",
				"value": "HH:mm:ss"
			}
		]
	},
	{
		"name": "Code Editor - Default Config",
		"id": "3a389162-9e57-4021-be97-070a82faf184",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Plain.String",
		"editorUiAlias": "Umb.PropertyEditorUi.TextBox",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "language",
				"value": [
					"css"
				]
			},
			{
				"alias": "height",
				"value": 400
			},
			{
				"alias": "wordWrap",
				"value": true
			},
			{
				"alias": "minimap",
				"value": true
			},
			{
				"alias": "lineNumbers",
				"value": true
			}
		]
	},
	{
		"name": "Content Picker - Dynamic Root",
		"id": "9e6524d3-40b9-4618-bdb6-9093fe33f133",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.MultiNodeTreePicker",
		"editorUiAlias": "Umb.PropertyEditorUi.ContentPicker",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "minNumber",
				"value": 0
			},
			{
				"alias": "maxNumber",
				"value": 0
			},
			{
				"alias": "startNode",
				"value": {
					"type": "content",
					"dynamicRoot": {
						"originAlias": "Root",
						"querySteps": [
							{
								"unique": "97e78777-4d3b-4b2f-8519-7251c8fbe228",
								"alias": "NearestDescendantOrSelf",
								"anyOfDocTypeKeys": [
									"7184285e-9709-4e13-8c72-1fe52f024b28"
								]
							}
						]
					}
				}
			},
			{
				"alias": "filter",
				"value": ""
			},
			{
				"alias": "showOpenButton",
				"value": true
			},
			{
				"alias": "ignoreUserStartNodes",
				"value": true
			}
		]
	},
	{
		"name": "Rich Text Editor - Blocks Config",
		"id": "e0f55305-5f94-4f6b-a2c1-76628e47e9eb",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic",
							"Umb.Tiptap.Toolbar.TextAlignLeft",
							"Umb.Tiptap.Toolbar.TextAlignCenter",
							"Umb.Tiptap.Toolbar.TextAlignRight",
							"Umb.Tiptap.Toolbar.BulletList",
							"Umb.Tiptap.Toolbar.OrderedList",
							"Umb.Tiptap.Toolbar.TextOutdent",
							"Umb.Tiptap.Toolbar.TextIndent",
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.SourceEditor",
							"Umb.Tiptap.Toolbar.MediaPicker",
							"Umb.Tiptap.Toolbar.EmbeddedMedia",
							"Umb.Tiptap.Toolbar.BlockPicker"
						]
					]
				]
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.RichTextEssentials",
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.TrailingNode"
				]
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089"
					},
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"displayInline": true
					}
				]
			}
		]
	},
	{
		"name": "Block List - Inline Mode",
		"id": "ecd9355b-b9e6-4bfd-889e-aa4642e02c80",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.BlockList",
		"editorUiAlias": "Umb.PropertyEditorUi.BlockList",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"label": "**Link: {umbContentName:link}; URL picker: {umbLink:multiUrlPicker}**"
					},
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089"
					}
				]
			},
			{
				"alias": "useLiveEditing",
				"value": false
			},
			{
				"alias": "useInlineEditingAsDefault",
				"value": true
			}
		]
	},
	{
		"name": "RTE Tiptap",
		"id": "6c85876b-5a9e-4a8a-8ec2-78162cfb7ef4",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.RichText",
		"editorUiAlias": "Umb.PropertyEditorUi.Tiptap",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "toolbar",
				"value": [
					[
						[
							"Umb.Tiptap.Toolbar.SourceEditor",
							"Umb.Tiptap.Toolbar.Undo",
							"Umb.Tiptap.Toolbar.Redo"
						],
						[
							"Umb.Tiptap.Toolbar.Bold",
							"Umb.Tiptap.Toolbar.Italic",
							"Umb.Tiptap.Toolbar.Underline",
							"Umb.Tiptap.Toolbar.Strike"
						],
						[
							"Umb.Tiptap.Toolbar.TextAlignLeft",
							"Umb.Tiptap.Toolbar.TextAlignCenter",
							"Umb.Tiptap.Toolbar.TextAlignRight",
							"Umb.Tiptap.Toolbar.TextAlignJustify"
						],
						[
							"Umb.Tiptap.Toolbar.StyleSelect",
							"Umb.Tiptap.Toolbar.FontFamily",
							"Umb.Tiptap.Toolbar.FontSize"
						],
						[
							"Umb.Tiptap.Toolbar.CharacterMap",
							"Umb.Tiptap.Toolbar.ClearFormatting"
						],
						[
							"Umb.Tiptap.Toolbar.Anchor",
							"Umb.Tiptap.Toolbar.Link",
							"Umb.Tiptap.Toolbar.Unlink"
						],
						[
							"Umb.Tiptap.Toolbar.TextColorForeground",
							"Umb.Tiptap.Toolbar.TextColorBackground"
						]
					],
					[
						[
							"Umb.Tiptap.Toolbar.Heading1",
							"Umb.Tiptap.Toolbar.Heading2",
							"Umb.Tiptap.Toolbar.Heading3"
						],
						[
							"Umb.Tiptap.Toolbar.BulletList",
							"Umb.Tiptap.Toolbar.OrderedList",
							"Umb.Tiptap.Toolbar.Blockquote"
						],
						[
							"Umb.Tiptap.Toolbar.Superscript",
							"Umb.Tiptap.Toolbar.Subscript",
							"Umb.Tiptap.Toolbar.CodeBlock",
							"Umb.Tiptap.Toolbar.HorizontalRule"
						],
						[
							"Umb.Tiptap.Toolbar.TextOutdent",
							"Umb.Tiptap.Toolbar.TextIndent"
						],
						[
							"Umb.Tiptap.Toolbar.TextDirectionRtl",
							"Umb.Tiptap.Toolbar.TextDirectionLtr"
						],
						[
							"Umb.Tiptap.Toolbar.Table",
							"Umb.Tiptap.Toolbar.MediaPicker",
							"Umb.Tiptap.Toolbar.EmbeddedMedia",
							"Umb.Tiptap.Toolbar.BlockPicker"
						]
					]
				]
			},
			{
				"alias": "maxImageSize",
				"value": 500
			},
			{
				"alias": "overlaySize",
				"value": "medium"
			},
			{
				"alias": "extensions",
				"value": [
					"Umb.Tiptap.Embed",
					"Umb.Tiptap.Figure",
					"Umb.Tiptap.Image",
					"Umb.Tiptap.Link",
					"Umb.Tiptap.MediaUpload",
					"Umb.Tiptap.Subscript",
					"Umb.Tiptap.Superscript",
					"Umb.Tiptap.Table",
					"Umb.Tiptap.TextAlign",
					"Umb.Tiptap.Underline",
					"Umb.Tiptap.Block",
					"Umb.Tiptap.TextDirection",
					"Umb.Tiptap.TextIndent",
					"Umb.Tiptap.WordCount",
					"Umb.Tiptap.Anchor",
					"Umb.Tiptap.Blockquote",
					"Umb.Tiptap.Bold",
					"Umb.Tiptap.BulletList",
					"Umb.Tiptap.CodeBlock",
					"Umb.Tiptap.Heading",
					"Umb.Tiptap.Italic",
					"Umb.Tiptap.OrderedList",
					"Umb.Tiptap.Strike",
					"Umb.Tiptap.HtmlAttributeClass",
					"Umb.Tiptap.HtmlAttributeDataset",
					"Umb.Tiptap.HtmlAttributeId",
					"Umb.Tiptap.HtmlTagDiv",
					"Umb.Tiptap.HtmlTagSpan",
					"Umb.Tiptap.TrailingNode",
					"Umb.Tiptap.HorizontalRule",
					"Umb.Tiptap.HtmlAttributeStyle",
					"Umb.Tiptap.RichTextEssentials"
				]
			},
			{
				"alias": "dimensions",
				"value": {
					"width": 0,
					"height": 0
				}
			},
			{
				"alias": "blocks",
				"value": [
					{
						"contentElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089"
					},
					{
						"contentElementTypeKey": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
						"displayInline": true,
						"settingsElementTypeKey": "b818bb55-31e1-4537-9c42-17471a176089"
					}
				]
			},
			{
				"alias": "statusbar",
				"value": [
					[
						"Umb.Tiptap.Statusbar.ElementPath"
					],
					[
						"Umb.Tiptap.Statusbar.WordCount"
					]
				]
			}
		]
	},
	{
		"name": "Numeric - Misconfigured",
		"id": "4e0fd13d-b387-4e69-b241-959e8ca9b022",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.Integer",
		"editorUiAlias": "Umb.PropertyEditorUi.Integer",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "min",
				"value": 10
			},
			{
				"alias": "max",
				"value": 5
			},
			{
				"alias": "step",
				"value": 1
			}
		]
	},
	{
		"name": "Textarea - Misconfigured",
		"id": "0ed2de25-fc8f-41eb-a2c6-2ea299df9201",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.TextArea",
		"editorUiAlias": "Umb.PropertyEditorUi.TextArea",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "rows",
				"value": 0
			},
			{
				"alias": "minHeight",
				"value": 100
			},
			{
				"alias": "maxHeight",
				"value": 50
			},
			{
				"alias": "maxChars",
				"value": 50
			}
		]
	},
	{
		"name": "Email Address - Default Config",
		"id": "10eada9e-528f-49e8-bc8d-da34e538b7a6",
		"parent": {
			"id": "671efc93-83f3-47a8-bd08-b2b7179a8b5a"
		},
		"editorAlias": "Umbraco.EmailAddress",
		"editorUiAlias": "Umb.PropertyEditorUi.Email",
		"hasChildren": false,
		"isFolder": false,
		"isDeletable": true,
		"canIgnoreStartNodes": false,
		"flags": [],
		"values": [
			{
				"alias": "inputType",
				"value": "email"
			}
		]
	}
];
