import { UMB_PREVIEW_CONTEXT } from '../context/preview.context-token.js';
import type { UmbPopoverToggleEvent } from './types.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	property,
	query,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

export interface UmbPreviewDevice {
	alias: string;
	label: string;
	icon: string;
	iconClass?: string;
	dimensions: { height: string; width: string };
}

@customElement('umb-preview-device')
export class UmbPreviewDeviceElement extends UmbLitElement {
	@query('#devices-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	// TODO: [LK] Eventually, convert these devices to be an  extension point.
	#devices: Array<UmbPreviewDevice> = [
		{
			alias: 'fullsize',
			label: 'Fit browser',
			icon: 'icon-application-window-alt',
			dimensions: { height: '100%', width: '100%' },
		},
		{
			alias: 'desktop',
			label: 'Desktop',
			icon: 'icon-display',
			dimensions: { height: '1080px', width: '1920px' },
		},
		{
			alias: 'laptop',
			label: 'Laptop',
			icon: 'icon-laptop',
			dimensions: { height: '768px', width: '1366px' },
		},
		{
			alias: 'ipad-portrait',
			label: 'Tablet portrait',
			icon: 'icon-ipad',
			dimensions: { height: '929px', width: '769px' },
		},
		{
			alias: 'ipad-landscape',
			label: 'Tablet landscape',
			icon: 'icon-ipad',
			iconClass: 'flip',
			dimensions: { height: '675px', width: '1024px' },
		},
		{
			alias: 'smartphone-portrait',
			label: 'Smartphone portrait',
			icon: 'icon-iphone',
			dimensions: { height: '640px', width: '360px' },
		},
		{
			alias: 'smartphone-landscape',
			label: 'Smartphone landscape',
			icon: 'icon-iphone',
			iconClass: 'flip',
			dimensions: { height: '360px', width: '640px' },
		},
	];

	@property({ attribute: false, type: Object })
	device = this.#devices[0];

	@state()
	private _popoverOpen = false;

	constructor() {
		super();
		this.addEventListener('blur', this.#onBlur, true); // Use capture phase to catch blur events
	}

	override connectedCallback() {
		super.connectedCallback();
		this.hidden = true;
		this.#loadDevices();
	}

	async #loadDevices() {
		// TODO: [LK] Eventually, load devices from extension points.
		this.hidden = false;
	}

	async #onClick(device: UmbPreviewDevice) {
		if (device === this.device) return;

		this.device = device;

		const previewContext = await this.getContext(UMB_PREVIEW_CONTEXT);

		await previewContext?.updateIFrame({
			wrapperClass: device.alias,
			height: device.dimensions.height,
			width: device.dimensions.width,
		});

		// Don't close popover for device selector - users often want to quickly test multiple devices
	}

	#onPopoverToggle(event: UmbPopoverToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	#onBlur = () => {
		if (this._popoverOpen) {
			this._popoverElement?.hidePopover();
		}
	};

	override render() {
		return html`
			<uui-button look="primary" popovertarget="devices-popover">
				<div>
					<uui-icon name=${this.device.icon} class=${ifDefined(this.device.iconClass)}></uui-icon>
					<span>${this.device.label}</span>
				</div>
				<uui-symbol-expand slot="extra" id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container id="devices-popover" placement="top-end" @toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					${repeat(
						this.#devices,
						(item) => item.alias,
						(item) => html`
							<uui-menu-item label=${item.label} ?active=${item === this.device} @click=${() => this.#onClick(item)}>
								<uui-icon slot="icon" name=${item.icon} class=${ifDefined(item.iconClass)}></uui-icon>
							</uui-menu-item>
						`,
					)}
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				border-left: 1px solid var(--uui-color-header-contrast);
				--uui-button-font-weight: 400;
				--uui-button-padding-left-factor: 3;
				--uui-button-padding-right-factor: 3;
				--uui-menu-item-flat-structure: 1;
			}

			:host([hidden]) {
				display: none;
			}

			#expand-symbol {
				transform: rotate(-90deg);
				margin-left: var(--uui-size-space-3, 9px);

				&[open] {
					transform: rotate(0deg);
				}
			}

			uui-button > div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-2, 6px);
			}

			uui-icon.flip {
				transform: rotate(90deg);
			}

			umb-popover-layout {
				--uui-color-surface: var(--uui-color-header-surface);
				--uui-color-border: var(--uui-color-header-surface);
				color: var(--uui-color-header-contrast);
			}
		`,
	];
}

export { UmbPreviewDeviceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-preview-device': UmbPreviewDeviceElement;
	}
}
