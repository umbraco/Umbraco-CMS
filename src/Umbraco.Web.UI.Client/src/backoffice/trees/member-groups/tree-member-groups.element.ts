import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

import '../shared/tree-navigator.element';

import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbTreeMemberGroupsContext } from './tree-member-groups.context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import type { ManifestTree } from '../../../core/models';

@customElement('umb-tree-member-groups')
export class UmbTreeMemberGroups extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	tree?: ManifestTree;

	private _treeContext?: UmbTreeMemberGroupsContext;
	private _entityStore?: UmbEntityStore;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this._entityStore || !this.tree) return;

			this._treeContext = new UmbTreeMemberGroupsContext(this.tree, this._entityStore);
			this.provideContext('umbTreeContext', this._treeContext);
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
