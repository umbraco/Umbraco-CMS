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
	private _entityKey = '';

	@state()
	private _entityType = '';

	@state()
	private _label = '';

	@state()
	private _hasChildren = false;

	@state()
	private _loading = true;

	private _rootSubscription?: Subscription;

	private _treeContext?: UmbTreeContext;

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext) => {
			this._treeContext = treeContext;

			this._loading = true;

			this._rootSubscription = this._treeContext?.fetchRoot().subscribe((items) => {
				if (items?.length === 0) return;

				this._loading = false;
				this._entityKey = items[0].key;
				this._entityType = items[0].type;
				this._label = items[0].name;
				this._hasChildren = items[0].hasChildren;
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._rootSubscription?.unsubscribe();
	}

	render() {
		return html`<umb-tree-item
			.itemKey=${this._entityKey}
			.itemType=${this._entityType}
			.label=${this._label}
			?hasChildren=${this._hasChildren}
			.loading=${this._loading}></umb-tree-item> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
