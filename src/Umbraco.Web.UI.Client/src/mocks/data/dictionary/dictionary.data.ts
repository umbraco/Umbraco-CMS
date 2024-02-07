import type {
	DictionaryItemItemResponseModel,
	DictionaryItemResponseModel,
	DictionaryOverviewResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type UmbMockDictionaryModelHack = DictionaryItemResponseModel &
	NamedEntityTreeItemResponseModel &
	DictionaryItemItemResponseModel &
	DictionaryOverviewResponseModel;

export interface UmbMockDictionaryModel extends Omit<UmbMockDictionaryModelHack, 'type'> {}

export const data: Array<UmbMockDictionaryModel> = [
	{
		name: 'Hello',
		id: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		parent: null,
		hasChildren: false,
		translatedIsoCodes: ['en'],
		translations: [
			{
				isoCode: 'en',
				translation: 'hello in en',
			},
			{
				isoCode: 'da',
				translation: '',
			},
		],
	},
	{
		name: 'Hello again',
		id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		parent: null,
		hasChildren: true,
		translatedIsoCodes: ['en', 'da'],
		translations: [
			{
				isoCode: 'en',
				translation: 'Hello again in en',
			},
			{
				isoCode: 'da',
				translation: 'Hello in da',
			},
		],
	},
	{
		name: 'Nested Hello again',
		id: '438b8693-2156-482b-84af-ccdae0c2df6e',
		parent: { id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb' },
		hasChildren: false,
		translatedIsoCodes: ['en', 'da'],
		translations: [
			{
				isoCode: 'en',
				translation: 'Nested Hello again in en',
			},
			{
				isoCode: 'da',
				translation: 'Nested Hello in da',
			},
		],
	},
];
