import type { UmbMediaTreeItemModel } from '../types.js';
import { css, html, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-media-tree-item';
@customElement(elementName)
export class UmbMediaTreeItemElement extends UmbTreeItemElementBase<UmbMediaTreeItemModel> {
	override renderIconContainer() {
		const icon = this.item?.mediaType.icon;
		const iconWithoutColor = icon?.split(' ')[0];

		return html`
			<span id="icon-container" slot="icon">
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
		`,
	];
}

export { UmbMediaTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTreeItemElement;
	}
}
