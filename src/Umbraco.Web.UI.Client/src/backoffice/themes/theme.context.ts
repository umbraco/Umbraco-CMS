import { map } from 'rxjs';
import { manifests } from './manifests';
import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { StringState, UmbObserverController } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ManifestTheme } from '@umbraco-cms/extensions-registry';

const LOCAL_STORAGE_KEY = 'umb-theme-alias';

export class UmbThemeContext {
	private _host: UmbControllerHostInterface;

	#theme = new StringState('umb-light-theme');
	public readonly theme = this.#theme.asObservable();

	private themeSubscription?: UmbObserverController;

	#styleElement: HTMLLinkElement | null = null;

	constructor(host: UmbControllerHostInterface) {
		this._host = host;

		new UmbContextProviderController(host, UMB_THEME_CONTEXT_TOKEN, this);

		this.#styleElement = document.createElement('link');
		this.#styleElement.setAttribute('rel', 'stylesheet');
		document.head.appendChild(this.#styleElement);

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
					if (themes.length > 0 && themes[0].loader) {
						const path = await themes[0].loader();
						this.#styleElement?.setAttribute('href', path);
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
