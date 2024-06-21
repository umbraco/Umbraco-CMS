import type { UmbMediaTreeItemModel } from '../types.js';
import { css, html, customElement, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-media-tree-item';
@customElement(elementName)
export class UmbMediaTreeItemElement extends UmbTreeItemElementBase<UmbMediaTreeItemModel> {
	override renderIconContainer() {
		return html`
			<span id="icon-container" slot="icon">
				${this.item?.mediaType.icon
					? html` <umb-icon id="icon" slot="icon" name="${this.item.mediaType.icon}"></umb-icon> `
					: nothing}
			</span>
		`;
	}

	override renderLabel() {
		return html`<span id="label" slot="label">${this._item?.variants[0].name}</span> `;
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
		`,
	];
}

export { UmbMediaTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMediaTreeItemElement;
	}
}
