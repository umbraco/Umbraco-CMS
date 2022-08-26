import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { ITreeContext } from '../tree.context';

import './tree-item.element';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeContext?: ITreeContext;

	@state()
	private _id = -1;

	@state()
	private _label = '';

	@state()
	private _hasChildren = false;

	@state()
	private _loading = true;

	@state()
	private _href? = '';

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbTreeContext', async (treeContext) => {
			this._treeContext = treeContext;

			const item = await this._treeContext?.getRoot?.();
			if (!item) return;

			this._id = item.id;
			this._label = item.name;
			this._hasChildren = item.hasChildren;
			this._loading = false;
			this._href = this._treeContext?.tree?.meta?.pathname;
		});
	}

	render() {
		return html`<umb-tree-item
			.id=${this._id}
			.label=${this._label}
			?hasChildren=${this._hasChildren}
			.loading=${this._loading}
			href=""></umb-tree-item> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
