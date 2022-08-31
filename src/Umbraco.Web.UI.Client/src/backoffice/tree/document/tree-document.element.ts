import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import '../shared/tree-navigator.element';
import { UmbTreeDocumentContext } from './tree-document.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import type { ManifestTree } from '../../../core/models';

@customElement('umb-tree-document')
export class UmbTreeDocumentElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	private _treeContext?: UmbTreeDocumentContext;

	@property({ attribute: false })
	public tree?: ManifestTree;

	private _entityStore?: UmbEntityStore;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this.tree || !this._entityStore) return;

			this._treeContext = new UmbTreeDocumentContext(this.tree, this._entityStore);
			this.provideContext('umbTreeContext', this._treeContext);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-document': UmbTreeDocumentElement;
	}
}
