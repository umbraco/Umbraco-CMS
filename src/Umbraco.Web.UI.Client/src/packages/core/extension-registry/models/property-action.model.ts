import type { ConditionTypes } from '../conditions/types.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyAction extends ManifestElement, ManifestWithDynamicConditions<ConditionTypes> {
	type: 'propertyAction';
	meta: MetaPropertyAction;
}

export interface MetaPropertyAction {
	propertyEditors: string[];
}
