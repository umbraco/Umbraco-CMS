import { css, customElement, html, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { sortVariants, UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UUIBooleanInputElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-content-variant-language-picker')
export class UmbContentVariantLanguagePickerElement extends UmbLitElement {
	#selectionManager!: UmbSelectionManager<string>;

	@property({ type: Array, attribute: false })
	public get variantLanguageOptions(): Array<UmbEntityVariantOptionModel> {
		return this.#variantLanguageOptions;
	}
	public set variantLanguageOptions(value: Array<UmbEntityVariantOptionModel>) {
		this.#variantLanguageOptions = [...value].sort(sortVariants);
	}
	#variantLanguageOptions: Array<UmbEntityVariantOptionModel> = [];

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
	private _selection: Array<string> = [];

	@state()
	private _isAllSelected: boolean = false;

	/**
	 * A filter function that determines if an item is pickable or not.
	 * @memberof UmbContentVariantLanguagePickerElement
	 * @returns {boolean} - True if the item is pickable, false otherwise.
	 */
	@property({ attribute: false })
	public pickableFilter?: (item: UmbEntityVariantOptionModel) => boolean;

	/**
	 * A filter function that determines if an item should be highlighted as a must select.
	 * @memberof UmbContentVariantLanguagePickerElement
	 * @returns {boolean} - True if the item is required, false otherwise.
	 */
	@property({ attribute: false })
	public requiredFilter?: (item: UmbEntityVariantOptionModel) => boolean;

	/**
	 * A render callback letting callers inject supplementary label content (e.g. a reference count) for an option,
	 * without this shared component needing to know about content-type-specific fields.
	 * @memberof UmbContentVariantLanguagePickerElement
	 */
	@property({ attribute: false })
	public renderAdditionalLabel?: (option: UmbEntityVariantOptionModel) => unknown;

	protected override updated(_changedProperties: PropertyValues): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('variantLanguageOptions')) {
			this._isAllSelected = this.#isAllSelected();
		}

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
		return this._selection.length !== 0 && this._selection.length === allowedUniques.length;
	}

	override render() {
		if (this.variantLanguageOptions.length === 0) {
			return html`
				<uui-box>
					<umb-localize key="content_noVariantsToProcess">There are no available variants</umb-localize>
				</uui-box>
			`;
		}

		return html`
			<uui-checkbox
				@change=${this.#onSelectAllChange}
				label=${this.localize.term('general_selectAll')}
				.checked=${this._isAllSelected}></uui-checkbox>
			${repeat(
				this.variantLanguageOptions,
				(option) => option.unique,
				(option) => this.#renderItem(option),
			)}
		`;
	}

	#renderItem(option: UmbEntityVariantOptionModel) {
		const pickable = this.pickableFilter ? this.pickableFilter(option) : true;
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
				${UmbContentVariantLanguagePickerElement.renderLabel(option, mustSelect, this.renderAdditionalLabel)}
			</uui-menu-item>
		`;
	}

	static renderLabel(
		option: UmbEntityVariantOptionModel,
		mustSelect?: boolean,
		renderAdditionalLabel?: (option: UmbEntityVariantOptionModel) => unknown,
	) {
		return html` <div class="label" slot="label">
			<strong> ${option.language.name} </strong>
			<div class="label-status">${UmbContentVariantLanguagePickerElement.renderVariantStatus(option)}</div>
			${renderAdditionalLabel?.(option)}
			${when(
				option.language.isMandatory && mustSelect,
				() => html`
					<div class="label-status">
						<umb-localize key="languages_mandatoryLanguage">Mandatory language</umb-localize>
					</div>
				`,
			)}
		</div>`;
	}

	static renderVariantStatus(option: UmbEntityVariantOptionModel) {
		switch (option.variant?.state) {
			case UmbPublishableVariantState.PUBLISHED:
				return html`<umb-localize key="content_published">Published</umb-localize>`;
			case UmbPublishableVariantState.PUBLISHED_PENDING_CHANGES:
				return html`<umb-localize key="content_publishedPendingChanges">Published with pending changes</umb-localize>`;
			case UmbPublishableVariantState.DRAFT:
				return html`<umb-localize key="content_unpublished">Draft</umb-localize>`;
			default:
				return html`<umb-localize key="content_notCreated">Not created</umb-localize>`;
		}
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			.required {
				color: var(--uui-color-danger);
				--uui-menu-item-color-hover: var(--uui-color-danger-emphasis);
				--uui-menu-item-color-disabled: var(--uui-color-danger);
			}
			.label {
				padding: var(--uui-size-space-3) 0;
			}
			.label-status {
				font-size: var(--uui-type-small-size);
			}

			uui-menu-item {
				--uui-menu-item-flat-structure: 1;
				--uui-menu-item-border-radius: var(--uui-border-radius);
			}

			uui-checkbox {
				margin-bottom: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbContentVariantLanguagePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-variant-language-picker': UmbContentVariantLanguagePickerElement;
	}
}
