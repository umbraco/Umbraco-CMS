import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UUISelectEvent } from '@umbraco-ui/uui';
import { UmbThemeService, UMB_THEME_SERVICE_CONTEXT_TOKEN } from '../../themes/theme.service';
import { UmbLitElement } from '@umbraco-cms/element';

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
				background: var(--uui-color-surface);
				color: var(--uui-color-text);
				border-radius: var(--uui-border-radius);
			}
		`,
	];

	#themeService?: UmbThemeService;

	@state()
	private _theme = '';

	@state()
	private _themes: Array<string> = [];

	constructor() {
		super();
		this.consumeContext(UMB_THEME_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#themeService = instance;
			instance.theme.subscribe((theme) => {
				this._theme = theme;
			});
			// TODO: We should get rid of the #themes state and instead use an extension point:
			instance.themes.subscribe((themes) => {
				this._themes = themes.map((t) => t.name);
			});
		});
	}

	private _handleThemeChange(event: UUISelectEvent) {
		if (!this.#themeService) return;

		const theme = event.target.value.toString();

		this.#themeService.changeTheme(theme);
	}

	get options() {
		return this._themes.map((t) => ({ name: t, value: t, selected: t === this._theme }));
	}

	render() {
		return html`
			<b>Select Theme</b>
			<uui-select
				label="theme select"
				@change=${this._handleThemeChange}
				.value=${this._theme}
				.options=${this.options}></uui-select>
		`;
	}
}

export default UmbUserDashboardTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-dashboard-test': UmbUserDashboardTestElement;
	}
}
