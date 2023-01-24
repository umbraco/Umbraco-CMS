import type { ManifestElement } from './models';

export interface ManifestPropertyAction extends ManifestElement {
	type: 'propertyAction';
	meta: MetaPropertyAction;
}

export interface MetaPropertyAction {
	propertyEditors: string[];
}
