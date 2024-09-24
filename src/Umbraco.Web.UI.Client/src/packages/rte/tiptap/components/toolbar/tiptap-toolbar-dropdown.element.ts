import { css, customElement, html, nothing, repeat, type TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import type { PopoverContainerPlacement, UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-tiptap-toolbar-dropdown';

type DropdownItem = {
	label: string;
	nested?: DropdownItem[];
};

@customElement(elementName)
export class UmbTiptapToolbarDropdownElement extends UmbLitElement {
	#testDropdownItems: Array<DropdownItem> = [
		{
			label: 'Headings',
			nested: [
				{
					label: 'Page header',
				},
				{
					label: 'Section header',
				},
				{
					label: 'Paragraph header',
					nested: [
						{
							label: 'Paragraph header 1',
						},
						{
							label: 'Paragraph header 2',
						},
					],
				},
			],
		},
		{
			label: 'Blocks',
			nested: [
				{
					label: 'Paragraph',
				},
			],
		},
		{
			label: 'Containers',
			nested: [
				{
					label: 'Quote',
				},
				{
					label: 'Code',
				},
			],
		},
	];

	#onMouseEnter = (popoverId: string) => {
		const popover = this.shadowRoot?.querySelector(`#${this.#makeAlias(popoverId)}`) as UUIPopoverContainerElement;
		if (!popover) return;
		popover.showPopover();
	};

	#onMouseLeave = (popoverId: string) => {
		popoverId = popoverId.replace(/\s/g, '-').toLowerCase();
		const popover = this.shadowRoot?.querySelector(`#${this.#makeAlias(popoverId)}`) as UUIPopoverContainerElement;
		if (!popover) return;
		popover.hidePopover();
	};

	#makeAlias(label: string) {
		return label.replace(/\s/g, '-').toLowerCase();
	}

	#renderItem(item: DropdownItem): TemplateResult {
		return html`
			<div
				class="dropdown-item"
				@mouseenter=${() => this.#onMouseEnter(item.label)}
				@mouseleave=${() => this.#onMouseLeave(item.label)}>
				<button class="label" aria-label=${item.label} popovertarget=${this.#makeAlias(item.label)}>
					${item.label}${item.nested ? html`<uui-symbol-expand></uui-symbol-expand>` : nothing}
				</button>
				${item.nested ? this.#renderItems(item.label, item.nested) : nothing}
			</div>
		`;
	}

	#renderItems(
		label: string,
		items: Array<DropdownItem>,
		placement: PopoverContainerPlacement = 'right-start',
	): TemplateResult {
		return html` <uui-popover-container placement=${placement} id=${this.#makeAlias(label)}>
			<div class="popover-content">
				${repeat(
					items,
					(item) => item.label,
					(item) => html`${this.#renderItem(item)}`,
				)}
			</div>
		</uui-popover-container>`;
	}

	override render() {
		return html`
			<button class="label selected-value" aria-label="Text styles" popovertarget="text-styles">
				Text styles<uui-symbol-expand style="top: 4px;" ?open=${true}></uui-symbol-expand>
			</button>
			${this.#renderItems('Text styles', this.#testDropdownItems, 'bottom-start')}
		`;
	}

	static override readonly styles = css`
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
	`;
}

export { UmbTiptapToolbarDropdownElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbTiptapToolbarDropdownElement;
	}
}
