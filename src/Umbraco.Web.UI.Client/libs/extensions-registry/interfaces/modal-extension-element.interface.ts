import type { UmbModalHandler } from '@umbraco-cms/backoffice/modal';

export interface UmbModalExtensionElement<UmbModalData extends object = object, UmbModalResult = unknown>
	extends HTMLElement {
	modalHandler?: UmbModalHandler<UmbModalData, UmbModalResult>;

	data?: UmbModalData;
}
