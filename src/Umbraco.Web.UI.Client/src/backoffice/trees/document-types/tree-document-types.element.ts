import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';

import '../shared/tree-navigator.element';
import { UmbDocumentTypeStore } from 'src/core/stores/document-type/document-type.store';

@customElement('umb-tree-document-types')
export class UmbTreeDocumentTypes extends UmbContextConsumerMixin(UmbContextProviderMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbDocumentTypeStore', (store: UmbDocumentTypeStore) => {
			this.provideContext('umbTreeStore', store);
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
