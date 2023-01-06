import { html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-data-type-tree')
export class UmbDataTypeTreeElement extends UmbLitElement {
	@state()
	private _renderTree = false;

	private _onShowChildren() {
		this._renderTree = true;
	}

	private _onHideChildren() {
		this._renderTree = false;
	}

	render() {
		return html`<uui-menu-item
			label="Data Types"
			has-children
			@show-children="${this._onShowChildren}"
			@hide-children="${this._onHideChildren}">
			<uui-icon slot="icon" name="umb:folder"></uui-icon>
			${this._renderTree ? html`<umb-tree alias="Umb.Tree.DataTypes"></umb-tree>` : nothing}
		</uui-menu-item> `;
	}
}

export default UmbDataTypeTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-tree': UmbDataTypeTreeElement;
	}
}
