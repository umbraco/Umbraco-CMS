import type { UmbContentConfigurationRepository } from '../../../configuration/types.js';
import type { UmbContentUnpublishModalData, UmbContentUnpublishModalValue } from './types.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type {
	UmbConfirmActionModalEntityReferencesConfig,
	UmbConfirmActionModalEntityReferencesElement,
} from '@umbraco-cms/backoffice/relations';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

import '../../../variant-picker/content-variant-language-picker.element.js';

/**
 * @function isPublished
 * @param {UmbEntityVariantOptionModel} option - the option to check.
 * @returns {boolean} boolean
 */
export function isPublished(option: UmbEntityVariantOptionModel): boolean {
	return (
		option.variant?.state === UmbPublishableVariantState.PUBLISHED ||
		option.variant?.state === UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES
	);
}

@customElement('umb-content-unpublish-modal')
export class UmbContentUnpublishModalElement extends UmbModalBaseElement<
	UmbContentUnpublishModalData,
	UmbContentUnpublishModalValue
> {
	protected readonly _selectionManager = new UmbSelectionManager<string>(this);

	@state()
	private _options: Array<UmbEntityVariantOptionModel> = [];

	@state()
	private _selection: Array<string> = [];

	// Three-state model for reference-aware unpublishing:
	//   undefined = loading (button disabled, no bypass possible while references are still being checked)
	//   false     = blocked (button disabled, references exist and disableUnpublishWhenReferenced is set)
	//   true       = allowed (button enabled)
	@state()
	private _canUnpublish: boolean | undefined = undefined;

	@state()
	private _hasInvalidSelection = true;

	@state()
	private _isInvariant = false;

	@state()
	private _referencesConfig?: UmbConfirmActionModalEntityReferencesConfig;

	readonly #pickableFilter = (option: UmbEntityVariantOptionModel) => {
		if (!option.variant) {
			return false;
		}
		return this.data?.pickableFilter ? this.data.pickableFilter(option) : true;
	};

	override firstUpdated() {
		this.#configureReferences();

		// If invariant, don't display the variant selection component.
		if (this.data?.options.length === 1 && this.data.options[0].culture === null) {
			this._isInvariant = true;
			this._hasInvalidSelection = false;
			return;
		}

		this.#configureSelectionManager();
	}

	#configureReferences() {
		if (!this.#hasReferencesConfig()) {
			this._canUnpublish = true;
			return;
		}

		this._referencesConfig = {
			itemRepositoryAlias: this.data!.itemRepositoryAlias!,
			referenceRepositoryAlias: this.data!.referenceRepositoryAlias!,
			unique: this.data!.unique!,
		};
	}

	#hasReferencesConfig(): boolean {
		return !!this.data?.unique && !!this.data.itemRepositoryAlias && !!this.data.referenceRepositoryAlias;
	}

	async #configureSelectionManager() {
		this._selectionManager.setMultiple(true);
		this._selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are published or published with pending changes.
		// If we don't know the state (e.g. from a bulk publishing selection) we need to consider it available for selection.
		this._options = this.data?.options.filter((option) => option.variant?.state === null || isPublished(option)) ?? [];

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
		const total = target.getTotalReferencedBy() + target.getTotalDescendantsWithReferences();

		if (total === 0) {
			this._canUnpublish = true;
			return;
		}

		if (!this.data?.configurationRepositoryAlias) {
			this._canUnpublish = true;
			return;
		}

		try {
			const configurationRepository = await createExtensionApiByAlias<UmbContentConfigurationRepository>(
				this,
				this.data.configurationRepositoryAlias,
			);
			const { data: configuration } = await configurationRepository.requestConfiguration();
			this._canUnpublish = (configuration?.disableUnpublishWhenReferenced ?? false) !== true;
		} catch {
			// Configuration could not be resolved — fail open, consistent with the "configuration unavailable" default.
			this._canUnpublish = true;
		}
	}

	private readonly _requiredFilter = (variantOption: UmbEntityVariantOptionModel): boolean => {
		return variantOption.language.isMandatory && !this._selection.includes(variantOption.unique);
	};

	override render() {
		return html`
			<uui-dialog-layout headline=${this.localize.term('content_unpublish')}>
				<p>
					<umb-localize key="prompt_confirmUnpublish"></umb-localize>
				</p>
				${when(
					!this._isInvariant,
					() => html`
						<umb-content-variant-language-picker
							.selectionManager=${this._selectionManager}
							.variantLanguageOptions=${this._options}
							.requiredFilter=${this._hasInvalidSelection ? this._requiredFilter : undefined}
							.pickableFilter=${this.#pickableFilter}
							.renderAdditionalLabel=${this.data?.renderAdditionalLabel}>
						</umb-content-variant-language-picker>
					`,
				)}
				${when(
					this._referencesConfig,
					() => html`
						<umb-confirm-action-modal-entity-references
							.config=${this._referencesConfig}
							@change=${this.#onReferencesChange}>
						</umb-confirm-action-modal-entity-references>
					`,
				)}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
					<uui-button
						label=${this.localize.term('actions_unpublish')}
						look="primary"
						color="warning"
						?disabled=${this._hasInvalidSelection ||
						!this._canUnpublish ||
						(!this._isInvariant && this._selection.length === 0)}
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

export default UmbContentUnpublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-unpublish-modal': UmbContentUnpublishModalElement;
	}
}
