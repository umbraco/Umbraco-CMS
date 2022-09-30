import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbTreeDocumentTypesDataContext } from './tree-document-types-data.context';

import '../shared/tree-navigator.element';

@customElement('umb-tree-document-types')
export class UmbTreeDocumentTypes extends UmbContextConsumerMixin(UmbContextProviderMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore) return;

			this._treeDataContext = new UmbTreeDocumentTypesDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeDocumentTypes;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-document-types': UmbTreeDocumentTypes;
	}
}
