import { Language } from '@umbraco-cms/backend-api';
import { UmbData } from './data';

// Temp mocked database
class UmbLanguagesData extends UmbData<Language> {
	constructor(data: Language[]) {
		super(data);
	}

	// skip can be number or null
	getAll(skip = 0, take = this.data.length): Array<Language> {
		return this.data.splice(skip, take);
	}
}

export const data: Array<Language> = [
	{
		id: 1,
		name: 'English',
		isoCode: 'en',
		isDefault: true,
		isMandatory: true,
	},
	{
		id: 2,
		name: 'Danish',
		isoCode: 'da',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
	{
		id: 3,
		name: 'German',
		isoCode: 'de',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
	{
		id: 4,
		name: 'French',
		isoCode: 'fr',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
];

export const umbLanguagesData = new UmbLanguagesData(data);
