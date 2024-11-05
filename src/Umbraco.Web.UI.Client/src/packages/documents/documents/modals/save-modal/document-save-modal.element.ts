import type { UmbDocumentVariantOptionModel } from '../../types.js';
import type { UmbDocumentSaveModalData, UmbDocumentSaveModalValue } from './document-save-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../shared/document-variant-language-picker.element.js';

@customElement('umb-document-save-modal')
export class UmbDocumentSaveModalElement extends UmbModalBaseElement<
	UmbDocumentSaveModalData,
	UmbDocumentSaveModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	override firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		this._options = this.data?.options ?? [];

		let selected = this.value?.selection ?? [];

		// Filter selection based on options:
		selected = selected.filter((s) => this._options.some((o) => o.unique === s));

		this.#selectionManager.setSelection(selected);
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('content_saveModalTitle')}>
			<p id="subtitle">
				<umb-localize key="content_variantsToSave">Choose which variants to be saved.</umb-localize>
			</p>

			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this._options}
				.pickableFilter=${this.data?.pickableFilter}></umb-document-variant-language-picker>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('buttons_saveAndClose')}"
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 400px;
				max-width: 90vw;
			}
		`,
	];
}

export default UmbDocumentSaveModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-save-modal': UmbDocumentSaveModalElement;
	}
}
