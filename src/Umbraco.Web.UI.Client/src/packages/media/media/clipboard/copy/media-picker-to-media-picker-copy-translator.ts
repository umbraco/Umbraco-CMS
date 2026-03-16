import type { UmbMediaPickerValueModel } from '../../property-editors/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbMediaPickerToMediaPickerClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbMediaPickerValueModel, UmbMediaPickerValueModel>
{
	async translate(propertyValue: UmbMediaPickerValueModel): Promise<UmbMediaPickerValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		return structuredClone(propertyValue);
	}
}

export { UmbMediaPickerToMediaPickerClipboardCopyPropertyValueTranslator as api };

