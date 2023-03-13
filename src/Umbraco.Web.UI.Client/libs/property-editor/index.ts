import { DataTypePropertyModel } from '@umbraco-cms/backend-api';

export interface UmbPropertyEditorElement extends HTMLElement {
	value: unknown;
	config: DataTypePropertyModel[];
}
