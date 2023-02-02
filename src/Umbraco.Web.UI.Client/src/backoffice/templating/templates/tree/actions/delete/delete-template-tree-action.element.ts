import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbTreeItemActionElement from '../../../../../shared/components/tree/action/tree-item-action.element';
import { UmbTemplateDetailRepository } from '../../../workspace/data/template.detail.repository';

@customElement('umb-delete-template-tree-action')
export default class UmbDeleteTemplateTreeAction extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];
	#templateDetailRepo = new UmbTemplateDetailRepository(this);

	private _handleLabelClick() {
		if (!this._activeTreeItem?.key) return;
		if (!this._treeContextMenuService) return;

		this.#templateDetailRepo.delete(this._activeTreeItem.key);
		this._treeContextMenuService.close();
	}

	render() {
		return html`<uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-delete-template-tree-action': UmbDeleteTemplateTreeAction;
	}
}
