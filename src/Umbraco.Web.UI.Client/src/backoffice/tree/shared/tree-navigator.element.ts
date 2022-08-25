import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextProviderMixin } from '../../../core/context';
import { UmbTreeService } from '../tree.service';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeService: UmbTreeService;

	@state()
	id = '2';

	@state()
	label = '';

	@state()
	hasChildren = false;

	@state()
	loading = true;

	constructor() {
		super();
		this._treeService = new UmbTreeService();
		this.provideContext('umbTreeService', this._treeService);
		this._treeService.getTreeItem(this.id).then((item) => {
			this.label = item.name;
			this.hasChildren = item.hasChildren;
			this.loading = false;
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
