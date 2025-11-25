import type {
	UmbContextSaveSearchModalData,
	UmbContextSaveSearchModalValue,
} from './log-viewer-search-input-modal.modal-token.js';
import { html, css, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-log-viewer-save-search-modal')
export default class UmbLogViewerSaveSearchModalElement extends UmbModalBaseElement<
	UmbContextSaveSearchModalData,
	UmbContextSaveSearchModalValue
> {
	@query('uui-input')
	private _input!: UUIInputElement;

	private _handleClose() {
		this.modalContext?.reject();
	}

	private _handleSubmit() {
		if (!this.data?.query) return;

		this.value = { name: this._input.value as string, query: this.data.query };
		this.modalContext?.submit();
	}

	@state()
	private _hasValue = false;

	#validate(event: Event) {
		const target = event.target as UUIInputElement;
		this._hasValue = (target.value as string).length > 0;
	}

	override render() {
		return html`
			<uui-dialog-layout headline=${this.localize.term('logViewer_saveSearch')}>
				<umb-localize key="logViewer_saveSearchDescription"> Enter a friendly name for your search query </umb-localize>
				<uui-form-layout-item>
					<uui-label slot="label"><umb-localize key="logViewer_query">Query</umb-localize>:</uui-label>
					<span>${this.data?.query}</span>
				</uui-form-layout-item>
				<uui-form-layout-item>
					<uui-label slot="label" for="input"><umb-localize key="general_name">Name</umb-localize>:</uui-label>
					<uui-input
						label=${this.localize.term('logViewer_searchName')}
						id="input"
						@input=${this.#validate}></uui-input>
				</uui-form-layout-item>

				<uui-button
					slot="actions"
					@click="${this._handleClose}"
					label=${this.localize.term('general_close')}></uui-button>
				<uui-button
					.disabled=${!this._hasValue}
					slot="actions"
					look="primary"
					color="positive"
					label=${this.localize.term('logViewer_saveSearch')}
					@click="${this._handleSubmit}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
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
