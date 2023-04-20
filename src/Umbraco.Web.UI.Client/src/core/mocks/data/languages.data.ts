import { UmbData } from './data';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

// Temp mocked database
class UmbLanguagesData extends UmbData<LanguageResponseModel> {
	constructor(data: LanguageResponseModel[]) {
		super(data);
	}

	// skip can be number or null
	getAll(skip = 0, take = this.data.length): Array<LanguageResponseModel> {
		return this.data.slice(skip, take);
	}

	getByKey(isoCode: string) {
		return this.data.find((item) => item.isoCode === isoCode);
	}

	getItems(isoCodes: Array<string>) {
		return this.data.filter((item) => isoCodes.indexOf(item.isoCode || '') !== -1);
	}

	insert(language: LanguageResponseModel) {
		const foundIndex = this.data.findIndex((item) => item.isoCode === language.isoCode);

		if (foundIndex !== -1) {
			throw new Error('Language with same iso code already exists');
		}

		this.data.push(language);
	}

	save(saveItems: Array<LanguageResponseModel>) {
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

	delete(isoCodes: Array<string>) {
		isoCodes.forEach((isoCode) => {
			const foundIndex = this.data.findIndex((item) => item.isoCode === isoCode);
			if (foundIndex !== -1) {
				this.data.splice(foundIndex, 1);
			}
		});

		return isoCodes;
	}

	updateData(updateItem: LanguageResponseModel) {
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

export const MockData: Array<LanguageResponseModel> = [
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
