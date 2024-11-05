import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import type { UUIBooleanInputElement } from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	customElement,
	html,
	nothing,
	property,
	repeat,
	state,
	type PropertyValues,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

@customElement('umb-document-variant-language-picker')
export class UmbDocumentVariantLanguagePickerElement extends UmbLitElement {
	#selectionManager!: UmbSelectionManager<string>;

	@property({ type: Array, attribute: false })
	variantLanguageOptions: Array<UmbDocumentVariantOptionModel> = [];

	@property({ attribute: false })
	set selectionManager(value: UmbSelectionManager<string>) {
		this.#selectionManager = value;
		this.observe(
			this.selectionManager.selection,
			(selection) => {
				this._selection = selection;
				this._isAllSelected = this.#isAllSelected();
			},
			'_selectionManager',
		);
	}
	get selectionManager() {
		return this.#selectionManager;
	}

	@state()
	_selection: Array<string> = [];

	@state()
	_isAllSelected?: boolean;

	/**
	 * A filter function that determines if an item is pickableFilter or not.
	 * @memberof UmbDocumentVariantLanguagePickerElement
	 * @returns {boolean} - True if the item is pickableFilter, false otherwise.
	 */
	@property({ attribute: false })
	public pickableFilter?: (item: UmbDocumentVariantOptionModel) => boolean;

	/**
	 * A filter function that determines if an item should be highlighted as a must select.
	 * @memberof UmbDocumentVariantLanguagePickerElement
	 * @returns {boolean} - True if the item is pickableFilter, false otherwise.
	 */
	@property({ attribute: false })
	public requiredFilter?: (item: UmbDocumentVariantOptionModel) => boolean;

	protected override updated(_changedProperties: PropertyValues): void {
		super.updated(_changedProperties);

		if (this.selectionManager && this.pickableFilter) {
			this.#selectionManager.setAllowLimitation((unique) => {
				const option = this.variantLanguageOptions.find((o) => o.unique === unique);
				return option ? this.pickableFilter!(option) : true;
			});
		}
	}

	#onSelectAllChange(event: Event) {
		const allUniques = this.variantLanguageOptions.map((o) => o.unique);
		const filter = this.selectionManager.getAllowLimitation();
		const allowedUniques = allUniques.filter((unique) => filter(unique));

		if ((event.target as UUIBooleanInputElement).checked) {
			this.selectionManager.setSelection(allowedUniques);
		} else {
			this.selectionManager.setSelection([]);
		}
	}

	#isAllSelected() {
		const allUniques = this.variantLanguageOptions.map((o) => o.unique);
		const filter = this.selectionManager.getAllowLimitation();
		const allowedUniques = allUniques.filter((unique) => filter(unique));
		return this._selection.length === allowedUniques.length;
	}

	override render() {
		if (this.variantLanguageOptions.length === 0) {
			return html`<uui-box>
				<umb-localize key="content_noVariantsToProcess">There are no available variants</umb-localize>
			</uui-box>`;
		}

		return html`
			<uui-checkbox
				@change=${this.#onSelectAllChange}
				label=${this.localize.term('general_selectAll')}
				.checked=${this._isAllSelected}></uui-checkbox>
			${repeat(
				this.variantLanguageOptions,
				(option) => option.unique,
				(option) => html` ${this.#renderItem(option)} `,
			)}
		`;
	}

	#renderItem(option: UmbDocumentVariantOptionModel) {
		const pickable = this.pickableFilter ? this.pickableFilter(option) : () => true;
		const selected = this._selection.includes(option.unique);
		const mustSelect = (!selected && this.requiredFilter?.(option)) ?? false;
		return html`
			<uui-menu-item
				class=${mustSelect ? 'required' : ''}
				?selectable=${pickable}
				?disabled=${!pickable}
				label=${option.variant?.name ?? option.language.name}
				@selected=${() => this.selectionManager.select(option.unique)}
				@deselected=${() => this.selectionManager.deselect(option.unique)}
				?selected=${selected}>
				<uui-icon slot="icon" name="icon-globe"></uui-icon>
				${UmbDocumentVariantLanguagePickerElement.renderLabel(option, mustSelect)}
			</uui-menu-item>
		`;
	}

	static renderLabel(option: UmbDocumentVariantOptionModel, mustSelect?: boolean) {
		return html`<div class="label" slot="label">
			<strong> ${option.language.name} </strong>
			<div class="label-status">${UmbDocumentVariantLanguagePickerElement.renderVariantStatus(option)}</div>
			${option.language.isMandatory && mustSelect
				? html`<div class="label-status">
						<umb-localize key="languages_mandatoryLanguage">Mandatory language</umb-localize>
					</div>`
				: nothing}
		</div>`;
	}

	static renderVariantStatus(option: UmbDocumentVariantOptionModel) {
		switch (option.variant?.state) {
			case UmbDocumentVariantState.PUBLISHED:
				return html`<umb-localize key="content_published">Published</umb-localize>`;
			case UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES:
				return html`<umb-localize key="content_publishedPendingChanges">Published with pending changes</umb-localize>`;
			case UmbDocumentVariantState.DRAFT:
				return html`<umb-localize key="content_unpublished">Draft</umb-localize>`;
			case UmbDocumentVariantState.NOT_CREATED:
			default:
				return html`<umb-localize key="content_notCreated">Not created</umb-localize>`;
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			.required {
				color: var(--uui-color-danger);
				--uui-menu-item-color-hover: var(--uui-color-danger-emphasis);
			}
			.label {
				padding: 0.5rem 0;
			}
			.label-status {
				font-size: 0.8rem;
			}

			uui-menu-item {
				--uui-menu-item-flat-structure: 1;
			}

			uui-checkbox {
				margin-bottom: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbDocumentVariantLanguagePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-variant-language-picker': UmbDocumentVariantLanguagePickerElement;
	}
}
