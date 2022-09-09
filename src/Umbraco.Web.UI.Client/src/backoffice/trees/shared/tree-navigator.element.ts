import { css, html, LitElement } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import { Entity } from '../../../mocks/data/entities';
import { UmbTreeDataContextBase } from '../tree-data.context';

import './tree-item.element';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _loading = true;

	@state()
	private _items: Entity[] = [];

	private _treeDataContext?: UmbTreeDataContextBase;
	private _treeRootSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbTreeDataContext', (treeDataContext) => {
			this._treeDataContext = treeDataContext;
			this._observeTreeRoot();
		});
	}

	private _observeTreeRoot() {
		this._loading = true;

		this._treeRootSubscription = this._treeDataContext?.rootChanges?.().subscribe((items) => {
			if (items?.length === 0) return;
			this._items = items;
			this._loading = false;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._treeRootSubscription?.unsubscribe();
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
