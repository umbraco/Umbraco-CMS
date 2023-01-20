import { UmbData } from './data';
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

	save(saveItems: Array<LanguageDetails>) {
		saveItems.forEach((saveItem) => {
			const foundIndex = this.data.findIndex((item) => item.key === saveItem.key);
			if (foundIndex !== -1) {
				// update
				this.data[foundIndex] = saveItem;
				this.updateData(saveItem);
			} else {
				this.data.push(saveItem);
			}
		});

		return saveItems;
	}

	updateData(updateItem: LanguageDetails) {
		const itemIndex = this.data.findIndex((item) => item.key === updateItem.key);
		const item = this.data[itemIndex];
		if (!item) return;

		const itemKeys = Object.keys(item);
		const newItem = {};

		for (const [key] of Object.entries(updateItem)) {
			if (itemKeys.indexOf(key) !== -1) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				newItem[key] = updateItem[key];
			}
		}

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.data[itemIndex] = newItem;
	}
}

export const data: Array<LanguageDetails> = [
	{
		id: 1,
		key: 'asdail12h3k1h23k12h3',
		name: 'English',
		isoCode: 'en',
		isDefault: true,
		isMandatory: true,
	},
	{
		id: 2,
		key: 'kajshdkjashdkuahwdu',
		name: 'Danish',
		isoCode: 'da',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
	{
		id: 3,
		key: 'k12n3kj12h3123n9812h3',
		name: 'German',
		isoCode: 'de',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
	{
		id: 4,
		key: '1kl2n31231iuqshdiuashd',
		name: 'French',
		isoCode: 'fr',
		isDefault: false,
		isMandatory: false,
		fallbackLanguageId: 1,
	},
];

export const umbLanguagesData = new UmbLanguagesData(data);
