import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbTreeItemActionElement from '../../shared/tree-item-action.element';

@customElement('umb-tree-action-create')
export class UmbTreeActionCreateElement extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];

	private _handleLabelClick() {
		this._actionPageService?.openPage('umb-tree-action-create-page');
	}

	render() {
		return html`<uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

export default UmbTreeActionCreateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-create': UmbTreeActionCreateElement;
	}
}
