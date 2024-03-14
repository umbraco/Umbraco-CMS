import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import type {
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue,
} from './document-unpublish-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../shared/document-variant-language-picker.element.js';

@customElement('umb-document-unpublish-modal')
export class UmbDocumentUnpublishModalElement extends UmbModalBaseElement<
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		this._options =
			this.data?.options.filter(
				(option) =>
					option.variant &&
					(!option.variant.state ||
						option.variant.state === UmbDocumentVariantState.PUBLISHED ||
						option.variant.state === UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES),
			) ?? [];

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

	render() {
		return html`<umb-body-layout headline=${this.localize.term('content_unpublish')}>
			<p id="subtitle">
				<umb-localize key="content_languagesToUnpublish">
					Select the languages to unpublish. Unpublishing a mandatory language will unpublish all languages.
				</umb-localize>
			</p>

			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this._options}></umb-document-variant-language-picker>

			<p>
				<umb-localize key="prompt_confirmUnpublish">
					Unpublishing will remove this page and all its descendants from the site.
				</umb-localize>
			</p>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('actions_unpublish')}"
					look="primary"
					color="warning"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static styles = [
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

export default UmbDocumentUnpublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-unpublish-modal': UmbDocumentUnpublishModalElement;
	}
}
