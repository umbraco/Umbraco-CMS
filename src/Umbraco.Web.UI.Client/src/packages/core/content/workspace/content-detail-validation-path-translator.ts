import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbDataPathPropertyValueQuery,
	UmbDataPathVariantQuery,
	UmbValidationPropertyPathTranslationController,
	umbQueryMapperForJsonPaths,
	umbScopeMapperForJsonPaths,
	type UmbValidationPathTranslator,
} from '@umbraco-cms/backoffice/validation';
import type { UmbContentDetailModel } from '../types';

export class UmbContentDetailValidationPathTranslator
	extends UmbControllerBase
	implements UmbValidationPathTranslator<UmbContentDetailModel>
{
	async translate(paths: Array<string>, data: UmbContentDetailModel): Promise<Array<string>> {
		// Translate the Values array:

		// First scope to the values array:
		paths = await umbScopeMapperForJsonPaths(paths, '$.values', async (paths) => {
			// then translate the values of these properties:
			const ctrl = new UmbValidationPropertyPathTranslationController(this);
			return await ctrl.translateProperties(paths, data.values, UmbDataPathPropertyValueQuery);
		});

		// translate the Variants array:

		// First scope to the variants array:
		paths = await umbScopeMapperForJsonPaths(paths, '$.variants', async (paths) => {
			// Then map each entry:
			return await umbQueryMapperForJsonPaths(paths, data.variants, (entry) => UmbDataPathVariantQuery(entry));
		});

		return paths;
	}
}
