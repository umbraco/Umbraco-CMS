import type { ManifestElement, ManifestWithConditions } from './models';

export interface ManifestPropertyAction extends ManifestElement, ManifestWithConditions<ConditionsPropertyAction> {
	type: 'propertyAction';
}

export interface ConditionsPropertyAction {
	propertyEditors: string[];
}
