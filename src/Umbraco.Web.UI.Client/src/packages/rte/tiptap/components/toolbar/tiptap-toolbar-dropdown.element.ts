import type { ManifestTiptapExtensionButtonKind } from '../../extensions/tiptap-extension.js';
import type { UmbTiptapToolbarElementApi } from '../../extensions/types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	repeat,
	state,
	when,
	type TemplateResult,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIPopoverContainerElement } from '@umbraco-ui/uui';

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
				<uui-button label=${item.label} popovertarget=${this.#makeAlias(item.label)}></uui-button>
				<uui-popover-container placement="right-start" id=${this.#makeAlias(item.label)}>
					${item.nested ? this.#renderItems(item.nested) : nothing}
				</uui-popover-container>
			</div>
		`;
	}

	#renderItems(items: Array<DropdownItem>) {
		return html` ${repeat(
			items,
			(item) => item.label,
			(item) => html`${this.#renderItem(item)}`,
		)}`;
	}

	override render() {
		return html`
			<uui-button popovertarget="text-styles">Text styles</uui-button>
			<uui-popover-container id="text-styles"
				><div class="dropdown-item">${this.#renderItems(this.#testDropdownItems)}</div></uui-popover-container
			>
		`;
	}

	static override readonly styles = css`
		:host {
			position: absolute;
			top: -67px;
			--uui-button-content-align: left;
		}

		uui-popover-container {
			box-shadow: var(--uui-shadow-depth-3);
		}

		.dropdown-item {
			background-color: var(--uui-color-surface);
			display: flex;
			flex-direction: column;
			text-wrap: nowrap;
		}
	`;
}

export { UmbTiptapToolbarDropdownElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbTiptapToolbarDropdownElement;
	}
}
