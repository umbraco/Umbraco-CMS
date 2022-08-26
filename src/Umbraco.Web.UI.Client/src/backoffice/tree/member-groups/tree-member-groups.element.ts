import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

import '../shared/tree-navigator.element';

import { UmbContextProviderMixin } from '../../../core/context';
import { UmbExtensionManifestTree } from '../../../core/extension';
import { UmbTreeMemberGroupsContext } from './tree-member-groups.context';

@customElement('umb-tree-member-groups')
export class UmbTreeMemberGroups extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _treeContext?: UmbTreeMemberGroupsContext;

	@property({ attribute: false })
	public tree?: UmbExtensionManifestTree;

	connectedCallback() {
		super.connectedCallback();
		if (!this.tree) return;

		this._treeContext = new UmbTreeMemberGroupsContext(this.tree);
		this.provideContext('umbTreeContext', this._treeContext);
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeMemberGroups;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-member-groups': UmbTreeMemberGroups;
	}
}
