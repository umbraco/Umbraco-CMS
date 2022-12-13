import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbMemberTypeStore } from 'src/core/stores/member-type/member-type.store';

import '../shared/tree-navigator.element';

@customElement('umb-tree-member-types')
export class UmbTreeMemberTypes extends UmbContextConsumerMixin(UmbContextProviderMixin(UmbTreeBase)) {
	constructor() {
		super();

		// TODO: how do we best expose the tree api to the tree navigator element?
		this.consumeContext('umbMemberTypeStore', (store: UmbMemberTypeStore) => {
			this.provideContext('umbTreeStore', store);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMemberTypes;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-member-types': UmbTreeMemberTypes;
	}
}
