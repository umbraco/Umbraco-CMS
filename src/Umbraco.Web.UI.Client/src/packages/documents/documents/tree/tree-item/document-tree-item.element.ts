import type { UmbDocumentTreeItemModel, UmbDocumentTreeItemVariantModel } from '../types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { css, html, nothing, customElement, state, classMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbTreeItemElementBase<UmbDocumentTreeItemModel> {
	#appLanguageContext?: UmbAppLanguageContext;

	@state()
	_currentCulture?: string;

	@state()
	_defaultCulture?: string;

	@state()
	_variant?: UmbDocumentTreeItemVariantModel;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguageContext = instance;
			this.#observeAppCulture();
			this.#observeDefaultCulture();
		});
	}

	#observeAppCulture() {
		this.observe(this.#appLanguageContext!.appLanguageCulture, (value) => {
			this._currentCulture = value;
			this._variant = this.#findVariant(value);
		});
	}

	#observeDefaultCulture() {
		this.observe(this.#appLanguageContext!.appDefaultLanguage, (value) => {
			this._defaultCulture = value?.unique;
		});
	}

	#findVariant(culture: string | undefined) {
		return this.item?.variants.find((x) => x.culture === culture);
	}

	#isInvariant() {
		const firstVariant = this.item?.variants[0];
		return firstVariant?.culture === null && firstVariant?.segment === null;
	}

	// TODO: we should move the fallback name logic to a helper class. It will be used in multiple places
	#getLabel() {
		if (this.#isInvariant()) {
			return this._item?.variants[0].name;
		}

		// ensure we always have the correct variant data
		this._variant = this.#findVariant(this._currentCulture);

		const fallbackName = this.#findVariant(this._defaultCulture)?.name ?? this._item?.variants[0].name ?? 'Unknown';
		return this._variant?.name ?? `(${fallbackName})`;
	}

	#isDraft() {
		if (this.#isInvariant()) {
			return this._item?.variants[0].state === DocumentVariantStateModel.DRAFT;
		}

		// ensure we always have the correct variant data
		this._variant = this.#findVariant(this._currentCulture);

		return this._variant?.state === DocumentVariantStateModel.DRAFT;
	}

	override renderIconContainer() {
		const icon = this.item?.documentType.icon;
		const iconWithoutColor = icon?.split(' ')[0];

		return html`
			<span id="icon-container" slot="icon" class=${classMap({ draft: this.#isDraft() })}>
				${icon && iconWithoutColor
					? html`
							<umb-icon id="icon" slot="icon" name="${this._isActive ? iconWithoutColor : icon}"></umb-icon>
							${this.#renderStateIcon()}
						`
					: nothing}
			</span>
		`;
	}

	override renderLabel() {
		return html`<span id="label" slot="label" class=${classMap({ draft: this.#isDraft() })}
			>${this.#getLabel()}</span
		> `;
	}

	#renderStateIcon() {
		if (this.item?.isProtected) {
			return this.#renderIsProtectedIcon();
		}

		if (this.item?.documentType.collection) {
			return this.#renderIsCollectionIcon();
		}

		return nothing;
	}

	#renderIsCollectionIcon() {
		return html`<umb-icon id="state-icon" slot="icon" name="icon-grid" title="Collection"></umb-icon>`;
	}

	#renderIsProtectedIcon() {
		return html`<umb-icon id="state-icon" slot="icon" name="icon-lock" title="Protected"></umb-icon>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#icon-container {
				position: relative;
			}

			#icon {
				vertical-align: middle;
			}

			#label {
				white-space: nowrap;
				overflow: hidden;
				text-overflow: ellipsis;
			}

			#state-icon {
				position: absolute;
				bottom: -5px;
				right: -5px;
				font-size: 10px;
				background: var(--uui-color-surface);
				width: 14px;
				height: 14px;
				border-radius: 100%;
				line-height: 14px;
			}

			:hover #state-icon {
				background: var(--uui-color-surface-emphasis);
			}

			/** Active */
			[active] #state-icon {
				background: var(--uui-color-current);
			}

			[active]:hover #state-icon {
				background: var(--uui-color-current-emphasis);
			}

			/** Selected */
			[selected] #state-icon {
				background-color: var(--uui-color-selected);
			}

			[selected]:hover #state-icon {
				background-color: var(--uui-color-selected-emphasis);
			}

			/** Disabled */
			[disabled] #state-icon {
				background-color: var(--uui-color-disabled);
			}

			.draft {
				opacity: 0.6;
			}
		`,
	];
}

export default UmbDocumentTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree-item': UmbDocumentTreeItemElement;
	}
}
