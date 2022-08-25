import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

import '../shared/tree-navigator.element';

import { UmbContextProviderMixin } from '../../../core/context';
import { UmbExtensionManifestTree } from '../../../core/extension';
import { UmbTreeMemberContext } from './tree-members.context';
@customElement('umb-tree-members')
export class UmbTreeMembers extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeContext?: UmbTreeMemberContext;

	@property({ attribute: false })
	public tree?: UmbExtensionManifestTree;

	connectedCallback() {
		super.connectedCallback();
		this._treeContext = new UmbTreeMemberContext();
		this.provideContext('umbTreeService', this._treeContext);
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
