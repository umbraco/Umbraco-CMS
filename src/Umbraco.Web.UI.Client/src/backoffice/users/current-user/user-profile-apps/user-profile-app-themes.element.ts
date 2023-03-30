import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UUISelectEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestTheme } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbThemeContext, UMB_THEME_CONTEXT_TOKEN } from '../../../themes/theme.context';

@customElement('umb-user-profile-app-themes')
export class UmbUserProfileAppThemesElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-5);
				background: var(--uui-color-surface);
				color: var(--uui-color-text);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-1);
			}
		`,
	];

	#themeService?: UmbThemeContext;

	@state()
	private _themeAlias: string | null = null;

	@state()
	private _themes: Array<ManifestTheme> = [];

	constructor() {
		super();
		this.consumeContext(UMB_THEME_CONTEXT_TOKEN, (instance) => {
			this.#themeService = instance;
			instance.theme.subscribe((themeAlias) => {
				this._themeAlias = themeAlias;
			});

			umbExtensionsRegistry.extensionsOfType('theme').subscribe((themes) => {
				this._themes = themes;
			});
		});
	}

	#handleThemeChange(event: UUISelectEvent) {
		if (!this.#themeService) return;

		const theme = event.target.value.toString();

		this.#themeService.setThemeByAlias(theme);
	}

	get #options() {
		return this._themes.map((t) => ({ name: t.name, value: t.alias, selected: t.alias === this._themeAlias }));
	}

	render() {
		return html`
			<b>Select Theme</b>
			<uui-select
				label="theme select"
				@change=${this.#handleThemeChange}
				.value=${this._themeAlias}
				.options=${this.#options}></uui-select>
		`;
	}
}

export default UmbUserProfileAppThemesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-profile-app-themes': UmbUserProfileAppThemesElement;
	}
}
