import { Language } from '@umbraco-cms/backend-api';
import { UmbData } from './data';
import { v4 as uuidv4 } from 'uuid';
import { LanguageDetails } from '@umbraco-cms/models';

// Temp mocked database
class UmbLanguagesData extends UmbData<LanguageDetails> {
	constructor(data: LanguageDetails[]) {
		super(data);
	}

	// skip can be number or null
	getAll(skip = 0, take = this.data.length): Array<LanguageDetails> {
		return this.data.splice(skip, take);
	}

	getByKey(key: string) {
		return this.data.find((item) => item.key === key);
	}
}

export const data: Array<LanguageDetails> = [
	{
		id: 1,
		key: uuidv4(),
		name: 'English',
		isoCode: 'en',
		isDefault: true,
		isMandatory: true,
	},
	{
		id: 2,
		key: uuidv4(),
		name: 'Danish',
		isoCode: 'da',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
	{
		id: 3,
		key: uuidv4(),
		name: 'German',
		isoCode: 'de',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
	{
		id: 4,
		key: uuidv4(),
		name: 'French',
		isoCode: 'fr',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
];

export const umbLanguagesData = new UmbLanguagesData(data);
