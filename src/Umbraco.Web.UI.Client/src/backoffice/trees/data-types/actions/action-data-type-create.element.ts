import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestEntityAction } from '../../../../core/models';

@customElement('umb-tree-action-data-type-create')
export default class UmbTreeActionDataTypeCreateElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	private _handleLabelClick() {
		console.log('create new treee item');
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
