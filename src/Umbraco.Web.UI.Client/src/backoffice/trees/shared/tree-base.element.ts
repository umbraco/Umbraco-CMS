import { LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeDataContext } from '../tree-data.context';
import type { ManifestTree } from '@umbraco-cms/models';

@customElement('umb-tree-base')
export class UmbTreeBase extends LitElement {
	@property({ type: Object, attribute: false })
	tree?: ManifestTree;

	protected _treeDataContext?: UmbTreeDataContext;
	protected _entityStore?: UmbEntityStore;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-base': UmbTreeBase;
	}
}
