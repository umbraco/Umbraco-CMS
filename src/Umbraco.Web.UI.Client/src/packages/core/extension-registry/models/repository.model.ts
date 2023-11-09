import type { ConditionTypes } from '../conditions/types.js';
import type { ExtensionApi, ManifestApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
// TODO: Consider adding a ClassType for this manifest. (Currently we cannot know the scope of a repository, therefor we are going with ExtensionApi for now.)
export interface ManifestRepository<ApiType extends ExtensionApi = ExtensionApi> extends ManifestApi<ApiType>, ManifestWithDynamicConditions<ConditionTypes> {
	type: 'repository';
}
