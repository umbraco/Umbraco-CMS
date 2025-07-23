import type { UmbBlockGridValueModel } from '../types.js';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { UmbBlockEditorValidationPropertyPathTranslatorBase } from '@umbraco-cms/backoffice/block';

export class UmbBlockGridValidationPropertyPathTranslator extends UmbBlockEditorValidationPropertyPathTranslatorBase<UmbBlockGridValueModel> {
	async translate(
		paths: Array<string>,
		data: UmbPropertyValueDataPotentiallyWithEditorAlias<UmbBlockGridValueModel>,
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

export { UmbBlockGridValidationPropertyPathTranslator as api };
