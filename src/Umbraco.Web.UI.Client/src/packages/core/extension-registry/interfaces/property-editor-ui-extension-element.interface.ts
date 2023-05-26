import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

export interface UmbPropertyEditorExtensionElement extends HTMLElement {
	value: unknown;
	config?: UmbDataTypePropertyCollection;
}
