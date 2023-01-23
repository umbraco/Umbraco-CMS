import { html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/context-api';

@customElement('umb-media-types-sidebar-menu-item')
export class UmbMediaTypesSidebarMenuItemElement extends UmbLitElement {
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
			label="Media Types"
			icon="umb:folder"
			@show-children=${this._onShowChildren}
			@hide-children=${this._onHideChildren}
			has-children>
			${this._renderTree ? html`<umb-tree alias="Umb.Tree.MediaTypes"></umb-tree>` : nothing}
		</umb-tree-item> `;
	}
}

export default UmbMediaTypesSidebarMenuItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-types-sidebar-menu-item': UmbMediaTypesSidebarMenuItemElement;
	}
}
