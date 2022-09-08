import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbActionElement from '../../actions/action.element';

@customElement('umb-tree-action-data-type-create')
export default class UmbTreeActionDataTypeCreateElement extends UmbActionElement {
	static styles = [UUITextStyles, css``];

	// TODO: how do we handle the href?
	private _constructUrl() {
		return `/section/settings/${this._activeTreeItem?.type}/${this._activeTreeItem?.key}/view/edit?create=true`;
	}

	// TODO: change to href. This is a temporary solution to get the link to work. For some reason query params gets removed when using href.
	private _handleLabelClick() {
		if (!this._treeContextMenuService) return;
		const href = this._constructUrl();
		history.pushState(null, '', href);
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
		'umb-tree-action-data-type-create': UmbTreeActionDataTypeCreateElement;
	}
}
