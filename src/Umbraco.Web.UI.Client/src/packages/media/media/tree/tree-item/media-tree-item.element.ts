import type { UmbMediaTreeItemModel } from '../types.js';
import type { UmbMediaTreeItemContext } from './media-tree-item.context.js';
import { css, html, customElement, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-media-tree-item';
@customElement(elementName)
export class UmbMediaTreeItemElement extends UmbTreeItemElementBase<UmbMediaTreeItemModel, UmbMediaTreeItemContext> {
	#api: UmbMediaTreeItemContext | undefined;

	@property({ type: Object, attribute: false })
	public override get api(): UmbMediaTreeItemContext | undefined {
		return this.#api;
	}
	public override set api(value: UmbMediaTreeItemContext | undefined) {
		this.#api = value;

		if (this.#api) {
			this.observe(this.#api.noAccess, (noAccess) => (this._noAccess = noAccess || false));
		}

		super.api = value;
	}

	/**
	 * @internal
	 * Indicates whether the user has no access to this media item, this is controlled internally but present as an attribute as it affects styling.
	 */
	@property({ type: Boolean, reflect: true, attribute: 'no-access' })
	protected _noAccess = false;

	constructor() {
		super();
		this.addEventListener('click', this.#handleClick);
	}

	#handleClick = (event: MouseEvent) => {
		if (this._noAccess) {
			event.preventDefault();
			event.stopPropagation();
		}
	};

	override renderIconContainer() {
		const icon = this.item?.mediaType.icon;

		return html`
			<span id="icon-container" slot="icon">
				${icon
					? html`
							<umb-icon id="icon" slot="icon" name="${this._getIconToRender(icon)}"></umb-icon>
							${this.#renderStateIcon()}
						`
					: nothing}
			</span>
		`;
	}

	override renderLabel() {
		return html`<span id="label" slot="label">${this._item?.variants[0].name}</span> `;
	}

	#renderStateIcon() {
		if (this.item?.mediaType.collection) {
			return this.#renderIsCollectionIcon();
		}

		return nothing;
	}

	#renderIsCollectionIcon() {
		return html`<umb-icon id="state-icon" slot="icon" name="icon-grid" title="Collection"></umb-icon>`;
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

			/** No Access */
			:host([no-access]) {
				cursor: not-allowed;
			}
			:host([no-access]) #label {
				opacity: 0.6;
				font-style: italic;
			}
			:host([no-access]) umb-icon {
				opacity: 0.6;
			}
		`,
	];
}

export { UmbMediaTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTreeItemElement;
	}
}
