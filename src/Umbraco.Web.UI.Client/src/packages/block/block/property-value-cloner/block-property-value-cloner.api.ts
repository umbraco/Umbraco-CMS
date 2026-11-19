import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../types.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbPropertyValueCloner } from '@umbraco-cms/backoffice/property';

export type UmbBlockPropertyValueClonerArgs = {
	contentIdUpdatedCallback?: (oldContentKey: string, newContentKey: string) => void;
	/**
	 * Called when a cloned layout entry gets a new key. Called for external content as well, and before `contentIdUpdatedCallback`.
	 * @param oldLayoutKey - The key of the original layout entry.
	 * @param newLayoutKey - The key of the cloned layout entry.
	 * @param contentKey - The content key of the original layout entry, from before it is renewed.
	 */
	layoutKeyUpdatedCallback?: (oldLayoutKey: string | undefined, newLayoutKey: string, contentKey: string) => void;
};

export abstract class UmbBlockPropertyValueCloner<
	ValueType extends UmbBlockValueType,
	LayoutEntryType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> implements UmbPropertyValueCloner<ValueType> {
	#contentIdUpdatedCallback?: UmbBlockPropertyValueClonerArgs['contentIdUpdatedCallback'];
	#layoutKeyUpdatedCallback?: UmbBlockPropertyValueClonerArgs['layoutKeyUpdatedCallback'];

	#propertyEditorAlias: string;
	#contentData?: ValueType['contentData'];
	#settingsData?: ValueType['settingsData'];
	#expose?: ValueType['expose'];

	constructor(propertyEditorAlias: string, args?: UmbBlockPropertyValueClonerArgs) {
		this.#propertyEditorAlias = propertyEditorAlias;
		this.#contentIdUpdatedCallback = args?.contentIdUpdatedCallback;
		this.#layoutKeyUpdatedCallback = args?.layoutKeyUpdatedCallback;
	}

	async cloneValue(value: ValueType) {
		if (value) {
			this.#contentData = value.contentData;
			this.#settingsData = value.settingsData;
			this.#expose = value.expose;

			const result = {
				...value,
				layout: {
					[this.#propertyEditorAlias]: await this._cloneLayout(
						value.layout[this.#propertyEditorAlias] as unknown as Array<LayoutEntryType>,
					),
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
		layouts: Array<LayoutEntryType> | undefined,
	): Promise<Array<LayoutEntryType> | undefined> | undefined;

	protected async _cloneBlock(layoutEntry: LayoutEntryType): Promise<LayoutEntryType> {
		const clonedLayoutEntry = { ...layoutEntry };

		const contentKey = layoutEntry.contentKey;
		const settingsKey = layoutEntry.settingsKey;

		clonedLayoutEntry.key = UmbId.new();
		this.#layoutKeyUpdatedCallback?.(layoutEntry.key, clonedLayoutEntry.key, contentKey);

		// External content is referenced by its contentKey, so the key must be kept:
		if (!layoutEntry.isExternalContent) {
			const newContentKey = UmbId.new();
			clonedLayoutEntry.contentKey = newContentKey;

			// Replace contentKeys in contentData
			this.#contentData = this.#contentData?.map((contentData) => {
				if (contentData.key === contentKey) {
					return { ...contentData, key: newContentKey };
				}
				return contentData;
			});

			// Replace contentKey in expose:
			this.#expose = this.#expose?.map((expose) => {
				if (expose.contentKey === contentKey) {
					return { ...expose, contentKey: newContentKey };
				}
				return expose;
			});

			this.#contentIdUpdatedCallback?.(contentKey, newContentKey);
		}

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
