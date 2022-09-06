import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from '../../core/context';
import { UmbModalService } from '../../core/services/modal';

// TODO: remove these imports when they are part of UUI
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';

@customElement('umb-property-editor-icon-picker')
class UmbPropertyEditorIconPicker extends UmbContextConsumerMixin(LitElement) {
	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _openModal() {
		this._modalService?.open('umb-modal-layout-icon-picker', { type: 'sidebar', size: 'small' });
	}

	render() {
		return html`
			<uui-button label="open-icon-picker" look="secondary" @click=${this._openModal} style="margin-right: 9px;">
				Pick an icon
			</uui-button>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-icon-picker': UmbPropertyEditorIconPicker;
	}
}
