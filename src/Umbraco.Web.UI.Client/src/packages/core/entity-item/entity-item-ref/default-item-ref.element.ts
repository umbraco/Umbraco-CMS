import type { UmbItemModel } from '../types.js';
import { getItemFallbackIcon, getItemFallbackName } from '../utils.js';
import { css, customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWithOptionalDescriptionModel } from '@umbraco-cms/backoffice/models';

type UmbDefaultItemRefItemModel = UmbItemModel & UmbWithOptionalDescriptionModel;

@customElement('umb-default-item-ref')
export class UmbDefaultItemRefElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDefaultItemRefItemModel;

	@property({ type: Boolean })
	standalone = false;

	@property({ type: Boolean })
	selectable = false;

	override render() {
		if (!this.item) return nothing;

		const name = this.item.name ?? getItemFallbackName(this.item);

		return html`
			<uui-ref-node
				name=${name}
				detail=${ifDefined(this.item.description ?? undefined)}
				?standalone=${this.standalone}
				?selectable=${this.selectable}
				readonly>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)}
			</uui-ref-node>
			<umb-entity-frame><uui-icon name="link"></uui-icon> ${name}</umb-entity-frame>
		`;
	}

	#renderIcon(item: UmbDefaultItemRefItemModel) {
		const icon = item.icon || getItemFallbackIcon();
		return html`<umb-icon slot="icon" name=${icon}></umb-icon>`;
	}

	static override styles = [
		css`
			:host {
				--umb-entity-frame-opacity: 0;
				--umb-entity-frame-color: var(--umb-color-reference);
				--umb-entity-frame-contrast-color: var(--umb-color-reference-contrast);

				display: block;
				position: relative;
			}

			:host(:hover),
			:host(:focus-within) {
				--umb-entity-frame-opacity: 1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-item-ref': UmbDefaultItemRefElement;
	}
}
