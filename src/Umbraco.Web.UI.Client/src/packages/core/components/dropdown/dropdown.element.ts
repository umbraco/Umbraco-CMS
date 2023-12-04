import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	css,
	html,
	nothing,
	customElement,
	property,
	PropertyValueMap,
	query,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { InterfaceColor, InterfaceLook, PopoverContainerPlacement, UUIPopoverContainerElement } from '@umbraco-ui/uui';

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
	look: InterfaceLook = 'default';

	@property()
	color: InterfaceColor = 'default';

	@property()
	placement: PopoverContainerPlacement = 'bottom-start';

	@property({ type: Boolean })
	compact = false;

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('open') && this.popoverContainerElement) {
			this.open ? this.popoverContainerElement.showPopover() : this.popoverContainerElement.hidePopover();
		}
	}

	render() {
		return html`
			<uui-button
				popovertarget="dropdown-popover"
				.look=${this.look}
				.color=${this.color}
				.label=${this.label}
				.compact=${this.compact}>
				<slot name="label"></slot>
			</uui-button>
			<uui-popover-container id="dropdown-popover" .placement=${this.placement}>
				<umb-popover-layout>
					<slot></slot>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropdown': UmbDropdownElement;
	}
}
