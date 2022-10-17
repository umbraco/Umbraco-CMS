import { css, html, LitElement } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Entity } from '../../../core/mocks/data/entities';
import { UmbTreeDataContextBase } from '../tree-data.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import './tree-item.element';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@state()
	private _loading = true;

	@state()
	private _items: Entity[] = [];

	private _treeDataContext?: UmbTreeDataContextBase;

	constructor() {
		super();

		this.consumeContext('umbTreeDataContext', (treeDataContext) => {
			this._treeDataContext = treeDataContext;
			this._observeTreeRoot();
		});
	}

	private _observeTreeRoot() {
		if (!this._treeDataContext?.rootChanges?.()) return;

		this._loading = true;

		this.observe<Entity[]>(this._treeDataContext.rootChanges(), (rootItems) => {
			if (rootItems?.length === 0) return;
			this._items = rootItems;
			this._loading = false;
		});
	}

	render() {
		return html`
			${repeat(
				this._items,
				(item) => item.key,
				(item) => html`<umb-tree-item .treeItem=${item} .loading=${this._loading}></umb-tree-item>`
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
