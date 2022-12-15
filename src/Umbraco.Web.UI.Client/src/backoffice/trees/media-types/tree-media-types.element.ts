import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbMediaTypeStore } from 'src/core/stores/media-type/media-type.store';

import '../shared/tree-navigator.element';

@customElement('umb-tree-media-types')
export class UmbTreeMediaTypes extends UmbContextConsumerMixin(UmbContextProviderMixin(UmbTreeBase)) {
	constructor() {
		super();

		// TODO: how do we best expose the tree api to the tree navigator element?
		this.consumeContext('umbMediaTypeStore', (store: UmbMediaTypeStore) => {
			this.provideContext('umbTreeStore', store);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMediaTypes;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-media-types': UmbTreeMediaTypes;
	}
}
