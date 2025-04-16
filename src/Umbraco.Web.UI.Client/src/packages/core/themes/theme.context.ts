import type { ManifestTheme } from './theme.extension.js';
import { loadManifestPlainCss } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

const LOCAL_STORAGE_KEY = 'umb-theme-alias';

export class UmbThemeContext extends UmbContextBase<UmbThemeContext> {
	#theme = new UmbStringState('umb-light-theme');
	#themeObserver?: UmbObserverController<ManifestTheme[]>;

	public readonly theme = this.#theme.asObservable();

	#styleElement: HTMLLinkElement | HTMLStyleElement | null = null;

	constructor(host: UmbControllerHost) {
		super(host, UMB_THEME_CONTEXT);

		const storedTheme = localStorage.getItem(LOCAL_STORAGE_KEY);
		if (storedTheme) {
			this.setThemeByAlias(storedTheme);
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
					if (themes.length > 0 && themes[0].css) {
						const activeTheme = themes[0];
						if (typeof activeTheme.css === 'function') {
							this.#styleElement = document.createElement('style') as HTMLStyleElement;
							// We store the current style element so we can check if it has been replaced by another theme in between.
							const currentStyleEl = this.#styleElement;
							currentStyleEl.setAttribute('type', 'text/css');

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
			this.#styleElement?.remove();
			this.#styleElement?.childNodes.forEach((node) => node.remove());
			this.#styleElement?.setAttribute('href', '');
			this.#styleElement = null;
		}
	}
}

export const UMB_THEME_CONTEXT = new UmbContextToken<UmbThemeContext>('umbThemeContext');

// Default export to enable this as a globalContext extension js:
export default UmbThemeContext;
