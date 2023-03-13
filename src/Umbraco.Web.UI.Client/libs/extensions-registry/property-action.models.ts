import type { ManifestElement } from './models';

export interface ManifestPropertyAction extends ManifestElement {
	type: 'propertyAction';
	conditions: ConditionsPropertyAction;
}

export interface ConditionsPropertyAction {
	propertyEditors: string[];
}
