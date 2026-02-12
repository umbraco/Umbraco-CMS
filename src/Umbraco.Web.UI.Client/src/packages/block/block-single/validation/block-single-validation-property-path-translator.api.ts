import type { UmbBlockSingleValueModel } from '../types.js';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { UmbBlockEditorValidationPropertyPathTranslatorBase } from '@umbraco-cms/backoffice/block';

export class UmbBlockSingleValidationPropertyPathTranslator extends UmbBlockEditorValidationPropertyPathTranslatorBase<UmbBlockSingleValueModel> {
	async translate(
		paths: Array<string>,
		data: UmbPropertyValueDataPotentiallyWithEditorAlias<UmbBlockSingleValueModel>,
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

export { UmbBlockSingleValidationPropertyPathTranslator as api };
