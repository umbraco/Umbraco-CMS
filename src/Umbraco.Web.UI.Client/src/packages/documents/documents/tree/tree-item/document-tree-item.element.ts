import type { UmbDocumentTreeItemModel } from '../types.js';
import type { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import { css, html, nothing, customElement, classMap, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbTreeItemElementBase<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeItemContext
> {
	#api: UmbDocumentTreeItemContext | undefined;
	@property({ type: Object, attribute: false })
	public override get api(): UmbDocumentTreeItemContext | undefined {
		return this.#api;
	}
	public override set api(value: UmbDocumentTreeItemContext | undefined) {
		this.#api = value;

		if (this.#api) {
			this.observe(this.#api.name, (name) => (this._name = name || ''));
			this.observe(this.#api.isDraft, (isDraft) => (this._isDraft = isDraft || false));
			this.observe(this.#api.icon, (icon) => (this._icon = icon || ''));
		}

		super.api = value;
	}

	@state()
	private _name = '';

	@state()
	private _isDraft = false;

	@state()
	private _icon = '';

	override renderIconContainer() {
		const icon = this._icon;
		const iconWithoutColor = icon?.split(' ')[0];

		return html`
			<span id="icon-container" slot="icon" class=${classMap({ draft: this._isDraft })}>
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
		return html`<span id="label" slot="label" class=${classMap({ draft: this._isDraft })}>${this._name}</span> `;
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
