import type { ConditionTypes } from '../conditions/types.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTree extends ManifestElementAndApi, ManifestWithDynamicConditions<ConditionTypes> {
	type: 'tree';
	meta: MetaTree;
}

export interface MetaTree {
	repositoryAlias: string;
}
