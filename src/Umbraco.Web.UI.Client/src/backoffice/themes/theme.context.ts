import { map } from 'rxjs';
import { manifests } from './manifests';
import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { StringState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ManifestTheme } from '@umbraco-cms/backoffice/extensions-registry';

const LOCAL_STORAGE_KEY = 'umb-theme-alias';

export class UmbThemeContext {
	private _host: UmbControllerHostElement;

	#theme = new StringState('umb-light-theme');
	public readonly theme = this.#theme.asObservable();

	private themeSubscription?: UmbObserverController<ManifestTheme[]>;

	#styleElement: HTMLLinkElement | HTMLStyleElement | null = null;

	constructor(host: UmbControllerHostElement) {
		this._host = host;

		new UmbContextProviderController(host, UMB_THEME_CONTEXT_TOKEN, this);

		const storedTheme = localStorage.getItem(LOCAL_STORAGE_KEY);
		if (storedTheme) {
			this.setThemeByAlias(storedTheme);
		}
	}

	public setThemeByAlias(themeAlias: string) {
		this.#theme.next(themeAlias);

		this.themeSubscription?.destroy();
		if (themeAlias) {
			localStorage.setItem(LOCAL_STORAGE_KEY, themeAlias);
			this.themeSubscription = new UmbObserverController(
				this._host,
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

// TODO: Can we do this in a smarter way:
const registerExtensions = (manifests: Array<ManifestTheme>) => {
	manifests.forEach((manifest) => {
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...manifests]);
