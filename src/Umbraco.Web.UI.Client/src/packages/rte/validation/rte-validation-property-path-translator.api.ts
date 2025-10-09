import type { UmbPropertyEditorRteValueType } from '../types.js';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { UmbBlockEditorValidationPropertyPathTranslatorBase } from '@umbraco-cms/backoffice/block';

export class UmbRteValidationPropertyPathTranslator extends UmbBlockEditorValidationPropertyPathTranslatorBase<UmbPropertyEditorRteValueType> {
	async translate(
		paths: Array<string>,
		data: UmbPropertyValueDataPotentiallyWithEditorAlias<UmbPropertyEditorRteValueType>,
	): Promise<Array<string>> {
		if (!data.value || !data.value.blocks) {
			return paths;
		}
		// ContentData:
		paths = await this._translateBlockData(paths, data.value.blocks.contentData, '$.value.blocks.contentData');

		// SettingsData:
		paths = await this._translateBlockData(paths, data.value.blocks.settingsData, '$.value.blocks.settingsData');

		return paths;
	}
}

export { UmbRteValidationPropertyPathTranslator as api };
