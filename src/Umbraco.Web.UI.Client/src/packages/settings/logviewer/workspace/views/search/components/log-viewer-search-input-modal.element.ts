import { html, css } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement } from '@umbraco-ui/uui';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { SavedLogSearchPresenationBaseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-log-viewer-save-search-modal')
export default class UmbLogViewerSaveSearchModalElement extends UmbModalBaseElement<
	{ query: string },
	SavedLogSearchPresenationBaseModel
> {
	@query('uui-input')
	private _input!: UUIInputElement;

	private _handleClose() {
		this.modalHandler?.reject();
	}

	private _handleSubmit() {
		this.modalHandler?.submit({ name: this._input.value as string, query: this.data?.query });
	}

	@state()
	private _hasValue = false;

	#validate(event: Event) {
		const target = event.target as UUIInputElement;
		this._hasValue = (target.value as string).length > 0;
	}

	render() {
		return html`
			<uui-dialog-layout headline="Save Search">
				<span>Enter a friendly name for your search query</span>
				<uui-form-layout-item>
					<uui-label slot="label">Query:</uui-label>
					<span>${this.data?.query}</span>
				</uui-form-layout-item>
				<uui-form-layout-item>
					<uui-label slot="label" for="input">Name:</uui-label>
					<uui-input label="Search name" id="input" @input=${this.#validate}></uui-input>
				</uui-form-layout-item>

				<uui-button slot="actions" @click="${this._handleClose}" label="Close dialog">Close</uui-button>
				<uui-button
					.disabled=${!this._hasValue}
					slot="actions"
					look="primary"
					color="positive"
					label="Save search"
					@click="${this._handleSubmit}"
					>Save</uui-button
				>
			</uui-dialog-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-save-search-modal': UmbLogViewerSaveSearchModalElement;
	}
}
