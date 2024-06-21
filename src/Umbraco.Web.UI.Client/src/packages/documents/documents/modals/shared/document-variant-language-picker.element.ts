import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
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
			async (selection) => {
				this._selection = selection;
			},
			'_selectionManager',
		);
	}
	get selectionManager() {
		return this.#selectionManager;
	}

	@state()
	_selection: Array<string> = [];

	render() {
		return this.variantLanguageOptions.length
			? repeat(
					this.variantLanguageOptions,
					(option) => option.unique,
					(option) => html`
						<uui-menu-item
							selectable
							label=${option.variant?.name ?? option.language.name}
							@selected=${() => this.selectionManager.select(option.unique)}
							@deselected=${() => this.selectionManager.deselect(option.unique)}
							?selected=${this._selection.includes(option.unique)}>
							<uui-icon slot="icon" name="icon-globe"></uui-icon>
							${UmbDocumentVariantLanguagePickerElement.renderLabel(option)}
						</uui-menu-item>
					`,
				)
			: html`<uui-box>
					<umb-localize key="content_noVariantsToProcess">There are no available variants</umb-localize>
				</uui-box>`;
	}

	static renderLabel(option: UmbDocumentVariantOptionModel) {
		return html`<div class="label" slot="label">
			<strong>
				${option.variant?.segment ? option.variant.segment + ' - ' : ''}${option.variant?.name ?? option.language.name}
			</strong>
			<div class="label-status">${UmbDocumentVariantLanguagePickerElement.renderVariantStatus(option)}</div>
			${option.language.isMandatory && option.variant?.state !== UmbDocumentVariantState.PUBLISHED
				? html`<div class="label-status">
						<umb-localize key="languages_mandatoryLanguage">Mandatory language</umb-localize>
					</div>`
				: ''}
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
				return html`<umb-localize key="content_notCreated">Not created</umb-localize>`;
			default:
				return nothing;
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			#subtitle {
				margin-top: 0;
			}
			.label {
				padding: 0.5rem 0;
			}
			.label-status {
				font-size: 0.8rem;
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
