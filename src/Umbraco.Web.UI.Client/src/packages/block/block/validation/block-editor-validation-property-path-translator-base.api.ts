import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import {
	UmbDataPathPropertyValueQuery,
	UmbValidationPropertyPathTranslationController,
	umbQueryMapperForJsonPaths,
	umbScopeMapperForJsonPaths,
	type UmbPropertyValidationPathTranslator,
} from '@umbraco-cms/backoffice/validation';
import { UmbDataPathBlockElementDataQuery, type UmbBlockDataModel } from '@umbraco-cms/backoffice/block';

export abstract class UmbBlockEditorValidationPropertyPathTranslatorBase<PropertyValueType>
	extends UmbControllerBase
	implements UmbPropertyValidationPathTranslator<PropertyValueType>
{
	abstract translate(
		paths: Array<string>,
		data: UmbPropertyValueDataPotentiallyWithEditorAlias<PropertyValueType>,
	): Promise<Array<string>>;

	protected async _translateBlockData(paths: Array<string>, values: Array<UmbBlockDataModel>, dataPath: string) {
		return await umbScopeMapperForJsonPaths(paths, dataPath, async (paths) => {
			if (values.length === 0) {
				return paths;
			}
			const ctrl = new UmbValidationPropertyPathTranslationController(this);

			paths = await umbQueryMapperForJsonPaths(
				paths,
				values,
				(block) => {
					return UmbDataPathBlockElementDataQuery(block);
				},
				async (paths: string[], block: UmbBlockDataModel) => {
					if (block.values.length === 0) {
						return paths;
					}
					return await umbScopeMapperForJsonPaths(paths, '$.values', async (paths) => {
						// then translate the values of these properties:
						return await ctrl.translateProperties(paths, block.values, UmbDataPathPropertyValueQuery);
					});
				},
			);
			ctrl.destroy();
			return paths;
		});
	}
}
