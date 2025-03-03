import type { ManifestTree } from './extensions/types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree')
export class UmbTreeElement extends UmbExtensionElementAndApiSlotElementBase<ManifestTree> {
	getExtensionType() {
		return 'tree';
	}

	getDefaultElementName() {
		return 'umb-default-tree';
	}

	getSelection() {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: make base interface for a tree element
		return this._element?.getSelection?.() ?? [];
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree': UmbTreeElement;
	}
}
