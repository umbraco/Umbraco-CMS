import type { ManifestElement, ManifestWithConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyAction extends ManifestElement, ManifestWithConditions<ConditionsPropertyAction> {
	type: 'propertyAction';
}

export interface ConditionsPropertyAction {
	propertyEditors: string[];
}
