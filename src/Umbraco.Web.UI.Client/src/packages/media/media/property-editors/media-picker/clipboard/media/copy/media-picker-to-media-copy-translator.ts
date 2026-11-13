import type { UmbMediaPickerValueModel } from '../../../../types.js';
import type { UmbMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbMediaPickerToMediaClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbMediaPickerValueModel, UmbMediaClipboardEntryValueModel>
{
	async translate(propertyValue: UmbMediaPickerValueModel): Promise<UmbMediaClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		return propertyValue.map((entry) => ({ unique: entry.mediaKey }));
	}
}

export { UmbMediaPickerToMediaClipboardCopyPropertyValueTranslator as api };
