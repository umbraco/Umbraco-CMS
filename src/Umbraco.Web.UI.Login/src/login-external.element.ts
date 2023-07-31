import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

@customElement('umb-login-external')
export class UmbLoginExternalElement extends LitElement {
	@property({ attribute: 'custom-view' })
	customView?: string;

	@property({ attribute: 'name' })
	name = '';

	@property({ attribute: 'external-login-url' })
	externalLoginUrl = '';

	@property({ attribute: 'icon' })
	icon = '';

  @state()
  protected externalComponent: HTMLElement | null = null;

  @state()
  protected loading = false;

  connectedCallback() {
    super.connectedCallback();
    if (this.customView) {
      this.loading = true;
    }
  }

  async firstUpdated() {
    await this.loadCustomView();
    this.loading = false;
  }

  render() {
		return this.loading ? html`<uui-button state="waiting" disabled label="Loading provider"></uui-button>` : (this.externalComponent ?? this.renderDefaultView());
	}

  protected renderDefaultView() {
    return html`
        <form method="post" action="${this.externalLoginUrl}">
					<uui-button><uui-icon name=${this.icon}></uui-icon> Continue with ${this.name}</uui-button>
				</form>
    `;
  }

  protected async loadCustomView() {
    if (!this.customView) return;
    const customViewModule = await import(this.customView);
    const customView = customViewModule.default;
    this.externalComponent = new customView();
  }

	static styles: CSSResultGroup = [
		css`
			button {
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

			button:hover {
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
