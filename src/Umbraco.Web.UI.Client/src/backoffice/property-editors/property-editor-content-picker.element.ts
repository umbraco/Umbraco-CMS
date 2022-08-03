import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query } from 'lit/decorators.js';

import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbModalService } from '../../core/services/modal.service';

import '../../core/services/modal-content-picker.element';

@customElement('umb-property-editor-content-picker')
class UmbPropertyEditorContentPicker extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: 16px;
				margin-right: 16px;
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}
		`,
	];

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _open() {
		const modalHandler = this._modalService?.openSidebar('umb-modal-content-picker', { size: 'small' });
		modalHandler?.onClose.then(() => {
			console.log('Closed the modal for:', this);
		});
	}

	private _tempOpenDialog() {
		//TODO: remove this
		this._modalService?.openDialog('umb-modal-content-picker');
	}

	render() {
		return html`
			<uui-button look="primary" @click=${this._open} label="open">Open sidebar</uui-button>
			<uui-button look="primary" @click=${this._tempOpenDialog} label="open">Open dialog</uui-button>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-content-picker': UmbPropertyEditorContentPicker;
	}
}
