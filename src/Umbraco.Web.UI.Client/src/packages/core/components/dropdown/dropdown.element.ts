import type {
	UUIInterfaceColor,
	UUIInterfaceLook,
	PopoverContainerPlacement,
	UUIPopoverContainerElement,
} from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, query, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// TODO: maybe move this to UI Library.
@customElement('umb-dropdown')
export class UmbDropdownElement extends UmbLitElement {
	@query('#dropdown-popover')
	popoverContainerElement?: UUIPopoverContainerElement;
	@property({ type: Boolean, reflect: true })
	open = false;

	@property()
	label = '';

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

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('open') && this.popoverContainerElement) {
			if (this.open) {
				// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				this.popoverContainerElement.showPopover();
			} else {
				// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				this.popoverContainerElement.hidePopover();
			}
		}
	}

	#onToggle(event: ToggleEvent) {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.open = event.newState === 'open';
	}

	override render() {
		return html`
			<uui-button
				id="dropdown-button"
				popovertarget="dropdown-popover"
				.look=${this.look}
				.color=${this.color}
				.label=${this.label}
				.compact=${this.compact}>
				<slot name="label"></slot>
				${when(
					!this.hideExpand,
					() => html`<uui-symbol-expand id="symbol-expand" .open=${this.open}></uui-symbol-expand>`,
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
