import { dark, highContrast } from './themes';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState, StringState } from '@umbraco-cms/observable-api';

export interface UmbTheme {
	name: string;
	css: string;
}

export class UmbThemeService {

	// TODO: Turn this into a extension type, get rid of the #themes subject and #themes observable
	#themes = new ArrayState(<Array<UmbTheme>>[
		{
			name: 'Light',
			css: '',
		},
	]);
	public readonly themes = this.#themes.asObservable();

	#theme = new StringState('Light');
	public readonly theme = this.#theme.asObservable();

	#styleElement: HTMLStyleElement;

	constructor() {
		//TODO: Figure out how to extend this with themes from packages
		this.addTheme(dark);
		this.addTheme(highContrast);

		this.#styleElement = document.createElement('style');
		const storedTheme = localStorage.getItem('umb-theme');
		this.changeTheme(storedTheme ?? this.#theme.value);

		document.documentElement.insertAdjacentElement('beforeend', this.#styleElement);
	}

	public changeTheme(theme: string) {
		this.#theme.next(theme);
		localStorage.setItem('umb-theme', theme);

		// TODO: This should come from the extension API:
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
