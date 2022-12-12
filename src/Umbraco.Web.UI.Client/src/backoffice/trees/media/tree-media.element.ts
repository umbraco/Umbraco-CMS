import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbMediaStore } from 'src/core/stores/media/media.store';

import '../shared/tree-navigator.element';

@customElement('umb-tree-media')
export class UmbTreeMediaElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbMediaStore', (store: UmbMediaStore) => {
			this.provideContext('umbTreeStore', store);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-media': UmbTreeMediaElement;
	}
}
