import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbTreeContext } from '../tree.context';

import './tree-item.element';
import { Subscription } from 'rxjs';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _loading = true;

	private _treeContext?: UmbTreeContext;
	private _treeRootSubscription?: Subscription;

	@state()
	private _items: any[] = [];

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext) => {
			this._treeContext = treeContext;
			this._observeTreeRoot();
		});
	}

	private _observeTreeRoot() {
		this._loading = true;

		this._treeRootSubscription = this._treeContext?.fetchRoot?.().subscribe((items) => {
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
			${this._items?.map(
				(item) => html`
					<umb-tree-item
						.itemKey=${item.key}
						.itemType=${item.type}
						.label=${item.name}
						?hasChildren=${item.hasChildren}
						.loading=${this._loading}></umb-tree-item>
				`
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
