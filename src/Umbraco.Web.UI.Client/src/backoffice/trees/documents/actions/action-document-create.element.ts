import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbActionElement from '../../actions/action.element';

@customElement('umb-tree-action-document-create')
export default class UmbTreeActionDocumentCreateElement extends UmbActionElement {
	static styles = [UUITextStyles, css``];

	// TODO: how do we handle the href?
	private _constructUrl() {
		return `/section/content/${this._activeTreeItem?.type}/${this._activeTreeItem?.key}/view/content?create=true`;
	}

	// TODO: change to href. This is a temporary solution to get the link to work. For some reason query params gets removed when using href.
	private _handleLabelClick() {
		if (!this._actionService) return;
		const href = this._constructUrl();
		history.pushState(null, '', href);
		this._actionService.close();
	}

	render() {
		return html`<uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-document-create': UmbTreeActionDocumentCreateElement;
	}
}
