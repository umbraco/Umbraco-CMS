import type { UmbMockDocumentTypeModel } from '../../types/mock-data-set.types.js';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string composition type to enum
function mapCompositionType(type: string): CompositionTypeModel {
	switch (type) {
		case 'Composition': return CompositionTypeModel.COMPOSITION;
		case 'Inheritance': return CompositionTypeModel.INHERITANCE;
		default: return CompositionTypeModel.COMPOSITION;
	}
}

const rawData = [
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "25b36f28-5051-4073-a0c7-3887f6f8c695",
		"alias": "testdocumenttypes",
		"name": "Test Document Types",
		"description": null,
		"icon": "icon-folder",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": true,
		"parent": null,
		"isFolder": true,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "a29519c1-1605-4811-8830-dde83e09d892",
		"alias": "testelementtypes",
		"name": "Test Element Types",
		"description": null,
		"icon": "icon-folder",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": true,
		"parent": null,
		"isFolder": true,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "84dd9060-ac7f-43c9-9963-5f2b5e722fd9",
		"alias": "dynamicroottest",
		"name": "Dynamic Root Test",
		"description": null,
		"icon": "icon-folder",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": true,
		"parent": null,
		"isFolder": true,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "671b6a56-0b2d-4a7e-b586-fa6da24ff5ed",
		"alias": "blocklisttest",
		"name": "Block List Test",
		"description": null,
		"icon": "icon-folder",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": true,
		"parent": null,
		"isFolder": true,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [
			{
				"id": "34c10c5b-6fc0-41de-88f0-c436a06dd21c"
			}
		],
		"defaultTemplate": {
			"id": "34c10c5b-6fc0-41de-88f0-c436a06dd21c"
		},
		"id": "7184285e-9709-4e13-8c72-1fe52f024b28",
		"alias": "home",
		"name": "Home",
		"description": "This is a description for the homepage.",
		"icon": "icon-home color-blue",
		"allowedAsRoot": true,
		"variesByCulture": true,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": null,
		"isFolder": false,
		"properties": [
			{
				"id": "pt-155",
				"container": {
					"id": "39ad8047-c922-4717-ad19-f8ae4a431f9a"
				},
				"alias": "collection",
				"name": "Collection",
				"description": "",
				"dataType": {
					"id": "c0808dd3-8133-4e4b-8ce8-e2bea84a96a4"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": true
				}
			}
		],
		"containers": [
			{
				"id": "39ad8047-c922-4717-ad19-f8ae4a431f9a",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [
			{
				"documentType": {
					"id": "015bc281-7410-40e2-81b5-b8f7c963bd61"
				},
				"sortOrder": 5
			},
			{
				"documentType": {
					"id": "13c10f78-bf14-411d-9444-751e4bd1b178"
				},
				"sortOrder": 7
			},
			{
				"documentType": {
					"id": "41f34bb7-fd63-442f-8dcb-142df4246310"
				},
				"sortOrder": 8
			},
			{
				"documentType": {
					"id": "9cff8f66-0e13-4617-ab9b-9f845ecc5e24"
				},
				"sortOrder": 9
			},
			{
				"documentType": {
					"id": "0180d16d-6a87-4631-9802-4e1b1f180bd4"
				},
				"sortOrder": 10
			},
			{
				"documentType": {
					"id": "fb88c3ab-40ee-4822-a63e-0edd97ad13f8"
				},
				"sortOrder": 11
			},
			{
				"documentType": {
					"id": "8856d507-76e0-47c7-8564-56467e717053"
				},
				"sortOrder": 12
			},
			{
				"documentType": {
					"id": "99431793-6f52-48c7-af53-6bf04668aca2"
				},
				"sortOrder": 16
			},
			{
				"documentType": {
					"id": "7b61b708-aa42-4978-a86c-f20fd4749a58"
				},
				"sortOrder": 17
			},
			{
				"documentType": {
					"id": "f7c73e80-e8f4-4ef6-a710-168d89991c7d"
				},
				"sortOrder": 18
			},
			{
				"documentType": {
					"id": "48a02560-7ce9-4be4-96e7-e4041cc19622"
				},
				"sortOrder": 19
			},
			{
				"documentType": {
					"id": "727b819b-af42-443f-a752-c4c5cfd69313"
				},
				"sortOrder": 20
			},
			{
				"documentType": {
					"id": "7025ee6c-8d6c-4abd-8e32-2cab5fde6f90"
				},
				"sortOrder": 21
			},
			{
				"documentType": {
					"id": "cc827fc0-e385-494b-88f6-d4abb47b7081"
				},
				"sortOrder": 22
			},
			{
				"documentType": {
					"id": "f984a2dc-01c0-4974-a860-b41dfeacf2b5"
				},
				"sortOrder": 23
			},
			{
				"documentType": {
					"id": "7b52f09a-3034-43d6-a83e-5f9fadfcc87d"
				},
				"sortOrder": 24
			},
			{
				"documentType": {
					"id": "fd62fafc-9cfd-470a-a260-93af5d1ed641"
				},
				"sortOrder": 25
			},
			{
				"documentType": {
					"id": "23c4c503-bcdf-46a5-9ff9-fb78d9dba4ae"
				},
				"sortOrder": 27
			},
			{
				"documentType": {
					"id": "af83a333-d5f9-4467-9013-9eaa8112a571"
				},
				"sortOrder": 30
			},
			{
				"documentType": {
					"id": "373eaceb-e41e-4dd2-ae3f-b73fd11cf182"
				},
				"sortOrder": 32
			},
			{
				"documentType": {
					"id": "6717ef28-57a2-4cb4-80fe-ddc7a76da5f4"
				},
				"sortOrder": 31
			},
			{
				"documentType": {
					"id": "b85bb884-ed5e-4f0b-8b10-8067090e8ada"
				},
				"sortOrder": 3
			},
			{
				"documentType": {
					"id": "6dcde803-d22e-4fcf-85a3-3a03be080d3a"
				},
				"sortOrder": 28
			},
			{
				"documentType": {
					"id": "dc965257-84c2-4f27-b452-55e8b0f91a96"
				},
				"sortOrder": 33
			},
			{
				"documentType": {
					"id": "25dd3762-cfdd-43cd-b0a5-8f094f8a7fd2"
				},
				"sortOrder": 15
			},
			{
				"documentType": {
					"id": "2a773487-9de7-403c-9207-54f4ace7f215"
				},
				"sortOrder": 14
			},
			{
				"documentType": {
					"id": "61c6b912-8fe8-4e10-a07b-4f777b99489b"
				},
				"sortOrder": 2
			},
			{
				"documentType": {
					"id": "1addd0ad-0e34-4386-801b-79cf7beb8cf1"
				},
				"sortOrder": 1
			},
			{
				"documentType": {
					"id": "11b48beb-3fd0-4b72-800e-364f6e833dc7"
				},
				"sortOrder": 13
			},
			{
				"documentType": {
					"id": "442ba583-4725-4652-b19a-8aa2e6529e94"
				},
				"sortOrder": 29
			},
			{
				"documentType": {
					"id": "e355cffc-e70c-4e36-946a-9d448f392b89"
				},
				"sortOrder": 6
			},
			{
				"documentType": {
					"id": "c07f8d38-302f-4e4a-bd84-d57d79a4af46"
				},
				"sortOrder": 26
			},
			{
				"documentType": {
					"id": "5ece407b-ca9e-44fe-be4f-8c819c444cdd"
				},
				"sortOrder": 0
			},
			{
				"documentType": {
					"id": "8f3cd603-85af-4784-bfb7-cf966cdd6ac7"
				},
				"sortOrder": 4
			}
		],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "015bc281-7410-40e2-81b5-b8f7c963bd61",
		"alias": "colorPickerDocType",
		"name": "Color Picker",
		"description": "This is a description for the color picker page.",
		"icon": "icon-colorpicker color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-52",
				"container": {
					"id": "7c07140b-4715-43ab-aa18-01e1456bf72f"
				},
				"alias": "colorPickerNoLabels",
				"name": "Color Picker - No Labels",
				"description": null,
				"dataType": {
					"id": "a62e05d6-f7f8-4929-b35b-2e3068692eb6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-53",
				"container": {
					"id": "7c07140b-4715-43ab-aa18-01e1456bf72f"
				},
				"alias": "colorPickerLabels",
				"name": "Color Picker - Labels",
				"description": null,
				"dataType": {
					"id": "c4fb2e7f-c707-41c4-994f-b224d0b66612"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "7c07140b-4715-43ab-aa18-01e1456bf72f",
				"parent": null,
				"name": "Color Pickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "7094e3db-3fbb-4f4b-aff3-6f60629ef816",
		"alias": "deeplyNestedTestPage",
		"name": "Deeply Nested Test Page",
		"description": null,
		"icon": "icon-document",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "d4858de0-f13e-45e9-a88e-e20d2e85223e"
		},
		"isFolder": false,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "5d53249e-330c-4527-a3fb-2ddab802305c",
		"alias": "category",
		"name": "Category",
		"description": null,
		"icon": "icon-tag",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "84dd9060-ac7f-43c9-9963-5f2b5e722fd9"
		},
		"isFolder": false,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "13c10f78-bf14-411d-9444-751e4bd1b178",
		"alias": "contentPickerDocType",
		"name": "Document Picker",
		"description": "This is a description for the content picker page.",
		"icon": "icon-autofill color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-54",
				"container": {
					"id": "38c46685-f235-4584-b245-11553d500484"
				},
				"alias": "contentPickerDefaultConfig",
				"name": "Document Picker - Default Config",
				"description": null,
				"dataType": {
					"id": "1bd0d68f-8fe9-4906-bb5e-e33eafa83aa3"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-55",
				"container": {
					"id": "38c46685-f235-4584-b245-11553d500484"
				},
				"alias": "contentPickerIgnoreUserStartNodes",
				"name": "Document Picker - Ignore User Start Nodes",
				"description": null,
				"dataType": {
					"id": "b01b3451-7875-4ac9-a772-b2f23b865af3"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-56",
				"container": {
					"id": "38c46685-f235-4584-b245-11553d500484"
				},
				"alias": "contentPickerShowOpenButton",
				"name": "Document Picker - Show Open Button",
				"description": null,
				"dataType": {
					"id": "8aa44228-5263-4395-9588-9ba401d9e0a1"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-57",
				"container": {
					"id": "38c46685-f235-4584-b245-11553d500484"
				},
				"alias": "contentPickerStartNode",
				"name": "Document Picker - Start Node",
				"description": null,
				"dataType": {
					"id": "adcccf89-532f-4a50-83c8-e742035a12a3"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "38c46685-f235-4584-b245-11553d500484",
				"parent": null,
				"name": "Content Pickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "f7f156a0-a3f3-42ec-8b9c-e788157bd84e",
		"alias": "elementTwoElementType",
		"name": "Element Two Groß æøå ${7*7}test page\"><iframe src=\"https://evil.com\"></iframe> קבוצתקישורים",
		"description": null,
		"icon": "icon-addressbook color-red",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": true,
		"hasChildren": false,
		"parent": {
			"id": "a29519c1-1605-4811-8830-dde83e09d892"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-145",
				"container": {
					"id": "4fe71f18-3851-4335-8875-efe72e7507db"
				},
				"alias": "link",
				"name": "Link",
				"description": null,
				"dataType": {
					"id": "1bd0d68f-8fe9-4906-bb5e-e33eafa83aa3"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-209",
				"container": {
					"id": "4fe71f18-3851-4335-8875-efe72e7507db"
				},
				"alias": "multiUrlPicker",
				"name": "Multi-URL Picker",
				"description": "",
				"dataType": {
					"id": "f455a80c-7f39-4fbb-b212-cf829dd28f7b"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-213",
				"container": {
					"id": "4fe71f18-3851-4335-8875-efe72e7507db"
				},
				"alias": "mediaPicker",
				"name": "Media Picker",
				"description": "",
				"dataType": {
					"id": "fec81092-5db6-45b2-8050-09dcfead2901"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-215",
				"container": {
					"id": "4fe71f18-3851-4335-8875-efe72e7507db"
				},
				"alias": "tiptapRte",
				"name": "Tiptap RTE",
				"description": "<umb-debug visible dialog></umb-debug>",
				"dataType": {
					"id": "3661aaf8-6e63-4005-aaa5-ea2245f36c7d"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "4fe71f18-3851-4335-8875-efe72e7507db",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [
			{
				"id": "592d4920-7a78-4c5f-917e-40eae48b03bb"
			}
		],
		"defaultTemplate": {
			"id": "592d4920-7a78-4c5f-917e-40eae48b03bb"
		},
		"id": "442ba583-4725-4652-b19a-8aa2e6529e94",
		"alias": "testPage",
		"name": "Test Page",
		"description": "This is a description for the test page.",
		"icon": "icon-document",
		"allowedAsRoot": false,
		"variesByCulture": true,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": true,
		"parent": null,
		"isFolder": false,
		"properties": [
			{
				"id": "pt-711",
				"container": {
					"id": "d32c9908-08ca-46df-af4e-2412d405d860"
				},
				"alias": "testText1",
				"name": "Test Text 1",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-712",
				"container": {
					"id": "9c986c6d-cf47-4204-99ca-35ec76dc1001"
				},
				"alias": "testText2",
				"name": "Test Text 2",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-713",
				"container": {
					"id": "1e16c34c-1d1b-4a0c-8453-cd4deb1446aa"
				},
				"alias": "testText3",
				"name": "Test Text 3",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-714",
				"container": {
					"id": "6adc4408-cb75-406d-9f04-5be52db38da6"
				},
				"alias": "testText4",
				"name": "Test Text 4",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-715",
				"container": {
					"id": "8f876fc8-130c-40d6-bfe6-42addbc1beee"
				},
				"alias": "testText5",
				"name": "Test Text 5",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-722",
				"container": {
					"id": "d193e812-d8a7-497c-b1f4-70993cc41cc8"
				},
				"alias": "entityDataPicker",
				"name": "Entity Data Picker",
				"description": "",
				"dataType": {
					"id": "68d8f7be-d93d-4fbe-951c-cad6c301ca76"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-720",
				"container": {
					"id": "d193e812-d8a7-497c-b1f4-70993cc41cc8"
				},
				"alias": "variantTextstring",
				"name": "Variant Textstring",
				"description": "",
				"dataType": {
					"id": "1bd0d68f-8fe9-4906-bb5e-e33eafa83aa3"
				},
				"variesByCulture": true,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-629",
				"container": {
					"id": "d193e812-d8a7-497c-b1f4-70993cc41cc8"
				},
				"alias": "headline",
				"name": "Headline",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 40,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-208",
				"container": {
					"id": "d193e812-d8a7-497c-b1f4-70993cc41cc8"
				},
				"alias": "testEditor",
				"name": "Test Editor",
				"description": "This property has an alias of _${alias}_, and a label of **${label}**. { =alias}; { umbValue: alias }\n\n6 * 7 = ${6 * 7}\n\n- [ ] Todo 1\n- [x] Todo 2\n- [ ] Todo 3\n\nHere is some inline markup: <uui-tag color=\"positive\" look=\"primary\">Umbraco</uui-tag> <uui-tag color=\"positive\" look=\"secondary\">Bellissima</uui-tag>\n\nThis is a loooooooooooooooooooooooooooooooooooong line.\n\n---\n\n![image](https://localhost:44339/media/dqaijrza/nxnw_300x300.jpg)\n\nThis description has **bold**, _italic_ and ~strikethrough~.\nThis is a [link](https://umbraco.com).\n\n- List item 1\n- List item 2\n- List item 3\n\n> blockquote\n\n1. {#general_add}\n1. {#general_choose}\n1. {#general_close}\n\n# Heading 1\n## Heading 2\n### Heading 3\n#### Heading 4\n##### Heading 5\n###### Heading 6\n\nInline `code`\n\n```\n// block\n// code\n```\n\n<p><i>Custom</i> <b>HTML</b></p>\n\n<script>alert('XSS');</script>\n\n<umb-debug visible dialog></umb-debug>",
				"dataType": {
					"id": "3661aaf8-6e63-4005-aaa5-ea2245f36c7d"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 50,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "d193e812-d8a7-497c-b1f4-70993cc41cc8",
				"parent": null,
				"name": "Content",
				"type": "Group",
				"sortOrder": 10
			},
			{
				"id": "d32c9908-08ca-46df-af4e-2412d405d860",
				"parent": null,
				"name": "A very overly long and silly billy tab name 1",
				"type": "Group",
				"sortOrder": 11
			},
			{
				"id": "9c986c6d-cf47-4204-99ca-35ec76dc1001",
				"parent": null,
				"name": "A very overly long and silly billy tab name 2",
				"type": "Group",
				"sortOrder": 12
			},
			{
				"id": "1e16c34c-1d1b-4a0c-8453-cd4deb1446aa",
				"parent": null,
				"name": "A very overly long and silly billy tab name 3",
				"type": "Group",
				"sortOrder": 13
			},
			{
				"id": "6adc4408-cb75-406d-9f04-5be52db38da6",
				"parent": null,
				"name": "A very overly long and silly billy tab name 4",
				"type": "Group",
				"sortOrder": 14
			},
			{
				"id": "8f876fc8-130c-40d6-bfe6-42addbc1beee",
				"parent": null,
				"name": "A very overly long and silly billy tab name 5",
				"type": "Group",
				"sortOrder": 15
			}
		],
		"allowedDocumentTypes": [
			{
				"documentType": {
					"id": "442ba583-4725-4652-b19a-8aa2e6529e94"
				},
				"sortOrder": 0
			}
		],
		"compositions": [
			{
				"documentType": {
					"id": "d4858de0-f13e-45e9-a88e-e20d2e85223e"
				},
				"compositionType": "Composition"
			}
		],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "651c932b-232d-42b3-a52e-c1399fcff9bc",
		"alias": "categoryStorage",
		"name": "Category Storage",
		"description": null,
		"icon": "icon-tags",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "84dd9060-ac7f-43c9-9963-5f2b5e722fd9"
		},
		"isFolder": false,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [
			{
				"documentType": {
					"id": "5d53249e-330c-4527-a3fb-2ddab802305c"
				},
				"sortOrder": 0
			}
		],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "98b2e832-1910-43e5-9769-f25086294a73",
		"alias": "blockListDocumentType",
		"name": "Block List Document Type",
		"description": null,
		"icon": "icon-gitbook color-blue",
		"allowedAsRoot": true,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "671b6a56-0b2d-4a7e-b586-fa6da24ff5ed"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-295",
				"container": {
					"id": "7a90eab4-6ec4-4e17-9c48-34608eb4d7c3"
				},
				"alias": "blockList",
				"name": "Block List",
				"description": "",
				"dataType": {
					"id": "29c12c36-f409-4ecd-8976-0bbe01157ebf"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-789",
				"container": {
					"id": "587ef7a2-cb16-427d-9693-d567c73d1767"
				},
				"alias": "blockGrid",
				"name": "Block Grid",
				"description": "",
				"dataType": {
					"id": "5555758f-d1c7-4480-a8ee-391fba1ac2ca"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-393",
				"container": {
					"id": "7a90eab4-6ec4-4e17-9c48-34608eb4d7c3"
				},
				"alias": "blockList2",
				"name": "Block List 2",
				"description": "",
				"dataType": {
					"id": "aacdffb3-dd78-4427-af92-38100fad60cc"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "587ef7a2-cb16-427d-9693-d567c73d1767",
				"parent": null,
				"name": "Block Grid",
				"type": "Tab",
				"sortOrder": 10
			},
			{
				"id": "7a90eab4-6ec4-4e17-9c48-34608eb4d7c3",
				"parent": null,
				"name": "Block List",
				"type": "Tab",
				"sortOrder": 20
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "41f34bb7-fd63-442f-8dcb-142df4246310",
		"alias": "dateTypePickerDocType",
		"name": "DateType Picker",
		"description": null,
		"icon": "icon-time color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-58",
				"container": {
					"id": "fa9fb286-8ab3-492a-90fc-5777c0b00742"
				},
				"alias": "dateTimePickerDateFormat",
				"name": "DateTime Picker - Date Format",
				"description": null,
				"dataType": {
					"id": "59bbc6f4-9515-4264-bad5-66762e50c3d6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-59",
				"container": {
					"id": "fa9fb286-8ab3-492a-90fc-5777c0b00742"
				},
				"alias": "dateTimePickerDatePlusTimeFormat",
				"name": "DateTime Picker - Date + Time Format",
				"description": null,
				"dataType": {
					"id": "e8602b3f-8b89-4f91-8c94-d2fd7df534e6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-60",
				"container": {
					"id": "fa9fb286-8ab3-492a-90fc-5777c0b00742"
				},
				"alias": "dateTimePickerOffsetTime",
				"name": "DateTime Picker - Offset Time",
				"description": null,
				"dataType": {
					"id": "3773d64f-495d-4e88-9ec4-7eb58c334687"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-179",
				"container": {
					"id": "fa9fb286-8ab3-492a-90fc-5777c0b00742"
				},
				"alias": "dateTimePickerTime",
				"name": "DateTime Picker - Time",
				"description": "",
				"dataType": {
					"id": "85db23e2-d38d-4e19-a2ed-2e82a836f027"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "fa9fb286-8ab3-492a-90fc-5777c0b00742",
				"parent": null,
				"name": "DateTime Pickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "b818bb55-31e1-4537-9c42-17471a176089",
		"alias": "elementOneElementType",
		"name": "Element One",
		"description": null,
		"icon": "icon-attachment color-deep-purple",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": true,
		"hasChildren": false,
		"parent": {
			"id": "a29519c1-1605-4811-8830-dde83e09d892"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-144",
				"container": {
					"id": "4f13c5d7-3e86-464b-b0d1-2030f0253450"
				},
				"alias": "title",
				"name": "#leeDev_propertyHeadline",
				"description": "{#leeDev_propertyHeadlineDescription}",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-183",
				"container": {
					"id": "4f13c5d7-3e86-464b-b0d1-2030f0253450"
				},
				"alias": "mntpDynamicRoot",
				"name": "MNTP Dynamic Root",
				"description": "",
				"dataType": {
					"id": "9e6524d3-40b9-4618-bdb6-9093fe33f133"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-187",
				"container": {
					"id": "4f13c5d7-3e86-464b-b0d1-2030f0253450"
				},
				"alias": "blockList",
				"name": "Block List",
				"description": "",
				"dataType": {
					"id": "135b9faf-a464-4e5a-a02f-6d17b1d807cf"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-222",
				"container": {
					"id": "4f13c5d7-3e86-464b-b0d1-2030f0253450"
				},
				"alias": "radioButtonList",
				"name": "Radio Button List",
				"description": "",
				"dataType": {
					"id": "2d3a109a-de8d-4c12-af67-4f7a116cebe5"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 31,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "4f13c5d7-3e86-464b-b0d1-2030f0253450",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "5ece407b-ca9e-44fe-be4f-8c819c444cdd",
		"alias": "tiptapRteTestPage",
		"name": "Tiptap RTE Test Page",
		"description": null,
		"icon": "icon-app",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": null,
		"isFolder": false,
		"properties": [
			{
				"id": "pt-221",
				"container": {
					"id": "bdee1190-86f5-4580-9754-45a5bf203be2"
				},
				"alias": "tiptapRte",
				"name": "Tiptap RTE",
				"description": "",
				"dataType": {
					"id": "720e2055-413e-4f92-8187-f9ea44953531"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "bdee1190-86f5-4580-9754-45a5bf203be2",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "2657b3a0-3400-4fcd-b7b9-58d278461e89",
		"alias": "dynamicRootPage",
		"name": "Dynamic Root Page",
		"description": null,
		"icon": "icon-document",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "84dd9060-ac7f-43c9-9963-5f2b5e722fd9"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-226",
				"container": {
					"id": "225d30d8-4930-43f3-a71a-d059a6dd5d81"
				},
				"alias": "categories",
				"name": "Categories",
				"description": "",
				"dataType": {
					"id": "304ce88f-306e-4ee5-a21b-79f638e4eb15"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-498",
				"container": {
					"id": "98eb6573-8f6e-49ea-ba3a-d57223c28d74"
				},
				"alias": "mediaPicker",
				"name": "Media Picker",
				"description": "",
				"dataType": {
					"id": "0e33a2de-d6a3-4853-8a68-7f56667f1d75"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "225d30d8-4930-43f3-a71a-d059a6dd5d81",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 0
			},
			{
				"id": "ee8d1ded-bd44-4a10-abd1-47255343a78e",
				"parent": null,
				"name": "Content",
				"type": "Group",
				"sortOrder": 0
			},
			{
				"id": "98eb6573-8f6e-49ea-ba3a-d57223c28d74",
				"parent": null,
				"name": "Media",
				"type": "Tab",
				"sortOrder": 0
			},
			{
				"id": "5e1b902f-79d9-4ec4-8371-4e5669acba3f",
				"parent": null,
				"name": "Media",
				"type": "Group",
				"sortOrder": 1
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "511fef8a-5e2e-42b9-8312-0a6034881793",
		"alias": "blockElementType",
		"name": "Block Element Type",
		"description": null,
		"icon": "icon-brick color-blue",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": true,
		"hasChildren": false,
		"parent": {
			"id": "671b6a56-0b2d-4a7e-b586-fa6da24ff5ed"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-716",
				"container": {
					"id": "de29d746-2f04-4dc2-a5e6-979759415375"
				},
				"alias": "richTextEditor",
				"name": "Rich Text Editor",
				"description": "",
				"dataType": {
					"id": "3661aaf8-6e63-4005-aaa5-ea2245f36c7d"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-389",
				"container": {
					"id": "de29d746-2f04-4dc2-a5e6-979759415375"
				},
				"alias": "textstring",
				"name": "Textstring",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-392",
				"container": {
					"id": "de29d746-2f04-4dc2-a5e6-979759415375"
				},
				"alias": "nestedBlockList",
				"name": "Nested Block List",
				"description": "",
				"dataType": {
					"id": "29c12c36-f409-4ecd-8976-0bbe01157ebf"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "de29d746-2f04-4dc2-a5e6-979759415375",
				"parent": null,
				"name": "testGroup",
				"type": "Tab",
				"sortOrder": 10
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "3daded69-855c-4531-9db1-dfe075541304",
		"alias": "elementTypeComposition",
		"name": "Element Type Composition",
		"description": null,
		"icon": "icon-plugin color-brown",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": true,
		"hasChildren": false,
		"parent": {
			"id": "a29519c1-1605-4811-8830-dde83e09d892"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-630",
				"container": {
					"id": "6feeb6c7-ec69-4f10-8601-3e478472dce1"
				},
				"alias": "test",
				"name": "Test",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "6feeb6c7-ec69-4f10-8601-3e478472dce1",
				"parent": null,
				"name": "Test",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [
			{
				"documentType": {
					"id": "b818bb55-31e1-4537-9c42-17471a176089"
				},
				"compositionType": "Composition"
			}
		],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "9cff8f66-0e13-4617-ab9b-9f845ecc5e24",
		"alias": "decimalDocType",
		"name": "Decimal",
		"description": null,
		"icon": "icon-autofill color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-61",
				"container": {
					"id": "e629b81a-6b75-4b7f-9bfa-1bfc29b6e2a2"
				},
				"alias": "decimalDefaultConfig",
				"name": "Decimal - Default Config",
				"description": null,
				"dataType": {
					"id": "bed85b43-5d17-4676-9cf7-56bd193a053b"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-62",
				"container": {
					"id": "e629b81a-6b75-4b7f-9bfa-1bfc29b6e2a2"
				},
				"alias": "decimalFullyConfigured",
				"name": "Decimal - Fully Configured",
				"description": null,
				"dataType": {
					"id": "9ca94289-e14d-460a-bfd1-cb958ed90da5"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "e629b81a-6b75-4b7f-9bfa-1bfc29b6e2a2",
				"parent": null,
				"name": "Decimals",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "d4858de0-f13e-45e9-a88e-e20d2e85223e",
		"alias": "nestedTestPage",
		"name": "Nested Test Page",
		"description": null,
		"icon": "icon-document",
		"allowedAsRoot": false,
		"variesByCulture": true,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": true,
		"parent": {
			"id": "442ba583-4725-4652-b19a-8aa2e6529e94"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-216",
				"container": {
					"id": "eaf5a845-bace-4a09-91cc-e5575178b2f4"
				},
				"alias": "anotherTextbox",
				"name": "Another Textbox",
				"description": "",
				"dataType": {
					"id": "3a9b04b9-e96a-46d0-bd84-2c13fc36b70c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 51,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": true
				}
			}
		],
		"containers": [
			{
				"id": "00d99fa1-e029-4e2b-9fc9-dd468221ade6",
				"parent": null,
				"name": "Content",
				"type": "Group",
				"sortOrder": 10
			},
			{
				"id": "eaf5a845-bace-4a09-91cc-e5575178b2f4",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 10
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [
			{
				"documentType": {
					"id": "7094e3db-3fbb-4f4b-aff3-6f60629ef816"
				},
				"compositionType": "Composition"
			}
		],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "e36f0f5f-d7da-4ad0-a6eb-5bcbc3e80e16",
		"alias": "dynamicRootContainer",
		"name": "Dynamic Root Container",
		"description": null,
		"icon": "icon-flowerpot color-blue",
		"allowedAsRoot": true,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "84dd9060-ac7f-43c9-9963-5f2b5e722fd9"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-227",
				"container": {
					"id": "52d4ecbe-753b-4217-9160-6e2b0b3709dc"
				},
				"alias": "categories",
				"name": "Categories",
				"description": "",
				"dataType": {
					"id": "304ce88f-306e-4ee5-a21b-79f638e4eb15"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "52d4ecbe-753b-4217-9160-6e2b0b3709dc",
				"parent": null,
				"name": "Content",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [
			{
				"documentType": {
					"id": "651c932b-232d-42b3-a52e-c1399fcff9bc"
				},
				"sortOrder": 0
			},
			{
				"documentType": {
					"id": "2657b3a0-3400-4fcd-b7b9-58d278461e89"
				},
				"sortOrder": 1
			}
		],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": [],
		"collection": {
			"id": "c0808dd3-8133-4e4b-8ce8-e2bea84a96a4"
		}
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "0180d16d-6a87-4631-9802-4e1b1f180bd4",
		"alias": "dropdownDocType",
		"name": "Dropdown",
		"description": null,
		"icon": "icon-indent color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-718",
				"container": {
					"id": "de50f42a-9d88-4c92-b297-a3a61dee7dcc"
				},
				"alias": "dropdownMultiValue",
				"name": "Dropdown - Multi Value",
				"description": "",
				"dataType": {
					"id": "779051c2-7bb7-4ab4-82ac-698faa8286aa"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-63",
				"container": {
					"id": "de50f42a-9d88-4c92-b297-a3a61dee7dcc"
				},
				"alias": "dropdownMultiValueRequired",
				"name": "Dropdown - Multi Value (Required)",
				"description": null,
				"dataType": {
					"id": "779051c2-7bb7-4ab4-82ac-698faa8286aa"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 15,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": "Yo! Please select an option.",
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-719",
				"container": {
					"id": "de50f42a-9d88-4c92-b297-a3a61dee7dcc"
				},
				"alias": "dropdownSingleValue",
				"name": "Dropdown - Single Value",
				"description": "",
				"dataType": {
					"id": "3c1f48e0-6eec-44f3-8072-3e22d442a0a0"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-64",
				"container": {
					"id": "de50f42a-9d88-4c92-b297-a3a61dee7dcc"
				},
				"alias": "dropdownSingleValueRequired",
				"name": "Dropdown - Single Value (Required)",
				"description": null,
				"dataType": {
					"id": "3c1f48e0-6eec-44f3-8072-3e22d442a0a0"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 25,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": "Yo! Please select an option.",
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "de50f42a-9d88-4c92-b297-a3a61dee7dcc",
				"parent": null,
				"name": "Dropdowns",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "bf77367d-4a94-46d5-a61a-a017078150a8",
		"alias": "blockgridelement",
		"name": "BlockGridElement",
		"description": null,
		"icon": "icon-document",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": true,
		"hasChildren": false,
		"parent": null,
		"isFolder": false,
		"properties": [
			{
				"id": "pt-764",
				"container": {
					"id": "44200614-20e7-44cc-a9ac-67e2e7983dc9"
				},
				"alias": "textstring",
				"name": "Textstring",
				"description": "",
				"dataType": {
					"id": "0cc0eba1-9960-42c9-bf9b-60e150b429ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "44200614-20e7-44cc-a9ac-67e2e7983dc9",
				"parent": null,
				"name": "testGroup",
				"type": "Tab",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "f962a238-8a52-4e3a-b786-fceff98da120",
		"alias": "testdocumenttype",
		"name": "TestDocumentType",
		"description": null,
		"icon": "icon-umbraco",
		"allowedAsRoot": true,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": null,
		"isFolder": false,
		"properties": [],
		"containers": [],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "fb88c3ab-40ee-4822-a63e-0edd97ad13f8",
		"alias": "emailAddressDocType",
		"name": "Email Address",
		"description": null,
		"icon": "icon-message color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-497",
				"container": {
					"id": "f8082afc-a3fd-4d32-8927-c7159290fb8c"
				},
				"alias": "emailAddress",
				"name": "Email Address",
				"description": "",
				"dataType": {
					"id": "10eada9e-528f-49e8-bc8d-da34e538b7a6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "f8082afc-a3fd-4d32-8927-c7159290fb8c",
				"parent": null,
				"name": "Email Address",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "8856d507-76e0-47c7-8564-56467e717053",
		"alias": "eyeDropperColorPickerDocType",
		"name": "Eye Dropper Color Picker",
		"description": null,
		"icon": "icon-colorpicker color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-66",
				"container": {
					"id": "3f800461-2b29-45a0-a8ad-dc06e43ce6fe"
				},
				"alias": "eyeDropperColorPickerDefaultConfig",
				"name": "Eye Dropper Color Picker - Default Config",
				"description": null,
				"dataType": {
					"id": "b9484af9-4a64-4aa0-a200-2cfd344c1aa5"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-67",
				"container": {
					"id": "3f800461-2b29-45a0-a8ad-dc06e43ce6fe"
				},
				"alias": "eyeDropperColorPickerAlpha",
				"name": "Eye Dropper Color Picker - Alpha",
				"description": null,
				"dataType": {
					"id": "36c5e4da-fdf5-4ad7-8a1a-30bcaf453cc8"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-68",
				"container": {
					"id": "3f800461-2b29-45a0-a8ad-dc06e43ce6fe"
				},
				"alias": "eyeDropperColorPickerPalette",
				"name": "Eye Dropper Color Picker - Palette",
				"description": null,
				"dataType": {
					"id": "dcc7e763-4715-47ad-9215-a626e33a4a9f"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-69",
				"container": {
					"id": "3f800461-2b29-45a0-a8ad-dc06e43ce6fe"
				},
				"alias": "eyeDropperColorPickerFullyConfigured",
				"name": "Eye Dropper Color Picker - Fully Configured",
				"description": null,
				"dataType": {
					"id": "fb266db1-3e18-4cf2-9cf6-6f5096b12076"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "3f800461-2b29-45a0-a8ad-dc06e43ce6fe",
				"parent": null,
				"name": "Eye Dropper Color Pickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "99431793-6f52-48c7-af53-6bf04668aca2",
		"alias": "markdownEditorDocType",
		"name": "Markdown Editor",
		"description": null,
		"icon": "icon-code color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-70",
				"container": {
					"id": "7a2361ed-3f5f-4db5-af90-07f8a7bb60cb"
				},
				"alias": "markdownEditorDefaultConfig",
				"name": "Markdown Editor - Default Config",
				"description": null,
				"dataType": {
					"id": "4f14fe46-522a-4994-ad67-451f78c5d8f6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-71",
				"container": {
					"id": "7a2361ed-3f5f-4db5-af90-07f8a7bb60cb"
				},
				"alias": "markdownEditorFullyConfigured",
				"name": "Markdown Editor - Fully Configured",
				"description": null,
				"dataType": {
					"id": "3af82553-4d1e-42c4-9e93-880de599c7b3"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-72",
				"container": {
					"id": "7a2361ed-3f5f-4db5-af90-07f8a7bb60cb"
				},
				"alias": "markdownEditorDefaultValue",
				"name": "Markdown Editor - Default Value",
				"description": null,
				"dataType": {
					"id": "e56554ed-95a4-43c4-96b4-0122569f4193"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-73",
				"container": {
					"id": "7a2361ed-3f5f-4db5-af90-07f8a7bb60cb"
				},
				"alias": "markdownEditorLargeOverlaySize",
				"name": "Markdown Editor - Large Overlay Size",
				"description": null,
				"dataType": {
					"id": "9450742c-31ce-4db2-85d5-c85238473c39"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-74",
				"container": {
					"id": "7a2361ed-3f5f-4db5-af90-07f8a7bb60cb"
				},
				"alias": "markdownEditorPreview",
				"name": "Markdown Editor - Preview",
				"description": null,
				"dataType": {
					"id": "a5b1b0d5-f905-413f-8e00-aeb8bf4c0999"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "7a2361ed-3f5f-4db5-af90-07f8a7bb60cb",
				"parent": null,
				"name": "Markdown Editors",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "7b61b708-aa42-4978-a86c-f20fd4749a58",
		"alias": "mediaPickerDocType",
		"name": "Media Picker",
		"description": null,
		"icon": "icon-umb-media color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-75",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerDefaultConfig",
				"name": "Media Picker - Default Config",
				"description": null,
				"dataType": {
					"id": "87543f25-f2dc-41b4-b861-75159b7baff9"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-76",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerAcceptedTypes",
				"name": "Media Picker - Accepted Types",
				"description": null,
				"dataType": {
					"id": "ccff0509-5a13-4e86-a341-9b91868922ae"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-77",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerAmount",
				"name": "Media Picker - Amount",
				"description": null,
				"dataType": {
					"id": "ff53e2ac-d54b-4c78-a5cc-2bc050605854"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-78",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerCrops",
				"name": "Media Picker - Crops",
				"description": null,
				"dataType": {
					"id": "0e33a2de-d6a3-4853-8a68-7f56667f1d75"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-79",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerFocalPoint",
				"name": "Media Picker - Focal Point",
				"description": null,
				"dataType": {
					"id": "a6089297-6132-4f18-83da-bf11acb5e8e2"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-80",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerIgnoreUserStartNodes",
				"name": "Media Picker - Ignore User Start Nodes",
				"description": null,
				"dataType": {
					"id": "7c67bee1-7206-40c3-a8f3-cdad500c021b"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 5,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-81",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerPickMultipleItems",
				"name": "Media Picker - Pick Multiple Items",
				"description": null,
				"dataType": {
					"id": "fec81092-5db6-45b2-8050-09dcfead2901"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 6,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-82",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerPickMultipleItemsWithAmount",
				"name": "Media Picker - Pick Multiple Items, With Amount",
				"description": null,
				"dataType": {
					"id": "efe5ab7e-e78a-4b3c-8fe2-3c711e073a5d"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 7,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-83",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerStartNode",
				"name": "Media Picker - Start Node",
				"description": null,
				"dataType": {
					"id": "c686c86b-8941-4656-8f24-eea0afb0abb9"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 8,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-84",
				"container": {
					"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c"
				},
				"alias": "mediaPickerFullyConfigured",
				"name": "Media Picker - Fully Configured",
				"description": null,
				"dataType": {
					"id": "30f7ee63-69da-4b1f-a8f8-4c5ed882be17"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 9,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "a2114ac3-a87a-4c7d-a882-d55cefebbf2c",
				"parent": null,
				"name": "Media Pickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "f7c73e80-e8f4-4ef6-a710-168d89991c7d",
		"alias": "memberGroupPickerDocType",
		"name": "Member Group Picker",
		"description": null,
		"icon": "icon-users-alt color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-85",
				"container": {
					"id": "dc0c9e8c-e488-4a29-ae72-7e5791078719"
				},
				"alias": "memberGroupPicker",
				"name": "Member Group Picker",
				"description": null,
				"dataType": {
					"id": "2ac54465-7f8c-481e-926b-6fcc8bef1dc3"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "dc0c9e8c-e488-4a29-ae72-7e5791078719",
				"parent": null,
				"name": "Member Group Picker",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "48a02560-7ce9-4be4-96e7-e4041cc19622",
		"alias": "memberPickerDocType",
		"name": "Member Picker",
		"description": null,
		"icon": "icon-user color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-86",
				"container": {
					"id": "f516f9ea-e299-4f9d-892e-46c39d93489c"
				},
				"alias": "memberPicker",
				"name": "Member Picker",
				"description": null,
				"dataType": {
					"id": "2555acc6-6adf-4cc3-b0bd-86a2dfdcc7b1"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "f516f9ea-e299-4f9d-892e-46c39d93489c",
				"parent": null,
				"name": "Member Picker",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "727b819b-af42-443f-a752-c4c5cfd69313",
		"alias": "multiUrlPickerDocType",
		"name": "Multi URL Picker",
		"description": null,
		"icon": "icon-link color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-87",
				"container": {
					"id": "5a327c88-8e0f-421b-abf0-01b07533685e"
				},
				"alias": "multiUrlPickerDefaultConfig",
				"name": "Multi URL Picker - Default Config",
				"description": null,
				"dataType": {
					"id": "f455a80c-7f39-4fbb-b212-cf829dd28f7b"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-88",
				"container": {
					"id": "5a327c88-8e0f-421b-abf0-01b07533685e"
				},
				"alias": "multiUrlPickerFullyConfigured",
				"name": "Multi URL Picker - Fully Configured",
				"description": "Min: 2\nMax: 10\nMedium overlay",
				"dataType": {
					"id": "68bee672-9317-40a4-860c-32240d4a2926"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-89",
				"container": {
					"id": "5a327c88-8e0f-421b-abf0-01b07533685e"
				},
				"alias": "multiUrlPickerHideAnchorQueryString",
				"name": "Multi URL Picker - Hide Anchor Query String",
				"description": null,
				"dataType": {
					"id": "f088d56e-9efd-4c4d-8264-accbbe647181"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-90",
				"container": {
					"id": "5a327c88-8e0f-421b-abf0-01b07533685e"
				},
				"alias": "multiUrlPickerIgnoreUserStartNodes",
				"name": "Multi URL Picker - Ignore User Start Nodes",
				"description": null,
				"dataType": {
					"id": "50a7ce3b-ba5a-4f20-8264-361d2194a15c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-91",
				"container": {
					"id": "5a327c88-8e0f-421b-abf0-01b07533685e"
				},
				"alias": "multiUrlPickerLargeOverlaySize",
				"name": "Multi URL Picker - Large Overlay Size",
				"description": null,
				"dataType": {
					"id": "d6d131e2-822f-437a-a1bc-57ddf55e9a5f"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-92",
				"container": {
					"id": "5a327c88-8e0f-421b-abf0-01b07533685e"
				},
				"alias": "multiUrlPickerMinAndMax",
				"name": "Multi URL Picker - Min And Max",
				"description": null,
				"dataType": {
					"id": "5efb7a21-cbfa-452b-a7f0-9b6e467f651c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 5,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "5a327c88-8e0f-421b-abf0-01b07533685e",
				"parent": null,
				"name": "Multi URL Pickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "7025ee6c-8d6c-4abd-8e32-2cab5fde6f90",
		"alias": "multinodeTreepickerDocType",
		"name": "Content Picker",
		"description": null,
		"icon": "icon-page-add color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-182",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerDynamicRoot",
				"name": "Content Picker - Dynamic Root",
				"description": "",
				"dataType": {
					"id": "9e6524d3-40b9-4618-bdb6-9093fe33f133"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-93",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerDefaultConfig",
				"name": "Content Picker - Default Config",
				"description": null,
				"dataType": {
					"id": "fe2a2728-c6bc-450b-9e63-a68d60638b7e"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 5,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-94",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerAllowedTypes",
				"name": "Content Picker - Allowed Types",
				"description": null,
				"dataType": {
					"id": "58269615-9b07-481d-aa44-0613b9157f3e"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-96",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerMinAndMax",
				"name": "Content Picker - Min And Max",
				"description": null,
				"dataType": {
					"id": "07af09f9-819e-404b-b012-226e8978f5c0"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-97",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerStartNode",
				"name": "Content Picker - Start Node",
				"description": null,
				"dataType": {
					"id": "75800052-b5c4-4a82-a6ed-a7972129bcf6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-98",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerXPathStartNode",
				"name": "Content Picker - XPath Start Node",
				"description": null,
				"dataType": {
					"id": "612a8051-191c-4cbc-827f-aae9ec364fbc"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 40,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-95",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerFullyConfigured",
				"name": "Content Picker - Fully Configured",
				"description": null,
				"dataType": {
					"id": "98e2775d-37f6-4625-98f6-1a4cd4f7dac8"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 50,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-99",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerMediaDefaultConfig",
				"name": "Content Picker - Media - Default Config",
				"description": null,
				"dataType": {
					"id": "52d20340-cf21-4256-a136-55f91dbf353a"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 60,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-100",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerMediaFullyConfigured",
				"name": "Content Picker - Media - Fully Configured",
				"description": null,
				"dataType": {
					"id": "67782cbb-ee87-42cf-810a-7aab4b1cbcbc"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 70,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-101",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerMembersDefaultConfig",
				"name": "Content Picker - Members - Default Config",
				"description": null,
				"dataType": {
					"id": "9f1be990-9b28-4817-a662-841875071769"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 80,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-102",
				"container": {
					"id": "c697eddc-c31a-4886-b14c-22cd9718a477"
				},
				"alias": "multinodeTreepickerMembersFullyConfigured",
				"name": "Content Picker - Members - Fully Configured",
				"description": null,
				"dataType": {
					"id": "f6d4679e-2090-4acb-b7c8-564d908c657c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 90,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "c697eddc-c31a-4886-b14c-22cd9718a477",
				"parent": null,
				"name": "Multinode Treepickers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "cc827fc0-e385-494b-88f6-d4abb47b7081",
		"alias": "multipleTextstringDocType",
		"name": "Multiple Textstring",
		"description": null,
		"icon": "icon-ordered-list color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-103",
				"container": {
					"id": "4745f3e2-a4b4-475d-884f-3a615f2e7328"
				},
				"alias": "multipleTextstringDefaultConfig",
				"name": "Multiple Textstring - Default Config",
				"description": null,
				"dataType": {
					"id": "a9c636c7-d500-4ce5-bfb1-2d508fe79d7c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-105",
				"container": {
					"id": "4745f3e2-a4b4-475d-884f-3a615f2e7328"
				},
				"alias": "multipleTextstringMax",
				"name": "Multiple Textstring - Max",
				"description": null,
				"dataType": {
					"id": "b88cc0ae-9216-45de-83c2-5e92de3ae153"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-106",
				"container": {
					"id": "4745f3e2-a4b4-475d-884f-3a615f2e7328"
				},
				"alias": "multipleTextstringMin",
				"name": "Multiple Textstring - Min",
				"description": null,
				"dataType": {
					"id": "26ecc485-c84f-4806-9445-3996da82d0bb"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-104",
				"container": {
					"id": "4745f3e2-a4b4-475d-884f-3a615f2e7328"
				},
				"alias": "multipleTextstringFullyConfigured",
				"name": "Multiple Textstring - Fully Configured",
				"description": null,
				"dataType": {
					"id": "78350acf-b981-4a55-96f8-a91001c73eef"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "4745f3e2-a4b4-475d-884f-3a615f2e7328",
				"parent": null,
				"name": "Multiple Textstrings",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "f984a2dc-01c0-4974-a860-b41dfeacf2b5",
		"alias": "numericDocType",
		"name": "Numeric",
		"description": null,
		"icon": "icon-autofill color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-107",
				"container": {
					"id": "d1812831-2cb7-48bd-83d8-0decd916cd59"
				},
				"alias": "numericDefaultConfig",
				"name": "Numeric - Default Config",
				"description": null,
				"dataType": {
					"id": "fb9142d1-ce19-40dc-84cf-5f81edb11928"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-108",
				"container": {
					"id": "d1812831-2cb7-48bd-83d8-0decd916cd59"
				},
				"alias": "numericMinAndMax",
				"name": "Numeric - Min And Max",
				"description": null,
				"dataType": {
					"id": "1b026e4b-1486-44e1-91c5-62898e813036"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-109",
				"container": {
					"id": "d1812831-2cb7-48bd-83d8-0decd916cd59"
				},
				"alias": "numericStepSize",
				"name": "Numeric - Step Size",
				"description": null,
				"dataType": {
					"id": "5638868a-49df-44b5-8c05-42ca95d8476b"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-110",
				"container": {
					"id": "d1812831-2cb7-48bd-83d8-0decd916cd59"
				},
				"alias": "numericFullyConfigured",
				"name": "Numeric - Fully Configured",
				"description": null,
				"dataType": {
					"id": "4e7ffcd3-e8c6-450f-b5dc-c1bdcd234292"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-218",
				"container": {
					"id": "d1812831-2cb7-48bd-83d8-0decd916cd59"
				},
				"alias": "numericMisconfigured",
				"name": "Numeric - Misconfigured",
				"description": "",
				"dataType": {
					"id": "4e0fd13d-b387-4e69-b241-959e8ca9b022"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "d1812831-2cb7-48bd-83d8-0decd916cd59",
				"parent": null,
				"name": "Numerics",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "7b52f09a-3034-43d6-a83e-5f9fadfcc87d",
		"alias": "radioButtonListDocType",
		"name": "Radio Button List",
		"description": null,
		"icon": "icon-target color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-111",
				"container": {
					"id": "1c2c5bf0-b8a2-48eb-945b-1e872772c850"
				},
				"alias": "radioButtonList",
				"name": "Radio Button List",
				"description": null,
				"dataType": {
					"id": "2d3a109a-de8d-4c12-af67-4f7a116cebe5"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": "",
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-167",
				"container": {
					"id": "1c2c5bf0-b8a2-48eb-945b-1e872772c850"
				},
				"alias": "radioButtonList2",
				"name": "Radio Button List 2",
				"description": "",
				"dataType": {
					"id": "2d3a109a-de8d-4c12-af67-4f7a116cebe5"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "1c2c5bf0-b8a2-48eb-945b-1e872772c850",
				"parent": null,
				"name": "Radio Button List",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [
			{
				"id": "2829dfed-8894-4cdc-825b-cfb5c53f5771"
			}
		],
		"defaultTemplate": {
			"id": "2829dfed-8894-4cdc-825b-cfb5c53f5771"
		},
		"id": "fd62fafc-9cfd-470a-a260-93af5d1ed641",
		"alias": "richTextEditorDocType",
		"name": "Rich Text Editor TinyMCE",
		"description": null,
		"icon": "icon-browser-window color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-115",
				"container": {
					"id": "3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b"
				},
				"alias": "richTextEditorFullyConfigured",
				"name": "Rich Text Editor - Fully Configured",
				"description": null,
				"dataType": {
					"id": "dcede488-62e2-49ba-97a0-59c60ae09992"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-112",
				"container": {
					"id": "3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b"
				},
				"alias": "richTextEditorDefaultConfig",
				"name": "Rich Text Editor - Default Config",
				"description": null,
				"dataType": {
					"id": "e9f410d1-1f37-401d-b3b3-4678e4aab5fa"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-113",
				"container": {
					"id": "3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b"
				},
				"alias": "richTextEditorDimensions",
				"name": "Rich Text Editor - Dimensions",
				"description": null,
				"dataType": {
					"id": "b005391c-4735-4967-9c16-939dc846acac"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-114",
				"container": {
					"id": "3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b"
				},
				"alias": "richTextEditorStylesheets",
				"name": "Rich Text Editor - Stylesheets",
				"description": null,
				"dataType": {
					"id": "ae438007-05c6-4692-afc8-b86201a08882"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 40,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-207",
				"container": {
					"id": "3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b"
				},
				"alias": "richTextEditorBlocksConfig",
				"name": "Rich Text Editor - Blocks Config",
				"description": "",
				"dataType": {
					"id": "e0f55305-5f94-4f6b-a2c1-76628e47e9eb"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 50,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b",
				"parent": null,
				"name": "Rich Text Editors",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "23c4c503-bcdf-46a5-9ff9-fb78d9dba4ae",
		"alias": "sliderDocType",
		"name": "Slider",
		"description": null,
		"icon": "icon-navigation-horizontal color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-116",
				"container": {
					"id": "cb366883-9d00-4d98-8d1f-2a10337a63ff"
				},
				"alias": "sliderDefaultConfig",
				"name": "Slider - Default Config",
				"description": null,
				"dataType": {
					"id": "afc36215-ae90-4290-bf1a-af6ce0ca3c67"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-117",
				"container": {
					"id": "cb366883-9d00-4d98-8d1f-2a10337a63ff"
				},
				"alias": "sliderInitialValue",
				"name": "Slider - Initial Value",
				"description": null,
				"dataType": {
					"id": "1158cb90-ad8e-4c62-9ed3-085691d61a39"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-118",
				"container": {
					"id": "cb366883-9d00-4d98-8d1f-2a10337a63ff"
				},
				"alias": "sliderMinAndMax",
				"name": "Slider - Min And Max",
				"description": null,
				"dataType": {
					"id": "bf4179df-26ec-4c84-9607-0071785730dc"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-119",
				"container": {
					"id": "cb366883-9d00-4d98-8d1f-2a10337a63ff"
				},
				"alias": "sliderStepIncrements",
				"name": "Slider - Step Increments",
				"description": null,
				"dataType": {
					"id": "17d1054c-423a-4f89-a917-cbc055af6fa9"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-120",
				"container": {
					"id": "cb366883-9d00-4d98-8d1f-2a10337a63ff"
				},
				"alias": "sliderFullyConfigured",
				"name": "Slider - Fully Configured",
				"description": null,
				"dataType": {
					"id": "dae71dda-3838-459e-b018-c432952288b0"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "cb366883-9d00-4d98-8d1f-2a10337a63ff",
				"parent": null,
				"name": "Sliders",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "af83a333-d5f9-4467-9013-9eaa8112a571",
		"alias": "textareaDocType",
		"name": "Textarea",
		"description": null,
		"icon": "icon-application-window-alt color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-121",
				"container": {
					"id": "1f6e9b00-2294-48f0-bd40-7bc118b992ad"
				},
				"alias": "textareaDefaultConfig",
				"name": "Textarea - Default Config",
				"description": null,
				"dataType": {
					"id": "4a0ca16a-71bc-471b-b002-5a4b1999d2fa"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-123",
				"container": {
					"id": "1f6e9b00-2294-48f0-bd40-7bc118b992ad"
				},
				"alias": "textareaMaxChars",
				"name": "Textarea - Max Chars",
				"description": null,
				"dataType": {
					"id": "55dcc374-b218-450d-90c3-db3d163dde02"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-124",
				"container": {
					"id": "1f6e9b00-2294-48f0-bd40-7bc118b992ad"
				},
				"alias": "textareaRows",
				"name": "Textarea - Rows",
				"description": null,
				"dataType": {
					"id": "0afa9c4b-e2f2-450b-bb13-2e2684a5fe65"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-122",
				"container": {
					"id": "1f6e9b00-2294-48f0-bd40-7bc118b992ad"
				},
				"alias": "textareaFullyConfigured",
				"name": "Textarea - Fully Configured",
				"description": null,
				"dataType": {
					"id": "0fa454ae-8671-4c69-a754-7ee738bab707"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-219",
				"container": {
					"id": "1f6e9b00-2294-48f0-bd40-7bc118b992ad"
				},
				"alias": "textareaMisconfigured",
				"name": "Textarea - Misconfigured",
				"description": "",
				"dataType": {
					"id": "0ed2de25-fc8f-41eb-a2c6-2ea299df9201"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "1f6e9b00-2294-48f0-bd40-7bc118b992ad",
				"parent": null,
				"name": "Textareas",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "373eaceb-e41e-4dd2-ae3f-b73fd11cf182",
		"alias": "toggleDocType",
		"name": "Toggle",
		"description": null,
		"icon": "icon-checkbox color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-125",
				"container": {
					"id": "5abf162a-2d2c-43da-ac2a-21d1cb4c2f04"
				},
				"alias": "toggleDefaultConfig",
				"name": "Toggle - Default Config",
				"description": null,
				"dataType": {
					"id": "b6c1b8fc-7aaf-4177-b916-44cda751436d"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-127",
				"container": {
					"id": "5abf162a-2d2c-43da-ac2a-21d1cb4c2f04"
				},
				"alias": "toggleInitialState",
				"name": "Toggle - Initial State",
				"description": null,
				"dataType": {
					"id": "768f8670-982d-4911-886c-6f97fedf022a"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-128",
				"container": {
					"id": "5abf162a-2d2c-43da-ac2a-21d1cb4c2f04"
				},
				"alias": "toggleLabels",
				"name": "Toggle - Labels",
				"description": null,
				"dataType": {
					"id": "1522cae7-487c-427c-9bd1-aecc5eb7fab9"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-126",
				"container": {
					"id": "5abf162a-2d2c-43da-ac2a-21d1cb4c2f04"
				},
				"alias": "toggleFullyConfigured",
				"name": "Toggle - Fully Configured",
				"description": null,
				"dataType": {
					"id": "afb3d794-8214-4888-8030-b3def1e5fe27"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "5abf162a-2d2c-43da-ac2a-21d1cb4c2f04",
				"parent": null,
				"name": "Toggles",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "6717ef28-57a2-4cb4-80fe-ddc7a76da5f4",
		"alias": "textboxDocType",
		"name": "Textbox",
		"description": null,
		"icon": "icon-autofill color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-129",
				"container": {
					"id": "aa20dc91-4bb6-4ec4-980a-a1af95d5b6d8"
				},
				"alias": "textboxDefaultConfig",
				"name": "Textbox - Default Config",
				"description": null,
				"dataType": {
					"id": "3a9b04b9-e96a-46d0-bd84-2c13fc36b70c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-130",
				"container": {
					"id": "aa20dc91-4bb6-4ec4-980a-a1af95d5b6d8"
				},
				"alias": "textboxMaxChars",
				"name": "Textbox - Max Chars",
				"description": null,
				"dataType": {
					"id": "75c0a6a9-61a4-49e2-a8a9-5f19b72760e5"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "aa20dc91-4bb6-4ec4-980a-a1af95d5b6d8",
				"parent": null,
				"name": "Textboxes",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "b85bb884-ed5e-4f0b-8b10-8067090e8ada",
		"alias": "checkboxListDocType",
		"name": "Checkbox List",
		"description": null,
		"icon": "icon-bulleted-list color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-131",
				"container": {
					"id": "4b4223ef-7957-4be6-9a44-568aa6930b4f"
				},
				"alias": "checkboxList",
				"name": "Checkbox List",
				"description": null,
				"dataType": {
					"id": "dfa2595b-165c-48a7-b6ff-820914484c12"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": "Yo! Please select an option.",
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "4b4223ef-7957-4be6-9a44-568aa6930b4f",
				"parent": null,
				"name": "Checkbox List",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "6dcde803-d22e-4fcf-85a3-3a03be080d3a",
		"alias": "tagsDocType",
		"name": "Tags",
		"description": null,
		"icon": "icon-tags color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-132",
				"container": {
					"id": "26e56467-05e0-4e57-853c-f48e8148e8aa"
				},
				"alias": "tagsDefaultGroupJSONStorage",
				"name": "Tags - Default Group, JSON Storage",
				"description": null,
				"dataType": {
					"id": "965dc042-216d-4b0b-84ff-fb0050aed8e8"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-133",
				"container": {
					"id": "26e56467-05e0-4e57-853c-f48e8148e8aa"
				},
				"alias": "tagsCustomGroupCSVStorage",
				"name": "Tags - Custom Group, CSV Storage",
				"description": null,
				"dataType": {
					"id": "c6d5b717-ad81-431f-81e4-b289801eb802"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "26e56467-05e0-4e57-853c-f48e8148e8aa",
				"parent": null,
				"name": "Tags",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "dc965257-84c2-4f27-b452-55e8b0f91a96",
		"alias": "userPickerDocType",
		"name": "User Picker",
		"description": null,
		"icon": "icon-user color-green",
		"allowedAsRoot": false,
		"variesByCulture": true,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-134",
				"container": {
					"id": "b439bb1f-215f-4dcc-9db5-d27354ad61ef"
				},
				"alias": "userPicker",
				"name": "User Picker",
				"description": null,
				"dataType": {
					"id": "3387e5da-4e32-43dc-b4dc-840fcbc468f9"
				},
				"variesByCulture": true,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "b439bb1f-215f-4dcc-9db5-d27354ad61ef",
				"parent": null,
				"name": "User Picker",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [
			{
				"documentType": {
					"id": "dc965257-84c2-4f27-b452-55e8b0f91a96"
				},
				"sortOrder": 0
			}
		],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "25dd3762-cfdd-43cd-b0a5-8f094f8a7fd2",
		"alias": "labelDocType",
		"name": "Label",
		"description": null,
		"icon": "icon-readonly color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-135",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelString",
				"name": "Label - String",
				"description": null,
				"dataType": {
					"id": "f0bc4bfb-b499-40d6-ba86-058885a5178c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-136",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelDecimal",
				"name": "Label - Decimal",
				"description": null,
				"dataType": {
					"id": "3145614f-1e5e-47d2-a587-d7eb2d937a8f"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-137",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelDateTime",
				"name": "Label - DateTime",
				"description": null,
				"dataType": {
					"id": "d9e26ead-b55f-4b24-96be-3da10ce87241"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-138",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelTime",
				"name": "Label - Time",
				"description": null,
				"dataType": {
					"id": "3c78f54b-0812-4f7c-a483-29c54583bf9f"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 3,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-139",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelInteger",
				"name": "Label - Integer",
				"description": null,
				"dataType": {
					"id": "0169a2ba-63b5-442d-af00-98d54bf959d9"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 4,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-140",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelBigInteger",
				"name": "Label - Big Integer",
				"description": null,
				"dataType": {
					"id": "59171d58-e368-42dd-88a9-a3504561442c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 5,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-141",
				"container": {
					"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac"
				},
				"alias": "labelLongString",
				"name": "Label - Long String",
				"description": null,
				"dataType": {
					"id": "b76769c6-9662-4848-afce-2c06a7464bf8"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 6,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac",
				"parent": null,
				"name": "Labels",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "2a773487-9de7-403c-9207-54f4ace7f215",
		"alias": "imageCropper",
		"name": "Image Cropper",
		"description": null,
		"icon": "icon-crop color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-142",
				"container": {
					"id": "e8964451-7d12-407d-a963-cdbfee0eba35"
				},
				"alias": "imageCropperWithoutCrops",
				"name": "Image Cropper -  Without Crops",
				"description": null,
				"dataType": {
					"id": "eae1607c-15e9-47f3-bc03-88a9480dcee6"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": true,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-143",
				"container": {
					"id": "e8964451-7d12-407d-a963-cdbfee0eba35"
				},
				"alias": "imageCropperWithCrops",
				"name": "Image Cropper - With Crops",
				"description": null,
				"dataType": {
					"id": "444a843c-93fa-4589-9e66-949cca3f0e84"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "e8964451-7d12-407d-a963-cdbfee0eba35",
				"parent": null,
				"name": "Image Croppers",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "61c6b912-8fe8-4e10-a07b-4f777b99489b",
		"alias": "blockListDocType",
		"name": "Block List",
		"description": "This is a description for the block list page.",
		"icon": "icon-bulleted-list color-green",
		"allowedAsRoot": false,
		"variesByCulture": true,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-146",
				"container": {
					"id": "7e89bad4-a030-4878-a293-bae51cccb74e"
				},
				"alias": "blockListDefaultConfig",
				"name": "Block List - Default Config",
				"description": "",
				"dataType": {
					"id": "f955664b-9ab0-4f76-b9d6-5742c44a073c"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-147",
				"container": {
					"id": "7e89bad4-a030-4878-a293-bae51cccb74e"
				},
				"alias": "blockListMinAndMax",
				"name": "Block List - Min And Max",
				"description": null,
				"dataType": {
					"id": "44aae492-649c-4895-8c0a-01d29adf995f"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 20,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-148",
				"container": {
					"id": "7e89bad4-a030-4878-a293-bae51cccb74e"
				},
				"alias": "blockListSingleTypeOnly",
				"name": "Block List - Single Type Only",
				"description": null,
				"dataType": {
					"id": "135b9faf-a464-4e5a-a02f-6d17b1d807cf"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 30,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-190",
				"container": {
					"id": "7e89bad4-a030-4878-a293-bae51cccb74e"
				},
				"alias": "blockListInlineMode",
				"name": "Block List - Inline Mode",
				"description": "",
				"dataType": {
					"id": "ecd9355b-b9e6-4bfd-889e-aa4642e02c80"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 40,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "7e89bad4-a030-4878-a293-bae51cccb74e",
				"parent": null,
				"name": "Block Lists",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "1addd0ad-0e34-4386-801b-79cf7beb8cf1",
		"alias": "blockGrid",
		"name": "Block Grid",
		"description": "This is a description for the block grid page.",
		"icon": "icon-grid color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-149",
				"container": {
					"id": "b136c5af-636d-4625-a301-6a1e04ba548d"
				},
				"alias": "blockGridDefaultConfig",
				"name": "Block Grid - Default Config",
				"description": "This is the default configuration of a Block Grid property-editor.",
				"dataType": {
					"id": "5555758f-d1c7-4480-a8ee-391fba1ac2ca"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-150",
				"container": {
					"id": "b136c5af-636d-4625-a301-6a1e04ba548d"
				},
				"alias": "blockGridWithAreas",
				"name": "Block Grid - With Areas",
				"description": null,
				"dataType": {
					"id": "45ea7c93-cb3e-44d8-ada3-4382dfdc69f1"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-170",
				"container": {
					"id": "b136c5af-636d-4625-a301-6a1e04ba548d"
				},
				"alias": "blockGridEmpty",
				"name": "Block Grid - Empty",
				"description": "",
				"dataType": {
					"id": "a6bf4d6e-23fb-4fe7-9e58-4cae964da4aa"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 2,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "b136c5af-636d-4625-a301-6a1e04ba548d",
				"parent": null,
				"name": "Block Grids",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "11b48beb-3fd0-4b72-800e-364f6e833dc7",
		"alias": "fileUpload",
		"name": "File Upload",
		"description": null,
		"icon": "icon-download-alt color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-151",
				"container": {
					"id": "9849ef3a-73a1-46c4-b444-e4493f02b3c5"
				},
				"alias": "fileUploadDefaultConfig",
				"name": "File Upload - Default Config",
				"description": null,
				"dataType": {
					"id": "2ece7647-e59f-44d1-952e-c28cf763a375"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			},
			{
				"id": "pt-152",
				"container": {
					"id": "9849ef3a-73a1-46c4-b444-e4493f02b3c5"
				},
				"alias": "fileUploadSpecificFileTypes",
				"name": "File Upload - Specific File Types",
				"description": null,
				"dataType": {
					"id": "9510982e-4e51-48d7-bf37-13a189d72658"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 1,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "9849ef3a-73a1-46c4-b444-e4493f02b3c5",
				"parent": null,
				"name": "File Uploads",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "e355cffc-e70c-4e36-946a-9d448f392b89",
		"alias": "combobox",
		"name": "Combobox",
		"description": null,
		"icon": "icon-thumbnail-list color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-168",
				"container": {
					"id": "a00f85d5-fcee-4c52-aa2d-aa432a492695"
				},
				"alias": "comboboxDefaultConfig",
				"name": "Combobox - Default Config",
				"description": "",
				"dataType": {
					"id": "88c96046-c994-4017-96bd-2b7e62d0dd5a"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "a00f85d5-fcee-4c52-aa2d-aa432a492695",
				"parent": null,
				"name": "Combobox",
				"type": "Tab",
				"sortOrder": 0
			},
			{
				"id": "d7edf3b0-b27b-4a15-a17d-82e50cf6ff03",
				"parent": null,
				"name": "Combobox",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [],
		"defaultTemplate": null,
		"id": "8f3cd603-85af-4784-bfb7-cf966cdd6ac7",
		"alias": "codeEditor",
		"name": "Code Editor",
		"description": null,
		"icon": "icon-brackets color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-717",
				"container": {
					"id": "31417d76-88b6-40ba-8b91-68612cab8fc6"
				},
				"alias": "codeEditorDefaultConfig",
				"name": "Code Editor - Default Config",
				"description": "",
				"dataType": {
					"id": "3a389162-9e57-4021-be97-070a82faf184"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 0,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "31417d76-88b6-40ba-8b91-68612cab8fc6",
				"parent": null,
				"name": "Code Editor",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	},
	{
		"allowedTemplates": [
			{
				"id": "a83e40ed-a267-4d78-9db0-d309f0e814ff"
			}
		],
		"defaultTemplate": {
			"id": "a83e40ed-a267-4d78-9db0-d309f0e814ff"
		},
		"id": "c07f8d38-302f-4e4a-bd84-d57d79a4af46",
		"alias": "richTextEditorTiptap",
		"name": "Rich Text Editor Tiptap",
		"description": null,
		"icon": "icon-application-window color-green",
		"allowedAsRoot": false,
		"variesByCulture": false,
		"variesBySegment": false,
		"isElement": false,
		"hasChildren": false,
		"parent": {
			"id": "25b36f28-5051-4073-a0c7-3887f6f8c695"
		},
		"isFolder": false,
		"properties": [
			{
				"id": "pt-205",
				"container": {
					"id": "3333fd0a-1a8e-48c7-bf4e-b4a0e8746f1c"
				},
				"alias": "richTextEditorTiptapDefaultConfig",
				"name": "Rich Text Editor Tiptap - Default Config",
				"description": "",
				"dataType": {
					"id": "6c85876b-5a9e-4a8a-8ec2-78162cfb7ef4"
				},
				"variesByCulture": false,
				"variesBySegment": false,
				"sortOrder": 10,
				"validation": {
					"mandatory": false,
					"mandatoryMessage": null,
					"regEx": null,
					"regExMessage": null
				},
				"appearance": {
					"labelOnTop": false
				}
			}
		],
		"containers": [
			{
				"id": "3333fd0a-1a8e-48c7-bf4e-b4a0e8746f1c",
				"parent": null,
				"name": "Rich Text Editors",
				"type": "Group",
				"sortOrder": 0
			}
		],
		"allowedDocumentTypes": [],
		"compositions": [],
		"cleanup": {
			"preventCleanup": false,
			"keepAllVersionsNewerThanDays": null,
			"keepLatestVersionPerDayForDays": null
		},
		"flags": []
	}
];

export const data: Array<UmbMockDocumentTypeModel> = rawData.map(dt => ({
	...dt,
	compositions: dt.compositions.map(c => ({
		...c,
		compositionType: mapCompositionType(c.compositionType),
	})),
}));
