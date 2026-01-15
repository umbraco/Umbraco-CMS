import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { UMB_THEME_CONTEXT } from '@umbraco-cms/backoffice/themes';

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
	 * The type of logo to display. Mark will display the mark logo, and logo will display the full logo with text.
	 * @type {'mark' | 'logo'}
	 * @default 'mark'
	 */
	@property({ type: String, attribute: 'logo-type' })
	logoType: 'mark' | 'logo' = 'mark';

	/**
	 * Override the application theme, for example if you want to display the dark theme logo on a light theme.
	 * @example 'umb-dark-theme'
	 * @type {string}
	 * @default undefined
	 */
	@property({ attribute: 'override-theme' })
	overrideTheme?: string;

	@state()
	private _serverUrl?: string;

	/**
	 * The theme of the application.
	 */
	@state()
	private _theme?: string;

	constructor() {
		super();

		this.consumeContext(UMB_SERVER_CONTEXT, (instance) => {
			this._serverUrl = instance?.getServerUrl();
		});

		this.consumeContext(UMB_THEME_CONTEXT, (context) => {
			this.observe(
				context?.theme,
				(theme) => {
					this._theme = theme;
				},
				'_observeTheme',
			);
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

		/**
		 * This is a temporary solution until we have a better way to define the logo characteristics.
		 * TODO: The characteristics of the logo are not defined in any theme meta data, so we have to hardcode the logo file names.
		 */
		let logoFile = (this.overrideTheme ?? this._theme) === 'umb-dark-theme' ? 'logo' : 'logo-alternative';

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
