import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStringState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { ManifestTheme, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const LOCAL_STORAGE_KEY = 'umb-theme-alias';

export class UmbThemeContext extends UmbBaseController {
	#theme = new UmbStringState('umb-light-theme');
	#themeObserver?: UmbObserverController<ManifestTheme[]>;

	public readonly theme = this.#theme.asObservable();

	#styleElement: HTMLLinkElement | HTMLStyleElement | null = null;

	constructor(host: UmbControllerHostElement) {
		super(host);

		this.provideContext(UMB_THEME_CONTEXT_TOKEN, this);

		const storedTheme = localStorage.getItem(LOCAL_STORAGE_KEY);
		if (storedTheme) {
			this.setThemeByAlias(storedTheme);
		}
	}

	public setThemeByAlias(themeAlias: string) {
		this.#theme.next(themeAlias);

		this.#themeObserver?.destroy();
		if (themeAlias) {
			localStorage.setItem(LOCAL_STORAGE_KEY, themeAlias);
			this.#themeObserver = this.observe(
				umbExtensionsRegistry
					.extensionsOfType('theme')
					.pipe(map((extensions) => extensions.filter((extension) => extension.alias === themeAlias))),
				async (themes) => {
					this.#styleElement?.remove();
					if (themes.length > 0) {
						if (themes[0].loader) {
							const styleEl = (this.#styleElement = document.createElement('style'));
							styleEl.setAttribute('type', 'text/css');
							document.head.appendChild(styleEl);

							const result = await themes[0].loader();
							// Checking that this is still our styleElement, it has not been replaced with another theme in between.
							if (styleEl === this.#styleElement) {
								(styleEl as any).appendChild(document.createTextNode(result));
							}
						} else if (themes[0].css) {
							this.#styleElement = document.createElement('link');
							this.#styleElement.setAttribute('rel', 'stylesheet');
							this.#styleElement.setAttribute('href', themes[0].css);
							document.head.appendChild(this.#styleElement);
						}
					} else {
						localStorage.removeItem(LOCAL_STORAGE_KEY);
						this.#styleElement?.setAttribute('href', '');
					}
				}
			);
		} else {
			localStorage.removeItem(LOCAL_STORAGE_KEY);
			this.#styleElement?.setAttribute('href', '');
		}
	}
}

export const UMB_THEME_CONTEXT_TOKEN = new UmbContextToken<UmbThemeContext>('umbThemeContext');

// Default export to enable this as a globalContext extension js:
export default UmbThemeContext;
