import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbTreeService } from '../tree.service';

import './tree-item.element';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeService?: UmbTreeService;

	@state()
	id = '2';

	@state()
	label = '';

	@state()
	hasChildren = false;

	@state()
	loading = true;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbTreeService', (treeService) => {
			this._treeService = treeService;

			this._treeService?.getTreeItem(this.id).then((item) => {
				this.label = item.name;
				this.hasChildren = item.hasChildren;
				this.loading = false;
			});
		});
	}

	render() {
		return html`<umb-tree-item
			.id=${this.id}
			.label=${this.label}
			?hasChildren=${this.hasChildren}
			.loading=${this.loading}></umb-tree-item> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
