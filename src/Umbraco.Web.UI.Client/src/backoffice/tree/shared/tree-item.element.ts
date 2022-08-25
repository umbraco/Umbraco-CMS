import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbTreeService } from '../tree.service';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ type: Boolean })
	hasChildren = false;

	@property({ type: String })
	id = '-1';

	@property({ type: String })
	label = '';

	@state()
	childItems: any[] = [];

	@state()
	loading = false;

	private _treeService?: UmbTreeService;

	constructor() {
		super();

		this.consumeContext('umbTreeService', (treeService: UmbTreeService) => {
			this._treeService = treeService;
		});
	}

	private _renderChildItems() {
		return this.childItems.map((item) => {
			return html`<umb-tree-item .label=${item.name} .hasChildren=${item.hasChildren} .id=${item.id}></umb-tree-item>`;
		});
	}

	private _onShowChildren(event: UUIMenuItemEvent) {
		event.stopPropagation();
		if (this.childItems.length > 0) return;

		this.loading = true;
		this._treeService?.getChildren(this.id).then((items) => {
			this.childItems = items;
			this.loading = false;
		});
	}

	render() {
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				.loading=${this.loading}
				.hasChildren=${this.hasChildren}
				label=${this.label}>
				${this._renderChildItems()}
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItem;
	}
}
