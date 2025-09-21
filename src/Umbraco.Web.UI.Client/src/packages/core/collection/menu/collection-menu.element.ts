import type { ManifestCollectionMenu } from './extension/types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-collection-menu')
export class UmbCollectionMenuElement extends UmbExtensionElementAndApiSlotElementBase<ManifestCollectionMenu> {
	getExtensionType() {
		return 'collectionMenu';
	}

	getDefaultElementName() {
		return 'umb-default-collection-menu';
	}

	getSelection() {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: make base interface for a collection menu element
		return this._element?.getSelection?.() ?? [];
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-menu': UmbCollectionMenuElement;
	}
}
