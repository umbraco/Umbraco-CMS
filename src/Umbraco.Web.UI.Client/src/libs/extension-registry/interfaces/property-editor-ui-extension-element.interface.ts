import { UmbDataTypePropertyCollection } from 'src/libs/data-type';

export interface UmbPropertyEditorExtensionElement extends HTMLElement {
	value: unknown;
	config?: UmbDataTypePropertyCollection;
}
