import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbTreeMediaDataContext } from './tree-media-data.context';

import '../shared/tree-navigator.element';

@customElement('umb-tree-media')
export class UmbTreeMediaElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore) return;

			this._treeDataContext = new UmbTreeMediaDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
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
