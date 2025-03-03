import { UMB_APP_CONTEXT } from './app.context.js';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-app-logo')
export class UmbAppLogoElement extends UmbLitElement {
	/**
	 * The loading attribute of the image.
	 * @type {'lazy' | 'eager'}
	 * @default 'eager'
	 */
	@property()
	loading: 'lazy' | 'eager' = 'lazy';

	/**
	 * The type of logo to display.
	 * @type {'mark' | 'logo'}
	 * @default 'mark'
	 */
	@property({ type: String, attribute: 'logo-type' })
	logoType: 'mark' | 'logo' = 'mark';

	/**
	 * Whether to use the alternative logo.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean })
	alternative: boolean = false;

	@state()
	private _serverUrl?: string;

	constructor() {
		super();

		this.consumeContext(UMB_APP_CONTEXT, (instance) => {
			this._serverUrl = instance.getServerUrl();
		});
	}

	/**
	 * Do not use shadow DOM for this element.
	 * @returns {this} The element instance.
	 */
	override createRenderRoot(): this {
		return this;
	}

	override render() {
		if (!this._serverUrl) {
			return nothing;
		}

		let logoFile = this.alternative ? 'logo-alternative' : 'logo';

		if (this.logoType === 'logo') {
			logoFile = `login-${logoFile}`;
		}

		const logoUrl = `${this._serverUrl}/umbraco/management/api/v1/security/back-office/graphics/${logoFile}`;

		return html`<img src=${logoUrl} aria-hidden="true" loading=${this.loading} alt="logo" />`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-logo': UmbAppLogoElement;
	}
}
