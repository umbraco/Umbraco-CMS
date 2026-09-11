import type { UmbMediaPickerValueModel } from '../../../../types.js';
import type { UmbRichMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbMediaPickerToRichMediaClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbMediaPickerValueModel, UmbRichMediaClipboardEntryValueModel>
{
	async translate(propertyValue: UmbMediaPickerValueModel): Promise<UmbRichMediaClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		return propertyValue.map((entry) => ({
			unique: entry.mediaKey,
			focalPoint: entry.focalPoint ?? null,
			crops: structuredClone(entry.crops ?? []),
		}));
	}
}

export { UmbMediaPickerToRichMediaClipboardCopyPropertyValueTranslator as api };
