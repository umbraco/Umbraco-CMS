import { UmbElementVariantState, type UmbElementVariantOptionModel } from '../../../types.js';
import type { UmbElementUnpublishModalData, UmbElementUnpublishModalValue } from './element-unpublish-modal.token.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../../../modals/shared/element-variant-language-picker.element.js';

/**
 * @function isPublished
 * @param {UmbElementVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isPublished(option: UmbElementVariantOptionModel): boolean {
	return (
		option.variant?.state === UmbElementVariantState.PUBLISHED ||
		option.variant?.state === UmbElementVariantState.PUBLISHED_PENDING_CHANGES
	);
}

@customElement('umb-element-unpublish-modal')
export class UmbElementUnpublishModalElement extends UmbModalBaseElement<
	UmbElementUnpublishModalData,
	UmbElementUnpublishModalValue
> {
	protected readonly _selectionManager = new UmbSelectionManager<string>(this);

	@state()
	private _options: Array<UmbElementVariantOptionModel> = [];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _hasInvalidSelection = true;

	@state()
	private _isInvariant = false;

	#pickableFilter = (option: UmbElementVariantOptionModel) => {
		if (!option.variant) {
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
		this._selectionManager.setMultiple(true);
		this._selectionManager.setSelectable(true);

		// Only display variants that are published or published with pending changes.
		this._options =
			this.data?.options.filter((option) => (option.variant && option.variant.state === null) || isPublished(option)) ??
			[];

		let selected = this.value?.selection ?? [];

		const validOptions = this._options.filter((o) => this.#pickableFilter(o));

		// Filter selection based on options:
		selected = selected.filter((s) => validOptions.some((o) => o.unique === s));

		this._selectionManager.setSelection(selected);

		this.observe(
			this._selectionManager.selection,
			(selection) => {
				this._selection = selection;
				const selectionHasMandatory = this._options.some((o) => o.language.isMandatory && selection.includes(o.unique));
				const selectionDoesNotHaveAllMandatory = this._options.some(
					(o) => o.language.isMandatory && !selection.includes(o.unique),
				);
				this._hasInvalidSelection = selectionHasMandatory && selectionDoesNotHaveAllMandatory;
			},
			'observeSelection',
		);
	}

	#submit() {
		const selection = this._isInvariant ? ['invariant'] : this._selection;
		this.value = { selection };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	private _requiredFilter = (variantOption: UmbElementVariantOptionModel): boolean => {
		return variantOption.language.isMandatory && !this._selection.includes(variantOption.unique);
	};

	override render() {
		return html`<uui-dialog-layout headline=${this.localize.term('content_unpublish')}>
			<p>
				<umb-localize key="prompt_confirmUnpublish"></umb-localize>
			</p>
			${when(
				!this._isInvariant,
				() => html`
					<umb-element-variant-language-picker
						.selectionManager=${this._selectionManager}
						.variantLanguageOptions=${this._options}
						.requiredFilter=${this._hasInvalidSelection ? this._requiredFilter : undefined}
						.pickableFilter=${this.#pickableFilter}></umb-element-variant-language-picker>
				`,
			)}

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('actions_unpublish')}"
					?disabled=${this._hasInvalidSelection || (!this._isInvariant && this._selection.length === 0)}
					look="primary"
					color="warning"
					@click=${this.#submit}></uui-button>
			</div>
		</uui-dialog-layout> `;
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

export default UmbElementUnpublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-unpublish-modal': UmbElementUnpublishModalElement;
	}
}
