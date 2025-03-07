import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import type { UmbBlockListValueModel } from '../types.js';
import { UmbBlockEditorValidationPropertyPathTranslatorBase } from '@umbraco-cms/backoffice/block';

export class UmbBlockListValidationPropertyPathTranslator extends UmbBlockEditorValidationPropertyPathTranslatorBase<UmbBlockListValueModel> {
	async translate(
		paths: Array<string>,
		data: UmbPropertyValueDataPotentiallyWithEditorAlias<UmbBlockListValueModel>,
	): Promise<Array<string>> {
		if (!data.value) {
			return paths;
		}
		// ContentData:
		paths = await this._translateBlockData(paths, data.value.contentData, '$.value.contentData');

		// SettingsData:
		paths = await this._translateBlockData(paths, data.value.settingsData, '$.value.settingsData');

		return paths;
	}
}

export { UmbBlockListValidationPropertyPathTranslator as api };
