import { UMB_THEME_LIGHT_ALIAS } from './constants.js';
import type { ManifestTheme, UmbThemeColorScheme } from './theme.extension.js';
import { loadManifestPlainCss } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

const LOCAL_STORAGE_KEY = 'umb-theme-alias';

export class UmbThemeContext extends UmbContextBase {
	#theme = new UmbStringState(UMB_THEME_LIGHT_ALIAS);
	#themeObserver?: UmbObserverController<ManifestTheme[]>;
	#themeManifest?: ManifestTheme;

	/**
	 * The currently active theme alias.
	 */
	public readonly theme = this.#theme.asObservable();

	#colorScheme = new UmbBasicState<UmbThemeColorScheme>('light');
	/**
	 * The color scheme accordingly to the presented theme.
	 */
	public readonly colorScheme = this.#colorScheme.asObservable();

	#mediaQueryList?: MediaQueryList;
	#mediaQueryListener = () =>
		this.#colorScheme.setValue(
			this.#mediaQueryList?.matches ? (this.#themeManifest?.meta?.colorScheme ?? 'light') : 'light',
		);

	#styleElement: HTMLLinkElement | HTMLStyleElement | null = null;

	constructor(host: UmbControllerHost) {
		super(host, UMB_THEME_CONTEXT);

		const storedTheme = localStorage.getItem(LOCAL_STORAGE_KEY);
		if (storedTheme) {
			this.setThemeByAlias(storedTheme);
		}
	}

	#trackColorScheme(theme?: ManifestTheme) {
		this.#mediaQueryList?.removeEventListener('change', this.#mediaQueryListener);
		this.#mediaQueryList = undefined;
		this.#themeManifest = theme;

		if (theme?.meta?.mediaQuery) {
			this.#mediaQueryList = window.matchMedia(theme.meta.mediaQuery);
			this.#mediaQueryList.addEventListener('change', this.#mediaQueryListener);
			this.#mediaQueryListener();
		} else {
			this.#colorScheme.setValue(theme?.meta?.colorScheme ?? 'light');
		}
	}

	public setThemeByAlias(themeAlias: string) {
		this.#theme.setValue(themeAlias);

		this.#themeObserver?.destroy();
		if (themeAlias) {
			localStorage.setItem(LOCAL_STORAGE_KEY, themeAlias);
			this.#themeObserver = this.observe(
				umbExtensionsRegistry.byTypeAndFilter('theme', (extension) => extension.alias === themeAlias),
				async (themes) => {
					this.#styleElement?.remove();
					this.#trackColorScheme(themes[0]);
					if (themes.length > 0 && themes[0].css) {
						const activeTheme = themes[0];
						if (typeof activeTheme.css === 'function') {
							this.#styleElement = document.createElement('style') as HTMLStyleElement;
							// We store the current style element so we can check if it has been replaced by another theme in between.
							const currentStyleEl = this.#styleElement;
							currentStyleEl.setAttribute('type', 'text/css');
							if (activeTheme.meta?.mediaQuery) currentStyleEl.setAttribute('media', activeTheme.meta.mediaQuery);

							const result = await loadManifestPlainCss(activeTheme.css);
							// Checking that this is still our styleElement, it has not been replaced with another theme in between.
							if (result && currentStyleEl === this.#styleElement) {
								currentStyleEl.appendChild(document.createTextNode(result));
								document.head.appendChild(currentStyleEl);
							}
						} else if (typeof activeTheme.css === 'string') {
							this.#styleElement = document.createElement('link');
							this.#styleElement.setAttribute('rel', 'stylesheet');
							this.#styleElement.setAttribute('href', activeTheme.css);
							if (activeTheme.meta?.mediaQuery) this.#styleElement.setAttribute('media', activeTheme.meta.mediaQuery);
							document.head.appendChild(this.#styleElement);
						}
					} else {
						// We could not load a theme for this alias, so we remove the theme.
						localStorage.removeItem(LOCAL_STORAGE_KEY);
						this.#styleElement?.childNodes.forEach((node) => node.remove());
						this.#styleElement?.setAttribute('href', '');
						this.#styleElement = null;
					}
				},
			);
		} else {
			// Super clean, we got a falsy value, so we remove the theme.

			localStorage.removeItem(LOCAL_STORAGE_KEY);
			this.#trackColorScheme();
			this.#styleElement?.remove();
			this.#styleElement?.childNodes.forEach((node) => node.remove());
			this.#styleElement?.setAttribute('href', '');
			this.#styleElement = null;
		}
	}

	override destroy(): void {
		this.#mediaQueryList?.removeEventListener('change', this.#mediaQueryListener);
		this.#mediaQueryList = undefined;
		this.#themeManifest = undefined;
		this.#styleElement?.remove();
		this.#styleElement = null;
		super.destroy();
	}
}

export const UMB_THEME_CONTEXT = new UmbContextToken<UmbThemeContext>('umbThemeContext');

// Default export to enable this as a globalContext extension js:
export default UmbThemeContext;
