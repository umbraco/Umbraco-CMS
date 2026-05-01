import type { UmbElementItemVariantModel } from '../item/repository/types.js';
import type { UmbElementSearchItemModel } from './types.js';
import { css, customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

@customElement('umb-element-search-result-item')
export class UmbElementSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel & UmbElementSearchItemModel;

	@state()
	private _defaultCulture?: string;

	@state()
	private _variant?: UmbElementItemVariantModel;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.observe(instance?.appLanguageCulture, (value) => {
				this._variant = this.#getVariant(value);
			});
			this.observe(instance?.appDefaultLanguage, (value) => {
				this._defaultCulture = value?.unique;
			});
		});
	}

	#getVariant(culture: string | undefined) {
		return this.item?.variants.find((x) => x.culture === culture);
	}

	#getLabel() {
		if (this.item?.variants[0]?.culture === null) {
			return this.item?.name ?? 'Unknown';
		}

		const fallbackName = this.#getVariant(this._defaultCulture)?.name ?? this.item?.name ?? 'Unknown';
		return this._variant?.name ?? `(${fallbackName})`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			${when(
				this.item.documentType.icon,
				(icon) => html`<umb-icon name=${icon}></umb-icon>`,
				() => html`<uui-icon name="icon-document"></uui-icon>`,
			)}
			<span>${this.#getLabel()}</span>
		`;
	}

	static override styles = [
		css`
			:host {
				border-radius: var(--uui-border-radius);
				outline-offset: -3px;
				padding: var(--uui-size-space-3) var(--uui-size-space-5);

				display: flex;
				gap: var(--uui-size-space-3);
				align-items: center;

				width: 100%;

				> span {
					flex: 1;
				}
			}
		`,
	];
}

export { UmbElementSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-search-result-item': UmbElementSearchResultItemElement;
	}
}
