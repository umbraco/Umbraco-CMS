import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { ifDefined } from 'lit/directives/if-defined.js';
import { repeat } from 'lit/directives/repeat.js';
import { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-ui/uui';
import { UmbCultureRepository } from '../../../settings/cultures/repository/culture.repository';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { CultureReponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-culture-select')
export class UmbInputCultureSelectElement extends FormControlMixin(UmbLitElement) {
	static styles = [UUITextStyles, css``];

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

	protected getFormElement() {
		return undefined;
	}

	protected async firstUpdated() {
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
		this._value = target.value;
		const culture = this._cultures.find((culture) => culture.name === this._value);
		this.selectedCultureName = culture?.englishName;
		this.dispatchEvent(new UmbChangeEvent());
	}

	get #filteredCultures() {
		return this._cultures.filter((culture) => {
			return culture.englishName?.toLowerCase().includes(this._search.toLowerCase());
		});
	}

	get #fromAvailableCultures() {
		return this._cultures.find((culture) => culture.name === this.value);
	}

	render() {
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
									(culture) =>
										html`
											<uui-combobox-list-option value=${ifDefined(culture.name)}
												>${culture.englishName}</uui-combobox-list-option
											>
										`
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
