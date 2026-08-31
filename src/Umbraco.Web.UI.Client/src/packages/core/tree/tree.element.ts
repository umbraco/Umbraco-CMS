import type { ManifestTree } from './extensions/types.js';
import type { UmbTreeContext } from './tree.context.interface.js';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree')
export class UmbTreeElement extends UmbExtensionElementAndApiSlotElementBase<ManifestTree> {
	get interactionMemories(): Array<UmbInteractionMemoryModel> {
		return (this._api as UmbTreeContext | undefined)?.interactionMemory?.getAllMemories() ?? [];
	}

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

	getExpansion() {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: make base interface for a tree element
		return this._element?.getExpansion?.() ?? [];
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree': UmbTreeElement;
	}
}
