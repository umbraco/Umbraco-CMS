import { BehaviorSubject } from 'rxjs';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { css } from 'lit';

export interface UmbTheme {
	name: string;
	css: string;
}

export class UmbThemeService {
	#themes = new BehaviorSubject(<Array<UmbTheme>>[
		{
			name: 'light',
			css: '',
		},
	]);
	public readonly themes = this.#themes.asObservable();

	#theme = new BehaviorSubject('dark');
	public readonly theme = this.#theme.asObservable();

	#styleElement: HTMLStyleElement;

	constructor() {
		this.addTheme({ name: 'dark', css: _darkTheme.cssText });
		this.#styleElement = document.createElement('style');
		this.changeTheme(this.#theme.value);

		document.documentElement.insertAdjacentElement('beforeend', this.#styleElement);
	}

	public changeTheme(theme: string) {
		this.#theme.next(theme);

		const themeCss = this.#themes.value.find((t) => t.name === theme)?.css;

		if (themeCss !== undefined) {
			this.#styleElement.innerHTML = themeCss;
		}
	}

	public addTheme(theme: UmbTheme) {
		this.#themes.next([...this.#themes.value, theme]);
	}
}

export const UMB_THEME_SERVICE_CONTEXT_TOKEN = new UmbContextToken<UmbThemeService>(UmbThemeService.name);

const _darkTheme = css`
	:root {
		--uui-color-selected: #4e78cc;
		--uui-color-selected-emphasis: #4e78cc;
		--uui-color-selected-standalone: #4e78cc;
		--uui-color-selected-contrast: #fff;
		--uui-color-current: #4e78cc;
		--uui-color-current-emphasis: #4e78cc;
		--uui-color-current-standalone: #4e78cc;
		--uui-color-current-contrast: #f000;
		--uui-color-disabled: purple;
		--uui-color-disabled-standalone: purple;
		--uui-color-disabled-contrast: #fcfcfc4d;
		--uui-color-header-surface: #21262e;
		--uui-color-header-contrast: #ffffffcc;
		--uui-color-header-contrast-emphasis: #fff;
		--uui-color-focus: #4e78cc;
		--uui-color-surface: #2d333b;
		--uui-color-surface-alt: #373e47;
		--uui-color-surface-emphasis: #434c56;
		--uui-color-background: #21262e;
		--uui-color-text: #eeeeef;
		--uui-color-text-alt: #eeeeef;
		--uui-color-interactive: #eeeeef;
		--uui-color-interactive-emphasis: #eeeeef;
		--uui-color-border: #434c56;
		--uui-color-border-standalone: #545d68;
		--uui-color-border-emphasis: #626e7b;
		--uui-color-divider: #373e47;
		--uui-color-divider-standalone: #434c56;
		--uui-color-divider-emphasis: #545d68;
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
