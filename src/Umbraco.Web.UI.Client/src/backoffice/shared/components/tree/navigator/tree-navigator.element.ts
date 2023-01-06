import { css, html } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import type { Entity } from '@umbraco-cms/models';
import { UmbTreeDataStore } from '@umbraco-cms/stores/store';

import '../tree-item.element';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	private _loading = true;

	@state()
	private _items: Entity[] = [];

	private _store?: UmbTreeDataStore<any>;

	constructor() {
		super();

		this.consumeContext('umbStore', (store) => {
			this._store = store;
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this._observeTreeRoot();
	}

	private _observeTreeRoot() {
		if (!this._store?.getTreeRoot) return;

		this._loading = true;

		this.observe(this._store.getTreeRoot(), (rootItems) => {
			if (rootItems?.length === 0) return;
			this._items = rootItems;
			this._loading = false;
		});
	}

	render() {
		// TODO: Fix Type Mismatch ` as Entity` in this template:
		return html`
			${repeat(
				this._items,
				(item) => item.key,
				(item) =>
					html`<umb-tree-item
						.key=${item.key}
						.label=${item.name}
						.icon=${item.icon}
						.entityType=${item.type}
						.hasChildren=${item.hasChildren}
						.loading=${this._loading}></umb-tree-item>`
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
