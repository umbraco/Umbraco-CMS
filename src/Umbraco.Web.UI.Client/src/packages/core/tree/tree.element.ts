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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree': UmbTreeElement;
	}
}
