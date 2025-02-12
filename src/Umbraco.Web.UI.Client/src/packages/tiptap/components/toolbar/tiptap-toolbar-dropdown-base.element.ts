import { css, html, nothing, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PopoverContainerPlacement, UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';

export type UmbTiptapToolbarDropdownItem = {
	alias: string;
	label: string;
	icon?: string;
	items?: Array<UmbTiptapToolbarDropdownItem>;
	execute?: () => void;
	isActive?: () => boolean;
};

export abstract class UmbTiptapToolbarDropdownBaseElement extends UmbLitElement {
	protected abstract get alias(): string;

	protected abstract get items(): Array<UmbTiptapToolbarDropdownItem>;

	protected abstract get label(): string;

	readonly #onMouseEnter = (popoverId: string) => {
		const popover = this.shadowRoot?.querySelector(`#${popoverId}`) as UUIPopoverContainerElement;
		if (!popover) return;
		popover.showPopover();
	};

	readonly #onMouseLeave = (popoverId: string) => {
		const popover = this.shadowRoot?.querySelector(`#${popoverId}`) as UUIPopoverContainerElement;
		if (!popover) return;
		popover.hidePopover();
	};

	readonly #onClick = (item: UmbTiptapToolbarDropdownItem) => {
		item.execute?.();
		this.#onMouseLeave(item.alias);
	};

	protected renderItem(item: UmbTiptapToolbarDropdownItem): TemplateResult {
		return html`
			<div @mouseenter=${() => this.#onMouseEnter(item.alias)} @mouseleave=${() => this.#onMouseLeave(item.alias)}>
				<uui-menu-item popovertarget=${item.alias} @click-label=${() => this.#onClick(item)}>
					${when(item.icon, (icon) => html`<uui-icon slot="icon" name=${icon}></uui-icon>`)}
					<div slot="label" class="menu-item">
						<span>${item.label}</span>
						${when(item.items, () => html`<uui-symbol-expand></uui-symbol-expand>`)}
					</div>
				</uui-menu-item>
				${this.renderItems(item)}
			</div>
		`;
	}

	protected renderItems(item: UmbTiptapToolbarDropdownItem, placement: PopoverContainerPlacement = 'right-start') {
		if (!item.items) return nothing;
		return html`
			<uui-popover-container placement=${placement} id=${item.alias}>
				${repeat(
					item.items,
					(item) => item.alias,
					(item) => this.renderItem(item),
				)}
			</uui-popover-container>
		`;
	}

	protected override render() {
		return html`
			<uui-button compact look="secondary" popovertarget=${this.alias}>
				<span>${this.label}</span>
				<uui-symbol-expand slot="extra" open></uui-symbol-expand>
			</uui-button>
			${this.renderItems({ alias: this.alias, label: this.label, items: this.items }, 'bottom-start')}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
				--uui-button-font-weight: normal;
			}

			uui-button > uui-symbol-expand {
				margin-left: var(--uui-size-space-4);
			}

			uui-popover-container {
				background: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				padding: var(--uui-size-space-1);
			}

			.menu-item {
				flex: 1;
				display: flex;
				justify-content: space-between;
				align-items: center;
				gap: var(--uui-size-space-4);
			}
		`,
	];
}
