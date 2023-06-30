import type { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

export interface UmbPropertyEditorExtensionElement extends HTMLElement {
	value: unknown;
	config?: UmbDataTypeConfigCollection;
}
