import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import type {
	UmbDocumentPublishWithDescendantsModalData,
	UmbDocumentPublishWithDescendantsModalValue,
} from './document-publish-with-descendants-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../shared/document-variant-language-picker.element.js';

@customElement('umb-document-publish-with-descendants-modal')
export class UmbDocumentPublishWithDescendantsModalElement extends UmbModalBaseElement<
	UmbDocumentPublishWithDescendantsModalData,
	UmbDocumentPublishWithDescendantsModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);
	#includeUnpublishedDescendants = false;

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	override firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		this._options =
			this.data?.options.filter(
				(option) => option.variant && option.variant.state !== UmbDocumentVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.value?.selection ?? [];

		// Filter selection based on options:
		selected = selected.filter((s) => this._options.some((o) => o.unique === s));

		this.#selectionManager.setSelection(selected);

		// Additionally select mandatory languages:
		this._options.forEach((variant) => {
			if (variant.language?.isMandatory) {
				this.#selectionManager.select(variant.unique);
			}
		});
	}

	#submit() {
		this.value = {
			selection: this.#selectionManager.getSelection(),
			includeUnpublishedDescendants: this.#includeUnpublishedDescendants,
		};
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('buttons_publishDescendants')}>
			<p id="subtitle">
				${this._options.length === 1
					? html`<umb-localize
							key="content_publishDescendantsHelp"
							.args=${[this._options[0].variant?.name ?? this._options[0].language.name]}>
							Publish <strong>${this._options[0].variant?.name}</strong> and all content items underneath and thereby
							making their content publicly available.
						</umb-localize>`
					: html`
							<umb-localize key="content_publishDescendantsWithVariantsHelp">
								Publish variants and variants of same type underneath and thereby making their content publicly
								available.
							</umb-localize>
						`}
			</p>

			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this._options}
				.pickableFilter=${this.data?.pickableFilter}></umb-document-variant-language-picker>

			<uui-form-layout-item>
				<uui-toggle
					id="includeUnpublishedDescendants"
					label=${this.localize.term('content_includeUnpublished')}
					?checked=${this.value?.includeUnpublishedDescendants}
					@change=${() => (this.#includeUnpublishedDescendants = !this.#includeUnpublishedDescendants)}></uui-toggle>
			</uui-form-layout-item>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('buttons_publishDescendants')}"
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

export default UmbDocumentPublishWithDescendantsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-publish-with-descendants-modal': UmbDocumentPublishWithDescendantsModalElement;
	}
}
