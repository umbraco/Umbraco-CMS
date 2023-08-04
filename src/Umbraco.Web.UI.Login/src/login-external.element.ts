import {css, CSSResultGroup, html, LitElement, nothing} from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui';

@customElement('umb-login-external')
export class UmbLoginExternalElement extends LitElement {
	@property({ attribute: 'custom-view' })
	customView?: string;

	@property({ attribute: 'name' })
	name = '';

	@property({ attribute: 'external-login-url' })
	externalLoginUrl = '';

	@property({ attribute: 'icon' })
	icon = 'icon-lock';

	@property({ attribute: 'button-look' })
	buttonLook: InterfaceLook = 'outline';

	@property({ attribute: 'button-color' })
	buttonColor: InterfaceColor = 'default';

	@state()
	protected externalComponent: HTMLElement | null = null;

	@state()
	protected loading = false;

	async connectedCallback() {
		super.connectedCallback();
		if (this.customView) {
			this.loading = true;
      await this.loadCustomView();
      this.loading = false;
		}
	}

	render() {
		return this.loading
			? html`<uui-button state="waiting" disabled label="Loading provider"></uui-button>`
			: this.externalComponent ?? this.renderDefaultView();
	}

	protected renderDefaultView() {
		return html`
			<form id="defaultView" method="post" action="${this.externalLoginUrl}">
				<uui-button label="continue with ${this.name}" .look=${this.buttonLook} .color=${this.buttonColor}>
					<div><uui-icon name=${this.icon}></uui-icon> Continue with ${this.name}</div>
				</uui-button>
			</form>
		`;
	}

	protected async loadCustomView() {
    try {
      if (!this.customView) return;
      const customViewModule = await import(this.customView /* @vite-ignore */);

      if (!customViewModule.default) throw new Error(`Custom view ${this.customView} does not export a default`);

      const customView = customViewModule.default;
      this.externalComponent = new customView();
    } catch (error: unknown) {
      this.externalComponent = nothing;
      console.group('[External login] Failed to load');
      console.log('Provider name', this.name);
      console.log('Element reference', this);
      console.log('Custom view', this.customView);
      console.error('Failed to load custom view:', error);
      console.groupEnd();
    }
	}

	static styles: CSSResultGroup = [
		css`
			#defaultView uui-button {
				width: 100%;
				--uui-button-padding-top-factor: 1.5;
				--uui-button-padding-bottom-factor: 1.5;
			}
      #defaultView uui-button div {
				/* TODO: Remove this when uui-button has setting for aligning content */
				position: absolute;
				left: 9px;
				margin: auto;
				text-align: left;
				top: 50%;
				transform: translateY(-50%);
			}
      #defaultView button {
				font-size: var(--uui-button-font-size);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				padding: 9px;
				text-align: left;
				background-color: var(--uui-color-surface);
				cursor: pointer;
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
				box-sizing: border-box;

				line-height: 1.1; /* makes the text vertically centered */
				color: var(--uui-color-interactive);
			}

      #defaultView button:hover {
				color: var(--uui-color-interactive-emphasis);
				border-color: var(--uui-color-border-standalone);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-login-external': UmbLoginExternalElement;
	}
}
