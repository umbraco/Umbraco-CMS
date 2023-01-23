import { css, html, PropertyValueMap } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
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
				background: var(--uui-color-positive);
				color: var(--uui-color-positive-contrast);
				border-radius: var(--uui-border-radius);
			}
			p {
				margin: 0;
			}
		`,
	];

	private _darkTheme = css`
		:root {
			--uui-color-selected: #4e78cc;
			--uui-color-selected-emphasis: #4e78cc;
			--uui-color-selected-standalone: #4e78cc;
			--uui-color-selected-contrast: #fff;
			--uui-color-current: #f5c1bc;
			--uui-color-current-emphasis: #f8d6d3;
			--uui-color-current-standalone: #e8c0bd;
			--uui-color-current-contrast: #f000;
			--uui-color-disabled: purple;
			--uui-color-disabled-standalone: purple;
			--uui-color-disabled-contrast: #fcfcfc4d;
			--uui-color-header-surface: #1e2228;
			--uui-color-header-contrast: #ffffffcc;
			--uui-color-header-contrast-emphasis: #fff;
			--uui-color-focus: #4e78cc;
			--uui-color-surface: #23272e;
			--uui-color-surface-alt: #2c313c;
			--uui-color-surface-emphasis: #2c313c;
			--uui-color-background: #1e2228;
			--uui-color-text: #eeeeef;
			--uui-color-text-alt: #eeeeef;
			--uui-color-interactive: #eeeeef;
			--uui-color-interactive-emphasis: #eeeeef;
			--uui-color-border: #41414c;
			--uui-color-border-standalone: #4a4a55;
			--uui-color-border-emphasis: #484853;
			--uui-color-divider: #41414c;
			--uui-color-divider-standalone: #41414c;
			--uui-color-divider-emphasis: #41414c;
			--uui-color-default: #316dca;
			--uui-color-default-emphasis: #316dca;
			--uui-color-default-standalone: #316dca;
			--uui-color-default-contrast: #eeeeef;
			--uui-color-warning: #af7c12;
			--uui-color-warning-emphasis: #af7c12;
			--uui-color-warning-standalone: #af7c12;
			--uui-color-warning-contrast: #eeeeef;
			--uui-color-danger: #ca3b37;
			--uui-color-danger-emphasis: #ca3b37;
			--uui-color-danger-standalone: #ca3b37;
			--uui-color-danger-contrast: #eeeeef;
			--uui-color-positive: #347d39;
			--uui-color-positive-emphasis: #347d39;
			--uui-color-positive-standalone: #347d39;
			--uui-color-positive-contrast: #eeeeef;
		}
	`;

	private _styleElement?: HTMLStyleElement;

	@state()
	private _darkThemeEnabled = true;

	constructor() {
		super();
		const darkTheme = document.getElementById('dark-theme');
		if (!darkTheme) {
			this._styleElement = document.createElement('style');
			this._styleElement.innerText = this._darkTheme.cssText;
			this._styleElement.id = 'dark-theme';
		} else {
			this._styleElement = darkTheme as HTMLStyleElement;
			this._darkThemeEnabled = true;
		}
	}

	private _toggleTheme() {
		this._darkThemeEnabled = !this._darkThemeEnabled;
	}

	//if _darkThemeEnabledIs true, add the style element to the dom
	//if _darkThemeEnabledIs false, remove the style element from the dom
	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('_darkThemeEnabled') && this._styleElement) {
			if (this._darkThemeEnabled) {
				document.documentElement.insertAdjacentElement('beforeend', this._styleElement);
			} else {
				this._styleElement.remove();
			}
		}
	}

	render() {
		return html`
			<b>Dark theme toggle</b>
			<uui-button @click=${this._toggleTheme}>${this._styleElement ? 'Toggle Light' : 'Toggle Dark'}</uui-button>
		`;
	}
}

export default UmbUserDashboardTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-dashboard-test': UmbUserDashboardTestElement;
	}
}
