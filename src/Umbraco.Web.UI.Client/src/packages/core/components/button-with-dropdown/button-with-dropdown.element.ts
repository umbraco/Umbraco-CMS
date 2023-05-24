import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, customElement, property, query } from '@umbraco-cms/backoffice/external/lit';
import { PopoverPlacement, UUIPopoverElement, UUISymbolExpandElement } from '@umbraco-ui/uui';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';

// TODO: maybe this should go to UI library? It's a common pattern
// TODO: consider not using this, but instead use dropdown, which is more generic shared component of backoffice. (this is at the movement only used in Log Viewer)
@customElement('umb-button-with-dropdown')
export class UmbButtonWithDropdownElement extends LitElement {
	@property()
	label = '';

	@property()
	open = false;

	@property()
	look: InterfaceLook = 'default';

	@property()
	color: InterfaceColor = 'default';

	@property()
	placement: PopoverPlacement = 'bottom-start';

	@property({ type: Boolean })
	compact = false;

	@query('#symbol-expand')
	symbolExpand!: UUISymbolExpandElement;

	@query('#popover')
	popover!: UUIPopoverElement;

	#openPopover() {
		this.open = true;
		this.popover.open = true;
		this.symbolExpand.open = true;
	}

	#closePopover() {
		this.open = false;
		this.popover.open = false;
		this.symbolExpand.open = false;
	}

	#togglePopover() {
		this.open ? this.#closePopover() : this.#openPopover();
	}

	render() {
		return html`
			<uui-popover placement=${this.placement} id="popover" @close=${this.#closePopover}>
				<uui-button
					slot="trigger"
					.look=${this.look}
					.color=${this.color}
					.label=${this.label}
					.compact=${this.compact}
					id="myPopoverBtn"
					@click=${this.#togglePopover}>
					<slot></slot>
					<uui-symbol-expand id="symbol-expand" .open=${this.open}></uui-symbol-expand>
				</uui-button>
				<div slot="popover">
					<slot name="dropdown"></slot>
				</div>
			</uui-popover>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-symbol-expand {
				margin-left: var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-button-with-dropdown': UmbButtonWithDropdownElement;
	}
}
