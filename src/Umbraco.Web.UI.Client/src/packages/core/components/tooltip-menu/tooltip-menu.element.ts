import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, LitElement, nothing, repeat, customElement, property } from '@umbraco-cms/backoffice/external/lit';

export interface TooltipMenuItem {
	label: string;
	icon?: string;
	action: () => void;
}

@customElement('umb-tooltip-menu')
export class UmbTooltipMenuElement extends LitElement {
	@property({ type: Boolean, reflect: true, attribute: 'icon-only' })
	public iconOnly = false;

	@property()
	public items: Array<TooltipMenuItem> = [];

	private _handleItemClick(item: TooltipMenuItem) {
		item.action();
	}

	private _handleItemKeyDown(event: KeyboardEvent, item: TooltipMenuItem) {
		if (event.key === 'Enter') {
			item.action();
		}
	}

	private _renderItem(item: TooltipMenuItem) {
		if (this.iconOnly && item.icon) {
			return html`<div
				@click=${() => this._handleItemClick(item)}
				@keydown=${(e: KeyboardEvent) => this._handleItemKeyDown(e, item)}
				class="item icon">
				<uui-icon .name=${item.icon}></uui-icon>
			</div>`;
		}

		return html`<div
			@click=${() => this._handleItemClick(item)}
			@keydown=${(e: KeyboardEvent) => this._handleItemKeyDown(e, item)}
			class="item ${this.iconOnly ? 'icon' : 'label'}">
			${item.icon ? html`<uui-icon .name=${item.icon}></uui-icon>` : nothing}
			${!this.iconOnly ? html`<span>${item.label}</span>` : nothing}
		</div>`;
	}

	render() {
		return repeat(
			this.items,
			(item) => item.label,
			(item) => this._renderItem(item)
		);
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				width: max-content;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				border-radius: var(--uui-border-radius);
				overflow: clip;
			}
			.item {
				color: var(--uui-color-interactive);
				align-items: center;
				display: flex;
				gap: var(--uui-size-space-2);
				cursor: pointer;
			}
			.item:hover {
				color: var(--uui-color-interactive-emphasis);
				background-color: var(--uui-color-surface-emphasis);
			}
			.item.label {
				padding: var(--uui-size-space-2) var(--uui-size-space-4);
			}
			.item.icon {
				padding: var(--uui-size-space-4);
				aspect-ratio: 1/1;
				justify-content: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tooltip-menu': UmbTooltipMenuElement;
	}
}
