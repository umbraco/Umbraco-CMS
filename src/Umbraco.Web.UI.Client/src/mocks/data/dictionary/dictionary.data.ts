import type {
	DictionaryItemItemResponseModel,
	DictionaryItemResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type UmbMockDictionaryModelHack = DictionaryItemResponseModel &
	NamedEntityTreeItemResponseModel &
	DictionaryItemItemResponseModel;

export interface UmbMockDictionaryModel extends Omit<UmbMockDictionaryModelHack, 'type'> {}

export const data: Array<UmbMockDictionaryModel> = [
	{
		name: 'Hello',
		id: 'aae7d0ab-53ba-485d-b8bd-12537f9925cb',
		parent: null,
		hasChildren: false,
		translations: [
			{
				isoCode: 'en',
				translation: 'hello in en-US',
			},
			{
				isoCode: 'fr',
				translation: '',
			},
		],
	},
	{
		name: 'Hello again',
		id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb',
		parent: null,
		hasChildren: true,
		translations: [
			{
				isoCode: 'en',
				translation: 'Hello again in en-US',
			},
			{
				isoCode: 'fr',
				translation: 'Hello in fr',
			},
		],
	},
	{
		name: 'Nested Hello again',
		id: '438b8693-2156-482b-84af-ccdae0c2df6e',
		parent: { id: 'bbe7d0ab-53bb-485d-b8bd-12537f9925cb' },
		hasChildren: false,
		translations: [
			{
				isoCode: 'en',
				translation: 'Nested Hello again in en-US',
			},
			{
				isoCode: 'fr',
				translation: 'Nested Hello in fr',
			},
		],
	},
];
