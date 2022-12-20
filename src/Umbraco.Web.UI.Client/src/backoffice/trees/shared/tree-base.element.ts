import { LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestTree } from '@umbraco-cms/models';

@customElement('umb-tree-base')
export class UmbTreeBase extends LitElement {
	@property({ type: Object, attribute: false })
	tree?: ManifestTree;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-base': UmbTreeBase;
	}
}
