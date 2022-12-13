import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbMemberGroupStore } from 'src/core/stores/member-group/member-group.store';
import '../shared/tree-navigator.element';

@customElement('umb-tree-member-groups')
export class UmbTreeMemberGroups extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		// TODO: how do we best expose the tree api to the tree navigator element?
		this.consumeContext('umbMemberGroupStore', (store: UmbMemberGroupStore) => {
			this.provideContext('umbTreeStore', store);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMemberGroups;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-member-groups': UmbTreeMemberGroups;
	}
}
