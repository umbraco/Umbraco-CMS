import { css, html, PropertyValueMap } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbThemeService, UMB_THEME_SERVICE_CONTEXT_TOKEN } from 'src/core/theme/theme.service';

@customElement('umb-user-dashboard-test')
export class UmbUserDashboardTestElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-5);
				border: 1px solid var(--uui-color-border);
				background: var(--uui-color-positive);
				color: var(--uui-color-positive-contrast);
				border-radius: var(--uui-border-radius);
			}
			p {
				margin: 0;
			}
		`,
	];

	#themeService?: UmbThemeService;

	@state()
	private _theme = '';

	constructor() {
		super();
		this.consumeContext(UMB_THEME_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#themeService = instance;
			instance.theme.subscribe((theme) => {
				this._theme = theme;
			});
		});
	}

	private _toggleTheme() {
		if (!this.#themeService) return;
		console.log('toggle', this._theme);

		this.#themeService.changeTheme(this._theme !== 'dark' ? 'dark' : 'light');
	}

	render() {
		return html`
			<b>Dark theme toggle</b>
			<uui-button @click=${this._toggleTheme}>Toggle</uui-button>
		`;
	}
}

export default UmbUserDashboardTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-dashboard-test': UmbUserDashboardTestElement;
	}
}
