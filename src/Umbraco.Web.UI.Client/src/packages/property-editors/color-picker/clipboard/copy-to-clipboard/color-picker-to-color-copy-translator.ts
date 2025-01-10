import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbColorPickerToColorClipboardCopyTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<any>
{
	async translate(propertyValue: any): Promise<UmbBlockClipboardEntryValueModel> {
		return propertyValue;
	}
}

export { UmbColorPickerToColorClipboardCopyTranslator as api };
