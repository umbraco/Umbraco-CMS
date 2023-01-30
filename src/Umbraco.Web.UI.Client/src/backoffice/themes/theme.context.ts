import { map } from 'rxjs';
//import { dark, highContrast } from './themes';
import type { CSSResult } from 'lit';
import { manifests } from './manifests';
import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { StringState, UmbObserverController } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ManifestTheme } from '@umbraco-cms/extensions-registry';

export interface UmbTheme {
	name: string;
	css: CSSResult;
}


const LOCAL_STORAGE_KEY = 'umb-theme-alias';

export class UmbThemeContext {

	// TODO: Turn this into a extension type, get rid of the #themes subject and #themes observable
	/*
	#themes = new ArrayState(<Array<UmbTheme>>[
		{
			name: 'Light',
			css: '',
		},
	]);
	public readonly themes = this.#themes.asObservable();
	*/

	private _host: UmbControllerHostInterface;

	#theme = new StringState(null);
	public readonly theme = this.#theme.asObservable();

	#styleElement: HTMLStyleElement;

	private themeSubscription?: UmbObserverController;

	constructor(host: UmbControllerHostInterface) {

		this._host = host;

		console.log("Theme COntext")

		new UmbContextProviderController(host, UMB_THEME_CONTEXT_TOKEN, this);

		//TODO: Figure out how to extend this with themes from packages
		//this.addTheme(dark);
		//this.addTheme(highContrast);
		this.#styleElement = document.createElement('style');

		const storedTheme = localStorage.getItem(LOCAL_STORAGE_KEY);
		if(storedTheme) {
			this.setThemeByAlias(storedTheme);
		}

		document.documentElement.insertAdjacentElement('beforeend', this.#styleElement);
	}

	public setThemeByAlias(themeAlias: string | null) {

		this.#theme.next(themeAlias);

		this.themeSubscription?.destroy();
		if(themeAlias != null) {
			localStorage.setItem(LOCAL_STORAGE_KEY, themeAlias);
			this.themeSubscription = new UmbObserverController(this._host,
				umbExtensionsRegistry.extensionsOfType('theme').pipe(map(
					(extensions) => extensions.filter((extension) => extension.alias === themeAlias)
				))
			,
				async (themes) => {
					if (themes.length > 0 && themes[0].loader) {
						const result = await themes[0].loader();
						console.log("result from loader: ", result.default);
						this.#styleElement.innerHTML = result.default;
					}
					// how to get CSS.
					//this.#styleElement.innerHTML = "";
				}
			);
		} else {
			localStorage.removeItem(LOCAL_STORAGE_KEY);
			this.#styleElement.innerHTML = "";
		}

	}

	/*
	public addTheme(theme: UmbTheme) {
		this.#themes.next([...this.#themes.value, theme]);
	}
	*/
}

export const UMB_THEME_CONTEXT_TOKEN = new UmbContextToken<UmbThemeContext>('umbThemeContext');



// TODO: Can we do this in a smarter way:
const registerExtensions = (manifests: Array<ManifestTheme>) => {
	manifests.forEach((manifest) => {
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...manifests]);
