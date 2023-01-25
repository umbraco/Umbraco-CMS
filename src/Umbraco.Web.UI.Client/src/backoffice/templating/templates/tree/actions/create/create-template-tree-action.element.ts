import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbTreeItemActionElement from '../../../../../shared/components/tree/action/tree-item-action.element';

@customElement('umb-create-template-tree-action')
export default class UmbCreateTemplateTreeAction extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];

	// TODO: how do we handle the href?
	private _constructUrl() {
		return `section/settings/${this._activeTreeItem?.type}/create/${this._activeTreeItem?.key || 'root'}`;
	}

	private _handleLabelClick() {
		if (!this._treeContextMenuService) return;
		this._treeContextMenuService.close();
	}

	render() {
		return html`<uui-menu-item
			label=${this.treeAction?.meta.label ?? ''}
			@click-label="${this._handleLabelClick}"
			href="${this._constructUrl()}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-template-tree-action': UmbCreateTemplateTreeAction;
	}
}
