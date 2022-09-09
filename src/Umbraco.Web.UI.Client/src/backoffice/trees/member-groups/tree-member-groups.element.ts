import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbTreeBase } from '../shared/tree-base.element';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeMemberGroupsDataContext } from './tree-member-groups-data.context';

import '../shared/tree-navigator.element';

@customElement('umb-tree-member-groups')
export class UmbTreeMemberGroups extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbTreeBase)) {
	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore || !this.tree) return;

			this._treeDataContext = new UmbTreeMemberGroupsDataContext(this._entityStore);
			this.provideContext('umbTreeDataContext', this._treeDataContext);
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
