import { html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-member-types-menu-item')
export class UmbMemberTypesMenuItemElement extends UmbLitElement {
	@state()
	private _renderTree = false;

	private _onShowChildren() {
		this._renderTree = true;
	}

	private _onHideChildren() {
		this._renderTree = false;
	}

	// TODO: check if root has children before settings the has-children attribute
	// TODO: how do we want to cache the tree? (do we want to rerender every time the user opens the tree)?
	// TODO: can we make this reusable?
	render() {
		return html`<umb-tree-item
			label="Member Types"
			icon="umb:folder"
			@show-children=${this._onShowChildren}
			@hide-children=${this._onHideChildren}
			has-children>
			${this._renderTree ? html`<umb-tree alias="Umb.Tree.MemberTypes"></umb-tree>` : nothing}
		</umb-tree-item> `;
	}
}

export default UmbMemberTypesMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-types-menu-item': UmbMemberTypesMenuItemElement;
	}
}
