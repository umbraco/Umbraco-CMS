import type { ManifestElement, ManifestWithConditions } from 'src/libs/extension-api';

export interface ManifestPropertyAction extends ManifestElement, ManifestWithConditions<ConditionsPropertyAction> {
	type: 'propertyAction';
}

export interface ConditionsPropertyAction {
	propertyEditors: string[];
}
