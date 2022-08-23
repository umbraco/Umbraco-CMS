import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextProviderMixin } from '../../core/context';
import { UmbTreeService } from './tree.service';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeService: UmbTreeService;

	constructor() {
		super();
		this._treeService = new UmbTreeService();
		this.provideContext('umbTreeService', this._treeService);
	}

	renderItems() {
		return this._treeService.getRoot().map((item) => {
			return html`<umb-tree-item .id=${item.id} .hasChildren=${item.hasChildren} .label=${item.name}></umb-tree-item>`;
		});
	}
	render() {
		return this.renderItems();
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
