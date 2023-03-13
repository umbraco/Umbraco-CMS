import { DataTypePropertyModel } from '@umbraco-cms/backend-api';

export interface UmbPropertyEditorElement {
	value: unknown;
	config: DataTypePropertyModel[];
}
