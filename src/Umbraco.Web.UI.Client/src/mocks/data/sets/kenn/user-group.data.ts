import type { UmbMockUserGroupModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockUserGroupModel> = [
	{
		"id": "e5e7f6c8-7f9c-4b5b-8d5d-9e1e5a4f7e4d",
		"name": "Administrators",
		"alias": "admin",
		"description": null,
		"icon": "icon-medal",
		"fallbackPermissions": [],
		"permissions": [],
		"sections": [
			"Umb.Section.Content",
			"Umb.Section.Media",
			"Umb.Section.Settings",
			"Umb.Section.Members",
			"Umb.Section.Packages",
			"Umb.Section.Translation",
			"Umb.Section.Users"
		],
		"languages": [],
		"hasAccessToAllLanguages": true,
		"documentRootAccess": false,
		"mediaRootAccess": false,
		"documentStartNode": {
			"id": "-1"
		},
		"mediaStartNode": {
			"id": "-1"
		},
		"aliasCanBeChanged": false,
		"isDeletable": false,
		"flags": []
	},
	{
		"id": "9fc2a16f-528c-46d6-a014-75bf4ec2480c",
		"name": "Writers",
		"alias": "writer",
		"description": null,
		"icon": "icon-edit",
		"fallbackPermissions": [],
		"permissions": [],
		"sections": [
			"Umb.Section.Content"
		],
		"languages": [],
		"hasAccessToAllLanguages": true,
		"documentRootAccess": false,
		"mediaRootAccess": false,
		"documentStartNode": {
			"id": "-1"
		},
		"mediaStartNode": {
			"id": "-1"
		},
		"aliasCanBeChanged": true,
		"isDeletable": true,
		"flags": []
	},
	{
		"id": "44dc260e-b4d4-4dd9-9081-eec5598f1641",
		"name": "Editors",
		"alias": "editor",
		"description": null,
		"icon": "icon-tools",
		"fallbackPermissions": [],
		"permissions": [],
		"sections": [
			"Umb.Section.Content",
			"Umb.Section.Media"
		],
		"languages": [],
		"hasAccessToAllLanguages": true,
		"documentRootAccess": false,
		"mediaRootAccess": false,
		"documentStartNode": {
			"id": "-1"
		},
		"mediaStartNode": {
			"id": "-1"
		},
		"aliasCanBeChanged": true,
		"isDeletable": true,
		"flags": []
	},
	{
		"id": "f2012e4c-d232-4bd1-8eae-4384032d97d8",
		"name": "Translators",
		"alias": "translator",
		"description": null,
		"icon": "icon-globe",
		"fallbackPermissions": [],
		"permissions": [],
		"sections": [
			"Umb.Section.Translation"
		],
		"languages": [],
		"hasAccessToAllLanguages": true,
		"documentRootAccess": false,
		"mediaRootAccess": false,
		"documentStartNode": {
			"id": "-1"
		},
		"mediaStartNode": {
			"id": "-1"
		},
		"aliasCanBeChanged": true,
		"isDeletable": true,
		"flags": []
	},
	{
		"id": "8c6ad70f-d307-4e4a-af58-72c2e4e9439d",
		"name": "Sensitive data",
		"alias": "sensitiveData",
		"description": null,
		"icon": "icon-lock",
		"fallbackPermissions": [],
		"permissions": [],
		"sections": [],
		"languages": [],
		"hasAccessToAllLanguages": false,
		"documentRootAccess": true,
		"mediaRootAccess": true,
		"aliasCanBeChanged": false,
		"isDeletable": false,
		"flags": []
	},
	{
		"id": "2c75770e-5009-4e8c-b9e0-fb90520862a0",
		"name": "Custom",
		"alias": "custom",
		"description": null,
		"icon": "icon-ball color-red",
		"fallbackPermissions": [],
		"permissions": [],
		"sections": [
			"Umb.Section.Content"
		],
		"languages": [],
		"hasAccessToAllLanguages": false,
		"documentRootAccess": false,
		"mediaRootAccess": true,
		"documentStartNode": {
			"id": "1060"
		},
		"aliasCanBeChanged": true,
		"isDeletable": true,
		"flags": []
	}
];
