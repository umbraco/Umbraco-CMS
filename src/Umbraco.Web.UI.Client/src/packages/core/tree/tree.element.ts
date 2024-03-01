import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestTree } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionInitializerElementBase } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree')
export class UmbTreeElement extends UmbExtensionInitializerElementBase<ManifestTree> {
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
