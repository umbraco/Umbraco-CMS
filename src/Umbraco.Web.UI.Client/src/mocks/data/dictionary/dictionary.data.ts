import type {
	DictionaryItemItemResponseModel,
	DictionaryItemResponseModel,
	DictionaryOverviewResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockDictionaryModel = DictionaryItemResponseModel &
	NamedEntityTreeItemResponseModel &
	DictionaryItemItemResponseModel &
	DictionaryOverviewResponseModel;

export const data: Array<UmbMockDictionaryModel> = [
	{
		name: 'Forbidden',
		id: 'forbidden',
		parent: null,
		hasChildren: false,
		translatedIsoCodes: ['en-us'],
		translations: [
			{
				isoCode: 'en-us',
				translation: 'This is a forbidden dictionary item',
			},
			{
				isoCode: 'da',
				translation: 'Dette er et forbudt ordbogsobjekt',
			},
		],
		flags: [],
	},
	{
		name: 'Hello',
		id: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		parent: null,
		hasChildren: false,
		translatedIsoCodes: ['en-us'],
		translations: [
			{
				isoCode: 'en-us',
				translation: 'hello in en',
			},
			{
				isoCode: 'da',
				translation: '',
			},
		],
		flags: [],
	},
	{
		name: 'Hello again',
		id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		parent: null,
		hasChildren: true,
		translatedIsoCodes: ['en-us', 'da'],
		translations: [
			{
				isoCode: 'en-us',
				translation: 'Hello again in en',
			},
			{
				isoCode: 'da',
				translation: 'Hello in da',
			},
		],
		flags: [],
	},
	{
		name: 'Nested Hello again',
		id: '438b8693-2156-482b-84af-ccdae0c2df6e',
		parent: { id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb' },
		hasChildren: false,
		translatedIsoCodes: ['en-us', 'da'],
		translations: [
			{
				isoCode: 'en-us',
				translation: 'Nested Hello again in en',
			},
			{
				isoCode: 'da',
				translation: 'Nested Hello in da',
			},
		],
		flags: [],
	},
];
