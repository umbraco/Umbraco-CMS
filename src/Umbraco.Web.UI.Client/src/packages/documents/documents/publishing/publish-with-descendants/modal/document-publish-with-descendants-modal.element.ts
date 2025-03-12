import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../../types.js';
import { isNotPublishedMandatory } from '../../utils.js';
import type {
	UmbDocumentPublishWithDescendantsModalData,
	UmbDocumentPublishWithDescendantsModalValue,
} from './document-publish-with-descendants-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../../../modals/shared/document-variant-language-picker.element.js';

@customElement('umb-document-publish-with-descendants-modal')
export class UmbDocumentPublishWithDescendantsModalElement extends UmbModalBaseElement<
	UmbDocumentPublishWithDescendantsModalData,
	UmbDocumentPublishWithDescendantsModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);
	#includeUnpublishedDescendants = false;

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	@state()
	_hasNotSelectedMandatory?: boolean;

	#pickableFilter = (option: UmbDocumentVariantOptionModel) => {
		if (!option.variant) {
			// If not data present, then its not pickable.
			return false;
		}
		return this.data?.pickableFilter ? this.data.pickableFilter(option) : true;
	};

	override firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft, not-published-mandatory or published with pending changes:
		this._options =
			this.data?.options.filter(
				(option) => isNotPublishedMandatory(option) || option.variant?.state !== UmbDocumentVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.value?.selection ?? [];

		const validOptions = this._options.filter((o) => this.#pickableFilter!(o));

		// Filter selection based on options:
		selected = selected.filter((s) => validOptions.some((o) => o.unique === s));

		// Additionally select mandatory languages:
		// [NL]: I think for now lets make it an active choice to select the languages. If you just made them, they would be selected. So it just to underline the act of actually selecting these languages.
		/*
		this._options.forEach((variant) => {
			if (variant.language?.isMandatory) {
				selected.push(variant.unique);
			}
		});
		*/

		this.#selectionManager.setSelection(selected);

		this.observe(
			this.#selectionManager.selection,
			(selection: Array<string>) => {
				if (!this._options && !selection) return;

				//Getting not published mandatory options — the options that are mandatory and not currently published.
				const missingMandatoryOptions = this._options.filter(isNotPublishedMandatory);
				this._hasNotSelectedMandatory = missingMandatoryOptions.some((option) => !selection.includes(option.unique));
			},
			'observeSelection',
		);
	}

	#onIncludeUnpublishedDescendantsChange() {
		this.#includeUnpublishedDescendants = !this.#includeUnpublishedDescendants;
	}

	async #submit() {

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
				.requiredFilter=${isNotPublishedMandatory}
				.pickableFilter=${this.#pickableFilter}></umb-document-variant-language-picker>

			<uui-form-layout-item>
				<uui-toggle
					id="includeUnpublishedDescendants"
					label=${this.localize.term('content_includeUnpublished')}
					?checked=${this.value?.includeUnpublishedDescendants}
					@change=${this.#onIncludeUnpublishedDescendantsChange}></uui-toggle>
			</uui-form-layout-item>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('buttons_publishDescendants')}"
					look="primary"
					color="positive"
					?disabled=${this._hasNotSelectedMandatory}
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				min-width: 460px;
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
