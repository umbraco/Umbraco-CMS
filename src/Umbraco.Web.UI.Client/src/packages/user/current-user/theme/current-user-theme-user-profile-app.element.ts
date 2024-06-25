import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_THEME_CONTEXT } from '@umbraco-cms/backoffice/themes';
import type { UmbThemeContext } from '@umbraco-cms/backoffice/themes';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-current-user-theme-user-profile-app')
export class UmbCurrentUserThemeUserProfileAppElement extends UmbLitElement {
	#themeContext?: UmbThemeContext;

	@state()
	private _themeAlias: string | null = null;

	@state()
	private _themes: Array<Option> = [];

	constructor() {
		super();
		this.consumeContext(UMB_THEME_CONTEXT, (context) => {
			this.#themeContext = context;
			this.observe(
				context.theme,
				(themeAlias) => {
					this._themeAlias = themeAlias;
				},
				'_observeCurrentTheme',
			);

			this.observe(
				umbExtensionsRegistry.byType('theme'),
				(themes) => {
					this._themes = themes.map((t) => ({ name: t.name, value: t.alias, selected: t.alias === this._themeAlias }));
				},
				'_observeThemeExtensions',
			);
		});
	}

	#onThemeChange(event: UUISelectEvent) {
		if (!this.#themeContext) return;

		const theme = event.target.value.toString();

		this.#themeContext.setThemeByAlias(theme);
	}

	override render() {
		if (!this._themes.length) return nothing;
		return html`
			<uui-box headline="Theme">
				<uui-tag slot="headline" look="placeholder">Experimental</uui-tag>
				<uui-select label="Select theme" .options=${this._themes} @change=${this.#onThemeChange}></uui-select>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			uui-select {
				width: 100%;
			}
		`,
	];
}

export default UmbCurrentUserThemeUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-theme-user-profile-app': UmbCurrentUserThemeUserProfileAppElement;
	}
}
