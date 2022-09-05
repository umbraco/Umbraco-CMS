import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestEntityAction } from '../../../core/models';
import UmbActionElement from './action.element';

@customElement('umb-tree-action-create')
export default class UmbTreeActionCreateElement extends UmbActionElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	private _handleLabelClick() {
		console.log(this.treeAction, 'label clicked');
		this._actionPageService?.openPage('umb-tree-action-create-page');
	}

	render() {
		return html`<uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-create': UmbTreeActionCreateElement;
	}
}
