import { dark, highContrast } from './themes';
import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { StringState, UmbObserverController } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { map } from 'rxjs';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export interface UmbTheme {
	name: string;
	css: string;
}

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

		new UmbContextProviderController(host, UMB_THEME_SERVICE_CONTEXT_TOKEN, this);

		//TODO: Figure out how to extend this with themes from packages
		//this.addTheme(dark);
		//this.addTheme(highContrast);
		this.#styleElement = document.createElement('style');

		const storedTheme = localStorage.getItem('umb-theme');
		if(storedTheme) {
			this.setThemeByName(storedTheme);
		}

		document.documentElement.insertAdjacentElement('beforeend', this.#styleElement);
	}

	private setThemeByName(themeName: string | null) {

		this.#theme.next(themeName);

		this.themeSubscription?.destroy();
		if(themeName != null) {
			localStorage.setItem('umb-theme', themeName);
			this.themeSubscription = new UmbObserverController(this._host,
				umbExtensionsRegistry.extensionsOfType('theme').pipe(map(
					(value) => value.name === themeName
				))
			,
				(theme) => {
					// how to get CSS.
					//this.#styleElement.innerHTML = "";
				}
			);
		} else {
			localStorage.removeItem('umb-theme');
			this.#styleElement.innerHTML = "";
		}

	}

	/*
	public addTheme(theme: UmbTheme) {
		this.#themes.next([...this.#themes.value, theme]);
	}
	*/
}

export const UMB_THEME_SERVICE_CONTEXT_TOKEN = new UmbContextToken<UmbThemeContext>(UmbThemeContext.name);
function UmbObserveController(arg0: Subscription): any {
	throw new Error('Function not implemented.');
}

