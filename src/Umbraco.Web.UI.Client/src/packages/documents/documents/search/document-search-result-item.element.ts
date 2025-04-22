import type { UmbDocumentItemModel, UmbDocumentItemVariantModel } from '../item/repository/types.js';
import {
	classMap,
	css,
	customElement,
	html,
	nothing,
	property,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

@customElement('umb-document-search-result-item')
export class UmbDocumentSearchResultItemElement extends UmbLitElement {
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

	#observeAppCulture(context: typeof UMB_APP_LANGUAGE_CONTEXT.TYPE) {
		this.observe(context.appLanguageCulture, (value) => {
			this._currentCulture = value;
			this._variant = this.#getVariant(value);
		});
	}

	#observeDefaultCulture(context: typeof UMB_APP_LANGUAGE_CONTEXT.TYPE) {
		this.observe(context.appDefaultLanguage, (value) => {
			this._defaultCulture = value?.unique;
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

	#getDraftState(): boolean {
		if (this.item?.isTrashed) return false;
		return this._variant?.state === 'Draft' || this.item?.variants[0]?.state === 'Draft';
	}

	override render() {
		if (!this.item) return nothing;

		const label = this.#getLabel();
		const isDraft = this.#getDraftState();

		const classes = {
			trashed: this.item.isTrashed,
			hasState: this.item.isTrashed || isDraft,
		};

		return html`
			${when(
				this.item.documentType.icon ?? this.item.icon,
				(icon) => html`<umb-icon name=${icon}></umb-icon>`,
				() => html`<uui-icon name="icon-document"></uui-icon>`,
			)}
			<span class=${classMap(classes)}>${label}</span>
			<div class="extra">
				${when(
					this.item.isTrashed,
					() => html`
						<uui-tag look="secondary">
							<umb-localize key="mediaPicker_trashed">Trashed</umb-localize>
						</uui-tag>
					`,
				)}
				${when(!this.item.isTrashed && isDraft, () => html`<uui-tag look="secondary">Draft</uui-tag>`)}
			</div>
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

					&.hasState {
						opacity: 0.6;
					}

					&.trashed {
						text-decoration: line-through;
					}
				}
			}
		`,
	];
}

export { UmbDocumentSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-search-result-item': UmbDocumentSearchResultItemElement;
	}
}
