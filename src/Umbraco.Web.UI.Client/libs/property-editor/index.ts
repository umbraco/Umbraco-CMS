import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbPropertyEditorElement extends HTMLElement {
	value: unknown;
	config: DataTypePropertyPresentationModel[];
}
