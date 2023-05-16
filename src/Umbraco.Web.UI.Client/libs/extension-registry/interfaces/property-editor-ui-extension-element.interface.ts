import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbPropertyEditorExtensionElement extends HTMLElement {
	value: unknown;
	config: DataTypePropertyPresentationModel[];
}
