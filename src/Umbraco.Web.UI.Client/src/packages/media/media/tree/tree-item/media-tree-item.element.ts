import type { UmbMediaTreeItemModel } from '../types.js';
import { css, html, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-media-tree-item';
@customElement(elementName)
export class UmbMediaTreeItemElement extends UmbTreeItemElementBase<UmbMediaTreeItemModel> {
	renderIconContainer() {
		return html`
			<span id="icon-container" slot="icon">
				${this.item?.mediaType.icon
					? html` <umb-icon id="icon" slot="icon" name="${this.item.mediaType.icon}"></umb-icon> `
					: nothing}
			</span>
		`;
	}

	renderLabel() {
		return html`<span id="label" slot="label">${this._item?.variants[0].name}</span> `;
	}

	static styles = [
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
		`,
	];
}

export { UmbMediaTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTreeItemElement;
	}
}
