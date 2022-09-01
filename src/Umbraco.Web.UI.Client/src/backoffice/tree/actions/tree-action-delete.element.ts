import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { ManifestEntityAction } from '../../../core/models';
import { UmbActionService } from '../actions.service';

@customElement('umb-tree-action-delete')
export default class UmbTreeActionDeleteElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	private _actionService?: UmbActionService;

	constructor() {
		super();

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});
	}

	private _handleLabelClick() {
		console.log(this.treeAction, 'label clicked');
		this._actionService?.openPage('umb-tree-action-delete-page');
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
