import { css, html, nothing, repeat, type TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import type { PopoverContainerPlacement, UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

export type TiptapDropdownItem = {
	alias: string;
	label: string;
	nested?: TiptapDropdownItem[];
	execute?: () => void;
	isActive?: () => boolean;
};

export abstract class UmbTiptapToolbarDropdownBaseElement extends UmbLitElement {
	protected abstract get items(): TiptapDropdownItem[];
	protected abstract get label(): string;

	readonly #onMouseEnter = (popoverId: string) => {
		const popover = this.shadowRoot?.querySelector(`#${this.makeAlias(popoverId)}`) as UUIPopoverContainerElement;
		if (!popover) return;
		popover.showPopover();
	};

	readonly #onMouseLeave = (popoverId: string) => {
		popoverId = popoverId.replace(/\s/g, '-').toLowerCase();
		const popover = this.shadowRoot?.querySelector(`#${this.makeAlias(popoverId)}`) as UUIPopoverContainerElement;
		if (!popover) return;
		popover.hidePopover();
	};

	protected makeAlias(label: string) {
		return label.replace(/\s/g, '-').toLowerCase();
	}

	protected renderItem(item: TiptapDropdownItem): TemplateResult {
		return html`
			<div
				class="dropdown-item"
				@mouseenter=${() => this.#onMouseEnter(item.label)}
				@mouseleave=${() => this.#onMouseLeave(item.label)}>
				<button class="label" aria-label=${item.label} popovertarget=${this.makeAlias(item.label)}>
					${item.label}${item.nested ? html`<uui-symbol-expand></uui-symbol-expand>` : nothing}
				</button>
				${item.nested ? this.renderItems(item.label, item.nested) : nothing}
			</div>
		`;
	}

	protected renderItems(
		label: string,
		items: Array<TiptapDropdownItem>,
		placement: PopoverContainerPlacement = 'right-start',
	): TemplateResult {
		return html` <uui-popover-container placement=${placement} id=${this.makeAlias(label)}>
			<div class="popover-content">
				${repeat(
					items,
					(item) => item.alias,
					(item) => html`${this.renderItem(item)}`,
				)}
			</div>
		</uui-popover-container>`;
	}
	protected override render() {
		return html`
			<button class="label selected-value" aria-label="Text styles" popovertarget="text-styles">
				<umb-localize .key=${this.label}></umb-localize>
				<uui-symbol-expand aria-hidden="true" style="top: 4px;" ?open=${true}></uui-symbol-expand>
			</button>
			${this.renderItems(this.label, this.items, 'bottom-start')}
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			button {
				border: unset;
				background-color: unset;
				font: unset;
				text-align: unset;
			}

			uui-symbol-expand {
				position: absolute;
				right: 5px;
				top: 5px;
			}

			.label {
				border-radius: var(--uui-border-radius);
				width: 100%;
				box-sizing: border-box;
				align-content: center;
				padding: var(--uui-size-space-1) var(--uui-size-space-3);
				padding-right: 21px;
				align-items: center;
				cursor: pointer;
				color: var(--uui-color-text);
				position: relative;
			}

			.label:hover {
				background: var(--uui-color-surface-alt);
				color: var(--uui-color-interactive-emphasis);
			}

			.selected-value {
				background: var(--uui-color-surface-alt);
			}

			.popover-content {
				background: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				padding: var(--uui-size-space-1);
			}
		`,
	];
}
