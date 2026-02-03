import { UmbElementVariantState, type UmbElementVariantOptionModel } from '../../../types.js';
import type { UmbElementPublishModalData, UmbElementPublishModalValue } from './element-publish-modal.token.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../../../modals/shared/element-variant-language-picker.element.js';

/**
 * Helper function to check if a variant is not published and has a mandatory language
 * @param option
 */
function isNotPublishedMandatory(option: UmbElementVariantOptionModel) {
	return (
		option.language?.isMandatory === true &&
		option.variant?.state !== UmbElementVariantState.PUBLISHED &&
		option.variant?.state !== UmbElementVariantState.PUBLISHED_PENDING_CHANGES
	);
}

@customElement('umb-element-publish-modal')
export class UmbElementPublishModalElement extends UmbModalBaseElement<
	UmbElementPublishModalData,
	UmbElementPublishModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	private _options: Array<UmbElementVariantOptionModel> = [];

	@state()
	private _hasNotSelectedMandatory?: boolean;

	@state()
	private _hasInvalidSelection = true;

	@state()
	private _isInvariant = false;

	#pickableFilter = (option: UmbElementVariantOptionModel) => {
		if (!option.variant || option.variant.state === UmbElementVariantState.NOT_CREATED) {
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

		// Only display variants that are relevant to pick from
		this._options =
			this.data?.options.filter(
				(option) =>
					(option.variant && option.variant.state === null) ||
					isNotPublishedMandatory(option) ||
					option.variant?.state !== UmbElementVariantState.NOT_CREATED,
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

				// Getting not published mandatory options
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
						html`<umb-element-variant-language-picker
							.selectionManager=${this.#selectionManager}
							.variantLanguageOptions=${this._options}
							.requiredFilter=${isNotPublishedMandatory}
							.pickableFilter=${this.#pickableFilter}></umb-element-variant-language-picker>`,
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

export default UmbElementPublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-publish-modal': UmbElementPublishModalElement;
	}
}
