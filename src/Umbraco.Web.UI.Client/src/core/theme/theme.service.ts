import { BehaviorSubject } from 'rxjs';
import { css } from 'lit';
import { UmbContextToken } from '@umbraco-cms/context-api';

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

	#theme = new BehaviorSubject('high-contrast');
	public readonly theme = this.#theme.asObservable();

	#styleElement: HTMLStyleElement;

	constructor() {
		this.addTheme({ name: 'dark', css: _darkTheme.cssText });
		this.addTheme({ name: 'high-contrast', css: _hightContrastTheme.cssText });
		this.#styleElement = document.createElement('style');
		const storedTheme = localStorage.getItem('umb-theme');
		this.changeTheme(storedTheme ?? this.#theme.value);

		document.documentElement.insertAdjacentElement('beforeend', this.#styleElement);
	}

	public changeTheme(theme: string) {
		this.#theme.next(theme);
		localStorage.setItem('umb-theme', theme);

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
		--uui-color-selected: #316dca;
		--uui-color-selected-emphasis: #3e79d0;
		--uui-color-selected-standalone: #5b8dd7;
		--uui-color-selected-contrast: #eeeeef;
		--uui-color-current: #316dca;
		--uui-color-current-emphasis: #3e79d0;
		--uui-color-current-standalone: #5b8dd7;
		--uui-color-current-contrast: #f000;
		--uui-color-disabled: #434c56;
		--uui-color-disabled-standalone: #545d68;
		--uui-color-disabled-contrast: #fcfcfc4d;
		--uui-color-header-surface: #21262e;
		--uui-color-header-contrast: #eeeeef;
		--uui-color-header-contrast-emphasis: #eeeeef;
		--uui-color-focus: #316dca;
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

const _hightContrastTheme = css`
	:root {
		--uui-color-selected: var(--uui-palette-violet-blue, #3544b1);
		--uui-color-selected-emphasis: var(--uui-palette-violet-blue-light, rgb(70, 86, 200));
		--uui-color-selected-standalone: var(--uui-palette-violet-blue-dark, rgb(54, 65, 156));
		--uui-color-selected-contrast: #fff;
		--uui-color-current: var(--uui-palette-spanish-pink, #f5c1bc);
		--uui-color-current-emphasis: var(--uui-palette-spanish-pink-light, rgb(248, 214, 211));
		--uui-color-current-standalone: var(--uui-palette-spanish-pink-dark, rgb(232, 192, 189));
		--uui-color-current-contrast: var(--uui-palette-space-cadet, #1b264f);
		--uui-color-disabled: var(--uui-palette-sand, #f3f3f5);
		--uui-color-disabled-standalone: var(--uui-palette-sand-dark, rgb(226, 226, 226));
		--uui-color-disabled-contrast: var(--uui-palette-grey, #c4c4c4);
		--uui-color-header-surface: var(--uui-palette-space-cadet, #1b264f);
		--uui-color-header-contrast: #fff;
		--uui-color-header-contrast-emphasis: #fff;
		--uui-color-focus: var(--uui-palette-malibu, #3879ff);
		--uui-color-surface: #fff;
		--uui-color-surface-alt: #fff;
		--uui-color-surface-emphasis: #dadada;
		--uui-color-background: #fff;
		--uui-color-text: var(--uui-palette-black, #060606);
		--uui-color-text-alt: var(--uui-palette-dune-black, #2e2b29);
		--uui-color-interactive: var(--uui-palette-space-cadet, #1b264f);
		--uui-color-interactive-emphasis: var(--uui-palette-violet-blue, #3544b1);
		--uui-color-border: #000000;
		--uui-color-border-standalone: #000000;
		--uui-color-border-emphasis: #000000;
		--uui-color-divider: #000000;
		--uui-color-divider-standalone: #000000;
		--uui-color-divider-emphasis: #000000;
		--uui-color-default: var(--uui-palette-space-cadet, #1b264f);
		--uui-color-default-emphasis: var(--uui-palette-violet-blue, #3544b1);
		--uui-color-default-standalone: var(--uui-palette-space-cadet-dark, rgb(28, 35, 59));
		--uui-color-default-contrast: #fff;
		--uui-color-warning: #ffd621;
		--uui-color-warning-emphasis: #ffdc41;
		--uui-color-warning-standalone: #ffdd43;
		--uui-color-warning-contrast: #000;
		--uui-color-danger: #c60239;
		--uui-color-danger-emphasis: #da114a;
		--uui-color-danger-standalone: #d0003b;
		--uui-color-danger-contrast: white;
		--uui-color-positive: #0d8844;
		--uui-color-positive-emphasis: #159c52;
		--uui-color-positive-standalone: #1cae5e;
		--uui-color-positive-contrast: #fff;

		--uui-shadow-depth-1: 0 0 0px 1px black;
		--uui-shadow-depth-2: 0 0 0px 1px black;
		--uui-shadow-depth-3: 0 0 0px 1px black;
		--uui-shadow-depth-4: 0 0 0px 1px black;
		--uui-shadow-depth-5: 0 0 0px 1px black;
	}
`;
