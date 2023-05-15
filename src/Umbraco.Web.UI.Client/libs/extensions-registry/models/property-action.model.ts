import type { ManifestElement, ManifestWithConditions } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestPropertyAction extends ManifestElement, ManifestWithConditions<ConditionsPropertyAction> {
	type: 'propertyAction';
}

export interface ConditionsPropertyAction {
	propertyEditors: string[];
}
