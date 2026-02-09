import type { UmbMediaTreeItemModel } from '../types.js';
import type { UmbMediaTreeItemContext } from './media-tree-item.context.js';
import { css, html, customElement, nothing, classMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-media-tree-item';
@customElement(elementName)
export class UmbMediaTreeItemElement extends UmbTreeItemElementBase<UmbMediaTreeItemModel, UmbMediaTreeItemContext> {
	public override set api(value: UmbMediaTreeItemContext | undefined) {
		// Observe noAccess from context and update base class property (_noAccess).
		// This enables access restriction behavior (click prevention) and styling from the base class.
		this.observe(value?.noAccess, (noAccess) => (this._noAccess = noAccess ?? false));
		super.api = value;
	}

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
		return html`<span id="label" slot="label" class=${classMap({ noAccess: this._noAccess })}>
			${this._item?.variants[0].name}
		</span> `;
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
		...UmbTreeItemElementBase.styles,
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
		`,
	];
}

export { UmbMediaTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTreeItemElement;
	}
}
