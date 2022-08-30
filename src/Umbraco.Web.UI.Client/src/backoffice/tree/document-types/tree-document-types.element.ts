import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeDocumentTypesContext } from './tree-document-types.context';
import type { ManifestTree } from '../../../core/models';

import '../shared/tree-navigator.element';

@customElement('umb-document-type-tree')
export class UmbDocumentTypeTree extends UmbContextConsumerMixin(UmbContextProviderMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@property({ type: String })
	public alias = '';

	@property({ attribute: false })
	public tree?: ManifestTree;

	private _entityStore?: UmbEntityStore;
	private _treeContext?: UmbTreeDocumentTypesContext;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this.tree || !this._entityStore) return;

			this._treeContext = new UmbTreeDocumentTypesContext(this.tree, this._entityStore);
			this.provideContext('umbTreeContext', this._treeContext);
		});
	}

	render() {
		return html`<umb-tree-navigator .label=${this.alias}></umb-tree-navigator>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-tree': UmbDocumentTypeTree;
	}
}
