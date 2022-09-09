import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbTreeMembersDataContext } from './tree-members.context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeBase } from '../shared/tree-base.element';

import '../shared/tree-navigator.element';

@customElement('umb-tree-members')
export class UmbTreeMembers extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore || !this.tree) return;

			this._treeDataContext = new UmbTreeMembersDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-members': UmbTreeMembers;
	}
}
