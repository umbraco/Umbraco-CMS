import type { ManifestElement, ManifestWithConditions } from '.';

export interface ManifestPropertyAction extends ManifestElement, ManifestWithConditions<ConditionsPropertyAction> {
	type: 'propertyAction';
}

export interface ConditionsPropertyAction {
	propertyEditors: string[];
}
