import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

export interface UmbPropertyEditorExtensionElement extends HTMLElement {
	value: unknown;
	config?: UmbDataTypePropertyCollection;
}
