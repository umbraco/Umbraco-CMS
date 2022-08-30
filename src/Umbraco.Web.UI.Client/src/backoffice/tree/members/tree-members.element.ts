import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

import '../shared/tree-navigator.element';

import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbTreeMembersContext } from './tree-members.context';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import type { ManifestTree } from '../../../core/models';

@customElement('umb-tree-members')
export class UmbTreeMembers extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	private _treeContext?: UmbTreeMembersContext;

	@property({ attribute: false })
	public tree?: ManifestTree;

	private _entityStore?: UmbEntityStore;

	constructor() {
		super();

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			if (!this.tree || !this._entityStore) return;

			this._treeContext = new UmbTreeMembersContext(this.tree, this._entityStore);
			this.provideContext('umbTreeContext', this._treeContext);
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
