import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../types.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbPropertyValueCloner } from '@umbraco-cms/backoffice/property';

export abstract class UmbBlockPropertyValueCloner<ValueType extends UmbBlockValueType>
	implements UmbPropertyValueCloner<ValueType>
{
	#propertyEditorAlias: string;
	#contentData?: ValueType['contentData'];
	#settingsData?: ValueType['settingsData'];
	#expose?: ValueType['expose'];

	constructor(propertyEditorAlias: string) {
		this.#propertyEditorAlias = propertyEditorAlias;
	}

	async cloneValue(value: ValueType) {
		if (value) {
			this.#contentData = value.contentData;
			this.#settingsData = value.settingsData;
			this.#expose = value.expose;

			const result = {
				...value,
				layout: {
					[this.#propertyEditorAlias]: await this._cloneLayout(value.layout[this.#propertyEditorAlias]),
				},
				contentData: this.#contentData,
				settingsData: this.#settingsData,
				expose: this.#expose,
			};

			return result;
		}
		return value;
	}

	protected abstract _cloneLayout(
		layouts: Array<UmbBlockLayoutBaseModel> | undefined,
	): Promise<Array<UmbBlockLayoutBaseModel> | undefined> | undefined;

	protected async _cloneBlock(layoutEntry: UmbBlockLayoutBaseModel): Promise<UmbBlockLayoutBaseModel> {
		const clonedLayoutEntry = { ...layoutEntry };

		const contentKey = layoutEntry.contentKey;
		const settingsKey = layoutEntry.settingsKey;

		// Generate new contentKey and settingsKey:
		const newContentKey = UmbId.new();
		clonedLayoutEntry.contentKey = newContentKey;

		// Replace contentKeys in contentData
		if (contentKey) {
			this.#contentData = this.#contentData?.map((contentData) => {
				if (contentData.key === contentKey) {
					return { ...contentData, key: newContentKey };
				}
				return contentData;
			});
		}

		// Replace contentKey in expose:
		this.#expose = this.#expose?.map((expose) => {
			if (expose.contentKey === contentKey) {
				return { ...expose, contentKey: newContentKey };
			}
			return expose;
		});

		if (settingsKey) {
			const newSettingsKey = UmbId.new();
			clonedLayoutEntry.settingsKey = newSettingsKey;
			// Replace settingsKeys in settingsData
			this.#settingsData = this.#settingsData?.map((settingsData) => {
				if (settingsData.key === settingsKey) {
					return { ...settingsData, key: newSettingsKey };
				}
				return settingsData;
			});
		}

		return clonedLayoutEntry;
	}

	destroy(): void {}
}
