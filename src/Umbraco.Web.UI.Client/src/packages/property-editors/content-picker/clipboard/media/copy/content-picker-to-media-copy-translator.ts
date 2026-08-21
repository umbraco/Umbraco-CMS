import { UMB_MEDIA_ENTITY_TYPE, type UmbMediaClipboardEntryValueModel } from '@umbraco-cms/backoffice/media';
import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

type UmbContentPickerValueModel = Array<UmbReferenceByUniqueAndType>;

export class UmbContentPickerToMediaClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbContentPickerValueModel, UmbMediaClipboardEntryValueModel>
{
	async translate(propertyValue: UmbContentPickerValueModel): Promise<UmbMediaClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		// The Content Picker can also reference documents/members; only media references map to a media entry.
		return propertyValue
			.filter((reference) => reference.type === UMB_MEDIA_ENTITY_TYPE)
			.map((reference) => ({ unique: reference.unique }));
	}
}

export { UmbContentPickerToMediaClipboardCopyPropertyValueTranslator as api };
