import { html, css } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { SavedLogSearchPresenationBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UUIInputElement } from '@umbraco-ui/uui';

@customElement('umb-log-viewer-save-search-modal')
export default class UmbLogViewerSaveSearchModalElement extends UmbModalBaseElement<
	{ query: string },
	SavedLogSearchPresenationBaseModel
> {
	static styles = [
		UUITextStyles,
		css`
			uui-dialog-layout {
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24));
				border-radius: var(--uui-border-radius);
				padding: var(--uui-size-space-5);
				box-sizing: border-box;
			}

			uui-input {
				width: 100%;
			}
		`,
	];

	@query('uui-input')
	private _input!: UUIInputElement;

	private _handleClose() {
		this.modalHandler?.reject();
	}

	private _handleSubmit() {
		this.modalHandler?.submit({ name: this._input.value as string, query: this.data?.query });
	}

	firstUpdated() {
		console.log('this.data', this.data);
	}

	render() {
		return html`
			<uui-dialog-layout headline="Save Search">
				<span>Enter a friendly name for your search query</span>
				<h4>Query:</h4>
				<span>${this.data?.query}</span>
				<h4>Name:</h4>
				<uui-input></uui-input>
				<uui-button slot="actions" @click="${this._handleClose}">Close</uui-button>
				<uui-button slot="actions" look="primary" color="positive" @click="${this._handleSubmit}">Save</uui-button>
			</uui-dialog-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-save-search-modal': UmbLogViewerSaveSearchModalElement;
	}
}
