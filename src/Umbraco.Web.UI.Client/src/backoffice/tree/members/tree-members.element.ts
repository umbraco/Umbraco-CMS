import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';

import '../shared/tree-navigator.element';

import { UmbTreeService } from '../tree.service';
import { UmbContextProviderMixin } from '../../../core/context';
@customElement('umb-tree-members')
export class UmbTreeMembers extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeService?: UmbTreeService;

	connectedCallback() {
		super.connectedCallback();
		this._treeService = new UmbTreeService();
		this.provideContext('umbTreeService', this._treeService);
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-members': UmbTreeMembers;
	}
}
