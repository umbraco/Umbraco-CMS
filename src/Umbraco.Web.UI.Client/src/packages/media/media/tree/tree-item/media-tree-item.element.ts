import type { UmbMediaTreeItemModel } from '../types.js';
import type { UmbMediaTreeItemContext } from './media-tree-item.context.js';
import { css, html, customElement, nothing, classMap, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-media-tree-item')
export class UmbMediaTreeItemElement extends UmbTreeItemElementBase<UmbMediaTreeItemModel, UmbMediaTreeItemContext> {
	public override set api(value: UmbMediaTreeItemContext | undefined) {
		// Observe noAccess from context and update base class property (_noAccess).
		// This enables access restriction behavior (click prevention) and styling from the base class.
		this.observe(value?.noAccess, (noAccess) => (this._noAccess = noAccess ?? false));
		super.api = value;
	}

	override renderIconContainer() {
		return html`
			<div id="icon-container" slot="icon">
				<umb-entity-sign-bundle .entityType=${this._item?.entityType} .entityFlags=${this._flags}>
					${when(
						this.item?.mediaType.icon,
						(icon) => html`<umb-icon id="icon" name=${this._getIconToRender(icon)}></umb-icon>`,
					)}
				</umb-entity-sign-bundle>
				${this.#renderStateIcon()}
			</div>
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
				background: var(--umb-sign-bundle-bg, var(--uui-color-surface));
				width: 14px;
				height: 14px;
				border-radius: 100%;
				line-height: 14px;
			}
		`,
	];
}

export { UmbMediaTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-tree-item': UmbMediaTreeItemElement;
	}
}
