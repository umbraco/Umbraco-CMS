import { UmbCultureRepository } from '../../repository/culture.repository.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, repeat, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { CultureReponseModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-input-culture-select')
export class UmbInputCultureSelectElement extends UUIFormControlMixin(UmbLitElement, '') {
	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _cultures: CultureReponseModel[] = [];

	@state()
	private _search = '';

	public selectedCultureName?: string;

	#cultureRepository = new UmbCultureRepository(this);

	protected override getFormElement() {
		return undefined;
	}

	protected override async firstUpdated() {
		const { data } = await this.#cultureRepository.requestCultures();
		if (data) {
			this._cultures = data.items;
		}
	}

	#onSearchChange(event: UUIComboboxEvent) {
		event.stopPropagation();
		const target = event.composedPath()[0] as UUIComboboxElement;
		this._search = target.search;
	}

	#onCultureChange(event: UUIComboboxEvent) {
		event.stopPropagation();
		const target = event.composedPath()[0] as UUIComboboxElement;
		this.value = target.value;
		const culture = this._cultures.find(
			(culture) => culture.name.toLowerCase() === (this.value as string).toLowerCase(),
		);
		this.selectedCultureName = culture?.englishName;
		this.dispatchEvent(new UmbChangeEvent());
	}

	get #filteredCultures() {
		return this._cultures.filter((culture) => {
			return culture.englishName?.toLowerCase().includes(this._search.toLowerCase());
		});
	}

	get #fromAvailableCultures() {
		return this._cultures.find((culture) => culture.name.toLowerCase() === (this.value as string)?.toLowerCase());
	}

	override render() {
		return html`
			<!-- TODO: comboxbox doesn't support disabled or readonly mode yet. This is a temp solution -->
			${this.disabled || this.readonly
				? html`${this.#fromAvailableCultures?.englishName}`
				: html`
						<uui-combobox
							value=${ifDefined(this.#fromAvailableCultures?.name)}
							@change=${this.#onCultureChange}
							@search=${this.#onSearchChange}>
							<uui-combobox-list>
								${repeat(
									this.#filteredCultures,
									(culture) => culture.name,
									(culture) => html`
										<uui-combobox-list-option value=${ifDefined(culture.name)}
											>${culture.englishName}</uui-combobox-list-option
										>
									`,
								)}
							</uui-combobox-list>
						</uui-combobox>
					`}
		`;
	}
}

export default UmbInputCultureSelectElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-culture-select': UmbInputCultureSelectElement;
	}
}
