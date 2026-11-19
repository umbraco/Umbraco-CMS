import type { UmbSortChildrenOfDocumentByFieldArgs, UmbSortChildrenOfDocumentByFieldOption } from '../types.js';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbSortChildrenOfContentModalElement } from '@umbraco-cms/backoffice/content';
import { UMB_APP_LANGUAGE_CONTEXT, sortLanguages } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-sort-children-of-document-modal')
export class UmbSortChildrenOfDocumentModalElement extends UmbSortChildrenOfContentModalElement {
	@state()
	private _languages: Array<UmbLanguageDetailModel> = [];

	@state()
	private _selectedCulture?: string;

	@state()
	private _hasMultipleLanguages = false;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(context?.languages, (languages) => {
				this._languages = [...(languages ?? [])].sort(sortLanguages);
			});

			this.observe(context?.moreThanOneLanguage, (moreThanOneLanguage) => {
				this._hasMultipleLanguages = moreThanOneLanguage ?? false;
			});

			this.observe(context?.appLanguageCulture, (culture) => {
				this._selectedCulture ??= culture;
			});
		});
	}

	protected override _getSortByFieldArgs(): UmbSortChildrenOfDocumentByFieldArgs {
		return {
			...super._getSortByFieldArgs(),
			culture: this.#selectedFieldVariesByCulture() ? this._selectedCulture : undefined,
		};
	}

	#selectedFieldVariesByCulture() {
		const options = this._sortByFieldOptions as Array<UmbSortChildrenOfDocumentByFieldOption>;
		return options.find((option) => option.value === this._selectedField)?.variesByCulture === true;
	}

	protected override _renderAdditionalSortByFieldOptions() {
		if (!this._hasMultipleLanguages || !this.#selectedFieldVariesByCulture()) return nothing;

		const options = this._languages.map((language) => ({
			name: language.name,
			value: language.unique,
			selected: language.unique === this._selectedCulture,
		}));

		return html`
			<span><umb-localize key="sort_sortByFieldCultureSentence">in</umb-localize></span>
			<uui-select
				label=${this.localize.term('sort_sortByFieldCultureLabel')}
				.options=${options}
				@change=${this.#onCultureChange}></uui-select>
		`;
	}

	#onCultureChange(event: UUISelectEvent) {
		this._selectedCulture = event.target.value as string;
	}
}

export { UmbSortChildrenOfDocumentModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-sort-children-of-document-modal': UmbSortChildrenOfDocumentModalElement;
	}
}
