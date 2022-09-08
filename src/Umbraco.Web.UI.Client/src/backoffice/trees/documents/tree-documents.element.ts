import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeDocumentDataContext } from './tree-documents-data.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-document')
export class UmbTreeDocumentElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore || !this.tree) return;

			this._treeDataContext = new UmbTreeDocumentDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
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
