import type { UmbMockDictionaryModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockDictionaryModel> = [
	{
		"id": "37a410ad-7418-4808-a952-1cc1bc31c949",
		"parent": null,
		"name": "Hello",
		"hasChildren": true,
		"translatedIsoCodes": [
			"en-US",
			"da-dk"
		],
		"translations": [
			{
				"isoCode": "en-US",
				"translation": "Hello!!???!!!fff"
			},
			{
				"isoCode": "da-dk",
				"translation": "Hej!"
			}
		],
		"flags": []
	},
	{
		"id": "9485dd99-5d4a-49a3-a3dc-b62d704da5ea",
		"parent": null,
		"name": "Foo",
		"hasChildren": true,
		"translatedIsoCodes": [
			"en-US",
			"da-dk"
		],
		"translations": [
			{
				"isoCode": "en-US",
				"translation": "Foo!!???"
			},
			{
				"isoCode": "da-dk",
				"translation": "Fóú!"
			}
		],
		"flags": []
	},
	{
		"id": "1c1b545c-6826-4a7b-8d61-ac5cc84fa1f8",
		"parent": null,
		"name": "Bar",
		"hasChildren": false,
		"translatedIsoCodes": [
			"en-US",
			"da-dk"
		],
		"translations": [
			{
				"isoCode": "en-US",
				"translation": "Bar!?!!!"
			},
			{
				"isoCode": "da-dk",
				"translation": "Bár???"
			}
		],
		"flags": []
	},
	{
		"id": "0fd59d11-b891-44e1-8e39-e2a7323afdb4",
		"parent": {
			"id": "9485dd99-5d4a-49a3-a3dc-b62d704da5ea"
		},
		"name": "Zero123",
		"hasChildren": false,
		"translatedIsoCodes": [
			"en-US",
			"da-dk"
		],
		"translations": [
			{
				"isoCode": "en-US",
				"translation": "Zero???%%%"
			},
			{
				"isoCode": "da-dk",
				"translation": "Zero!!!!££££"
			}
		],
		"flags": []
	},
	{
		"id": "03779703-274f-4ddd-a0fd-eac982b7a8a6",
		"parent": {
			"id": "37a410ad-7418-4808-a952-1cc1bc31c949"
		},
		"name": "<script>alert(\"XSS\");</script>",
		"hasChildren": false,
		"translatedIsoCodes": [
			"en-US",
			"da-dk"
		],
		"translations": [
			{
				"isoCode": "en-US",
				"translation": "<script>alert(\"XSS EN\");</script>"
			},
			{
				"isoCode": "da-dk",
				"translation": "<script>alert(\"XSS DA\");</script>"
			}
		],
		"flags": []
	}
];
