import type { UmbMediaPickerValueModel } from '../../property-editors/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbMediaPickerFromMediaPickerClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbMediaPickerValueModel, UmbMediaPickerValueModel>
{
	async translate(value: UmbMediaPickerValueModel): Promise<UmbMediaPickerValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		return structuredClone(value);
	}

	async isCompatibleValue(
		_propertyValue: UmbMediaPickerValueModel,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		config: unknown,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		filter?: (propertyValue: UmbMediaPickerValueModel, config: unknown) => Promise<boolean>,
	): Promise<boolean> {
		// For now we accept all clipboard values for the media picker.
		// Compatibility checks can be added later if needed.
		return true;
	}
}

export { UmbMediaPickerFromMediaPickerClipboardPastePropertyValueTranslator as api };

