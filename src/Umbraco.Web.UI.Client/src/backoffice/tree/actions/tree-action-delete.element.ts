import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { ManifestEntityAction } from '../../../core/models';

@customElement('umb-tree-action-delete')
export default class UmbTreeActionDeleteElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	private _handleLabelClick() {
		console.log(this.treeAction, 'label clicked');
	}

	render() {
		return html` <uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-delete': UmbTreeActionDeleteElement;
	}
}
