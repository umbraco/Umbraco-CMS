import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../../types.js';
import { isNotPublishedMandatory } from '../../utils.js';
import type { UmbDocumentPublishModalData, UmbDocumentPublishModalValue } from './document-publish-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../../../modals/shared/document-variant-language-picker.element.js';

@customElement('umb-document-publish-modal')
export class UmbDocumentPublishModalElement extends UmbModalBaseElement<
	UmbDocumentPublishModalData,
	UmbDocumentPublishModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	@state()
	_hasNotSelectedMandatory?: boolean;

	override firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft, not-published-mandatory or published with pending changes.
		// If we don't know the state (e.g. from a bulk publishing selection) we need to consider it available for selection.
		this._options =
			this.data?.options.filter(
				(option) =>
					(option.variant && option.variant.state === null) ||
					isNotPublishedMandatory(option) ||
					option.variant?.state !== UmbDocumentVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.value?.selection ?? [];

		const validOptions = this.data?.pickableFilter
			? this._options.filter((o) => this.data!.pickableFilter!(o))
			: this._options;

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

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('content_readyToPublish')}>
			<p id="subtitle">
				<umb-localize key="content_variantsToPublish">Which variants would you like to publish?</umb-localize>
			</p>
			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this._options}
				.requiredFilter=${isNotPublishedMandatory}
				.pickableFilter=${this.data?.pickableFilter}></umb-document-variant-language-picker>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('buttons_saveAndPublish')}"
					look="primary"
					color="positive"
					?disabled=${this._hasNotSelectedMandatory}
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout>`;
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

export default UmbDocumentPublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-publish-modal': UmbDocumentPublishModalElement;
	}
}
