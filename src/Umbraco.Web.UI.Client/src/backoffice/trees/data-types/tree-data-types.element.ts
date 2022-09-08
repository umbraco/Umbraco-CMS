import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeDataTypesDataContext } from './tree-data-types-data.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-data-types')
export class UmbTreeDataTypesElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore) return;

			this._treeDataContext = new UmbTreeDataTypesDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeDataTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-data-types': UmbTreeDataTypesElement;
	}
}
