import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../../types.js';
import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from '../../../constants.js';
import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../../../global-contexts/index.js';
import type {
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue,
} from './document-unpublish-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type {
	UmbConfirmActionModalEntityReferencesConfig,
	UmbConfirmActionModalEntityReferencesElement,
} from '@umbraco-cms/backoffice/relations';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import '../../../modals/shared/document-variant-language-picker.element.js';

/**
 * @function isPublished
 * @param {UmbDocumentVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isPublished(option: UmbDocumentVariantOptionModel): boolean {
	return (
		option.variant?.state === UmbDocumentVariantState.PUBLISHED ||
		option.variant?.state === UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES
	);
}

@customElement('umb-document-unpublish-modal')
export class UmbDocumentUnpublishModalElement extends UmbModalBaseElement<
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue
> {
	protected readonly _selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	@state()
	_selection: Array<string> = [];

	@state()
	_canUnpublish = true;

	@state()
	_hasInvalidSelection = true;

	@state()
	_isInvariant = false;

	@state()
	_referencesConfig?: UmbConfirmActionModalEntityReferencesConfig;

	#pickableFilter = (option: UmbDocumentVariantOptionModel) => {
		if (!option.variant) {
			return false;
		}
		return this.data?.pickableFilter ? this.data.pickableFilter(option) : true;
	};

	override firstUpdated() {
		this.#configureReferences();

		// If invariant, don't display the variant selection component.
		if (this.data?.options.length === 1 && this.data.options[0].unique === 'invariant') {
			this._isInvariant = true;
			this._hasInvalidSelection = false;
			return;
		}

		this.#configureSelectionManager();
	}

	#configureReferences() {
		if (!this.data?.documentUnique) return;

		this._referencesConfig = {
			itemRepositoryAlias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
			unique: this.data.documentUnique,
		};
	}

	async #configureSelectionManager() {
		this._selectionManager.setMultiple(true);
		this._selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are published or published with pending changes.
		// If we don't know the state (e.g. from a bulk publishing selection) we need to consider it available for selection.
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
		if (this._canUnpublish) {
			const selection = this._isInvariant ? ['invariant'] : this._selection;
			this.value = { selection };
			this.modalContext?.submit();
			return;
		}
		this.modalContext?.reject();
	}

	#close() {
		this.modalContext?.reject();
	}

	async #onReferencesChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbConfirmActionModalEntityReferencesElement;
		const getReferencedByTotal = target.getTotalReferencedBy();
		const descendantsWithReferencesTotal = target.getTotalDescendantsWithReferences();
		const total = getReferencedByTotal + descendantsWithReferencesTotal;

		if (total > 0) {
			const context = await this.getContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT);
			this._canUnpublish = (await context.getDocumentConfiguration())?.disableUnpublishWhenReferenced === false;
		}
	}

	private _requiredFilter = (variantOption: UmbDocumentVariantOptionModel): boolean => {
		return variantOption.language.isMandatory && !this._selection.includes(variantOption.unique);
	};

	override render() {
		return html`<uui-dialog-layout headline=${this.localize.term('content_unpublish')}>
			${!this._isInvariant
				? html`
						<p id="subtitle">
							<umb-localize key="content_languagesToUnpublish">
								Select the languages to unpublish. Unpublishing a mandatory language will unpublish all languages.
							</umb-localize>
						</p>
						<umb-document-variant-language-picker
							.selectionManager=${this._selectionManager}
							.variantLanguageOptions=${this._options}
							.requiredFilter=${this._hasInvalidSelection ? this._requiredFilter : undefined}
							.pickableFilter=${this.#pickableFilter}></umb-document-variant-language-picker>
					`
				: nothing}

			<p>
				<umb-localize key="prompt_confirmUnpublish">
					Unpublishing will remove this page and all its descendants from the site.
				</umb-localize>
			</p>

			${this._referencesConfig
				? html`<umb-confirm-action-modal-entity-references
						.config=${this._referencesConfig}
						@change=${this.#onReferencesChange}></umb-confirm-action-modal-entity-references>`
				: nothing}

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('actions_unpublish')}"
					?disabled=${this._hasInvalidSelection ||
					!this._canUnpublish ||
					(!this._isInvariant && this._selection.length === 0)}
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
				min-width: 600px;
				max-width: 90vw;
			}

			#references {
				--uui-table-cell-padding: 0;
			}

			#references-warning {
				margin-top: 1rem;
				background-color: var(--uui-color-danger);
				color: var(--uui-color-danger-contrast);
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
