import { UmbData } from './data';
import { LanguageModel } from '@umbraco-cms/backend-api';

// Temp mocked database
class UmbLanguagesData extends UmbData<LanguageModel> {
	constructor(data: LanguageModel[]) {
		super(data);
	}

	// skip can be number or null
	getAll(skip = 0, take = this.data.length): Array<LanguageModel> {
		return this.data.slice(skip, take);
	}

	getByKey(key: string) {
		return this.data.find((item) => item.isoCode === key);
	}

	insert(language: LanguageModel) {
		const foundIndex = this.data.findIndex((item) => item.isoCode === language.isoCode);

		if (foundIndex !== -1) {
			throw new Error('Language with same iso code already exists');
		}

		this.data.push(language);
	}

	save(saveItems: Array<LanguageModel>) {
		saveItems.forEach((saveItem) => {
			const foundIndex = this.data.findIndex((item) => item.isoCode === saveItem.isoCode);
			if (foundIndex !== -1) {
				// update
				this.data[foundIndex] = saveItem;
				this.updateData(saveItem);
			} else {
				// Set all other languages to not default
				if (saveItem.isDefault) {
					this.data.forEach((item) => {
						if (saveItem !== item) {
							item.isDefault = false;
						}
					});
				}
				this.data.push(saveItem);
			}
		});

		return this.data;
	}

	delete(keys: Array<string>) {
		keys.forEach((key) => {
			const foundIndex = this.data.findIndex((item) => item.isoCode === key);
			if (foundIndex !== -1) {
				this.data.splice(foundIndex, 1);
			}
		});

		return keys;
	}

	updateData(updateItem: LanguageModel) {
		const itemIndex = this.data.findIndex((item) => item.isoCode === updateItem.isoCode);
		const item = this.data[itemIndex];
		if (!item) return;

		const itemKeys = Object.keys(item);
		const newItem = {};

		// Set all other languages to not default
		if (updateItem.isDefault) {
			this.data.forEach((item) => {
				if (updateItem !== item) {
					item.isDefault = false;
				}
			});
		}

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

export const MockData: Array<LanguageModel> = [
	{
		name: 'English',
		isoCode: 'en',
		isDefault: true,
		isMandatory: true,
	},
	{
		name: 'Danish',
		isoCode: 'da',
		isDefault: false,
		isMandatory: false,
		fallbackIsoCode: 'en',
	},
];

export const umbLanguagesData = new UmbLanguagesData(MockData);
