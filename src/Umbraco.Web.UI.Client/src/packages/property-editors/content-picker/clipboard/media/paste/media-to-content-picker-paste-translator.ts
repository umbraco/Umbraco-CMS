import type { UmbContentPickerSource } from '../../../types.js';
import { UMB_MEDIA_ENTITY_TYPE, type UmbMediaClipboardEntryValueModel } from '@umbraco-cms/backoffice/media';
import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

type UmbContentPickerValueModel = Array<UmbReferenceByUniqueAndType>;

export class UmbMediaToContentPickerClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbMediaClipboardEntryValueModel, UmbContentPickerValueModel>
{
	async translate(value: UmbMediaClipboardEntryValueModel): Promise<UmbContentPickerValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		return value.map((item) => ({ type: UMB_MEDIA_ENTITY_TYPE, unique: item.unique }));
	}

	async isCompatibleValue(
		_propertyValue: UmbContentPickerValueModel,
		config: UmbPropertyEditorConfigCollection | undefined,
	): Promise<boolean> {
		// Only accept media into a Content Picker that is configured to pick media.
		const startNode = config?.getValueByAlias<UmbContentPickerSource>('startNode');
		return startNode?.type === 'media';
	}
}

export { UmbMediaToContentPickerClipboardPastePropertyValueTranslator as api };
