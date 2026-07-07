import { isNotPublishedMandatory } from '../../utils.js';
import type { UmbContentPublishModalData, UmbContentPublishModalValue } from './types.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

import '../../../variant-picker/content-variant-language-picker.element.js';

@customElement('umb-content-publish-modal')
export class UmbContentPublishModalElement extends UmbModalBaseElement<
	UmbContentPublishModalData,
	UmbContentPublishModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	private _options: Array<UmbEntityVariantOptionModel> = [];

	@state()
	private _hasNotSelectedMandatory?: boolean;

	@state()
	private _hasInvalidSelection = true;

	@state()
	private _isInvariant = false;

	#pickableFilter = (option: UmbEntityVariantOptionModel) => {
		if (!option.variant || option.variant.state === UmbPublishableVariantState.NOT_CREATED) {
			return false;
		}
		return this.data?.pickableFilter ? this.data.pickableFilter(option) : true;
	};

	override firstUpdated() {
		// If invariant, don't display the variant selection component.
		if (this.data?.options.length === 1 && this.data.options[0].culture === null) {
			this._isInvariant = true;
			this._hasInvalidSelection = false;
			return;
		}

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
					option.variant?.state !== UmbPublishableVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.value?.selection ?? [];

		const validOptions = this._options.filter((o) => this.#pickableFilter(o));

		// Filter selection based on options:
		selected = selected.filter((s) => validOptions.some((o) => o.unique === s));

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
		this.value = { selection: this._isInvariant ? ['invariant'] : this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		const headline = this.data?.headline ?? this.localize.term('content_publishModalTitle');

		return html`
			<uui-dialog-layout headline=${headline}>
				<p><umb-localize key="prompt_confirmPublish"></umb-localize></p>

				${when(
					!this._isInvariant,
					() =>
						html`<umb-content-variant-language-picker
							.selectionManager=${this.#selectionManager}
							.variantLanguageOptions=${this._options}
							.requiredFilter=${isNotPublishedMandatory}
							.pickableFilter=${this.#pickableFilter}></umb-content-variant-language-picker>`,
				)}

				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
					<uui-button
						${umbFocus()}
						label="${this.data?.confirmLabel
							? this.localize.string(this.data.confirmLabel)
							: this.localize.term('buttons_saveAndPublish')}"
						look="primary"
						color="positive"
						?disabled=${this._hasNotSelectedMandatory}
						@click=${this.#submit}></uui-button>
				</div>
			</uui-dialog-layout>
		`;
	}

	static override readonly styles = [
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

export default UmbContentPublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-publish-modal': UmbContentPublishModalElement;
	}
}
