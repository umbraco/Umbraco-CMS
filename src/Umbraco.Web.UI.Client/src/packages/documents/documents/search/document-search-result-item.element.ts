import type { UmbDocumentItemModel, UmbDocumentItemVariantModel } from '../repository/item/types.js';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';

const elementName = 'umb-document-search-result-item';
@customElement(elementName)
export class UmbSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel & UmbDocumentItemModel;

	@state()
	_currentCulture?: string;

	@state()
	_defaultCulture?: string;

	@state()
	_variant?: UmbDocumentItemVariantModel;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#observeAppCulture(instance);
			this.#observeDefaultCulture(instance);
		});
	}

	#observeAppCulture(context: UmbAppLanguageContext) {
		this.observe(context.appLanguageCulture, (value) => {
			this._currentCulture = value;
			this._variant = this.#getVariant(value);
		});
	}

	#observeDefaultCulture(context: UmbAppLanguageContext) {
		this.observe(context.appDefaultLanguage, (value) => {
			this._defaultCulture = value?.unique;
		});
	}

	#getVariant(culture: string | undefined) {
		return this.item?.variants.find((x) => x.culture === culture);
	}

	#isInvariant() {
		const firstVariant = this.item?.variants[0];
		return firstVariant?.culture === null;
	}

	#getLabel() {
		if (this.#isInvariant()) {
			return this.item?.name ?? 'Unknown';
		}

		const fallbackName = this.#getVariant(this._defaultCulture)?.name ?? this.item?.name ?? 'Unknown';
		return this._variant?.name ?? `(${fallbackName})`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<span class="item-icon">
				${this.item.icon ? html`<umb-icon name="${this.item.icon}"></umb-icon>` : this.#renderHashTag()}
			</span>
			<span class="item-name"> ${this.#getLabel()} </span>
		`;
	}

	#renderHashTag() {
		return html`
			<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
				<path fill="none" d="M0 0h24v24H0z" />
				<path
					fill="currentColor"
					d="M7.784 14l.42-4H4V8h4.415l.525-5h2.011l-.525 5h3.989l.525-5h2.011l-.525 5H20v2h-3.784l-.42 4H20v2h-4.415l-.525 5h-2.011l.525-5H9.585l-.525 5H7.049l.525-5H4v-2h3.784zm2.011 0h3.99l.42-4h-3.99l-.42 4z" />
			</svg>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				padding: var(--uui-size-space-3) var(--uui-size-space-5);
				border-radius: var(--uui-border-radius);
				display: grid;
				grid-template-columns: var(--uui-size-space-6) 1fr var(--uui-size-space-5);
				align-items: center;
				width: 100%;
				outline-offset: -3px;
			}
			.item-icon {
				margin-bottom: auto;
				margin-top: 5px;
			}
			.item-icon {
				opacity: 0.4;
			}
			.item-name {
				display: flex;
				flex-direction: column;
			}
			.item-icon > * {
				height: 1rem;
				display: flex;
				width: min-content;
			}
			a {
				text-decoration: none;
				color: inherit;
			}
		`,
	];
}

export { UmbSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbSearchResultItemElement;
	}
}
