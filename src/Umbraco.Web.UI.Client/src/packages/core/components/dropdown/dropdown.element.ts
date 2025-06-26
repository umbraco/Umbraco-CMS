import type {
	UUIInterfaceColor,
	UUIInterfaceLook,
	PopoverContainerPlacement,
	UUIPopoverContainerElement,
} from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, query, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbClosedEvent, UmbOpenedEvent } from '@umbraco-cms/backoffice/event';

// TODO: maybe move this to UI Library.
@customElement('umb-dropdown')
export class UmbDropdownElement extends UmbLitElement {
	#open = false;

	@property({ type: Boolean, reflect: true })
	public get open() {
		return this.#open;
	}
	public set open(value) {
		this.#open = value;

		if (value === true && this.popoverContainerElement) {
			this.openDropdown();
		} else {
			this.closeDropdown();
		}
	}

	@property()
	label?: string;

	@property()
	look: UUIInterfaceLook = 'default';

	@property()
	color: UUIInterfaceColor = 'default';

	@property()
	placement: PopoverContainerPlacement = 'bottom-start';

	@property({ type: Boolean })
	compact = false;

	@property({ type: Boolean, attribute: 'hide-expand' })
	hideExpand = false;

	@query('#dropdown-popover')
	popoverContainerElement?: UUIPopoverContainerElement;

	openDropdown() {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.popoverContainerElement?.showPopover();
		this.#open = true;
	}

	closeDropdown() {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.popoverContainerElement?.hidePopover();
		this.#open = false;
	}

	#onToggle(event: ToggleEvent) {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.open = event.newState === 'open';

		if (this.open) {
			this.dispatchEvent(new UmbOpenedEvent());
		} else {
			this.dispatchEvent(new UmbClosedEvent());
		}
	}

	override render() {
		return html`
			<uui-button
				id="dropdown-button"
				popovertarget="dropdown-popover"
				data-mark="open-dropdown"
				.look=${this.look}
				.color=${this.color}
				.label=${this.label ?? ''}
				.compact=${this.compact}>
				<slot name="label"></slot>
				${when(
					!this.hideExpand,
					() => html`<uui-symbol-expand id="symbol-expand" .open=${this.#open}></uui-symbol-expand>`,
				)}
			</uui-button>
			<uui-popover-container id="dropdown-popover" .placement=${this.placement} @toggle=${this.#onToggle}>
				<umb-popover-layout>
					<slot></slot>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#dropdown-button {
				min-width: max-content;
				height: 100%;
			}
			:host(:not([hide-expand]):not([compact])) #dropdown-button {
				--uui-button-padding-right-factor: 2;
			}

			:host(:not([compact])) #symbol-expand {
				margin-left: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropdown': UmbDropdownElement;
	}
}
