import { type UUIIconHost, UUIIconRegistry } from '@umbraco-cms/backoffice/external/uui';

interface UmbIconDescriptor {
	name: string;
	path: string;
}

/**
 * @export
 * @class UmbIconRegistry
 * @extends {UUIIconRegistry}
 * @description - Icon Registry. Provides icons from the icon manifest. Icons are loaded on demand. All icons are prefixed with 'icon-'
 */
export class UmbIconRegistry extends UUIIconRegistry {
	#initResolve?: () => void;
	#init: Promise<void> = new Promise((resolve) => {
		this.#initResolve = resolve;
	});

	#icons: UmbIconDescriptor[] = [];
	#unhandledProviders: Map<string, UUIIconHost> = new Map();

	setIcons(icons: UmbIconDescriptor[]) {
		const oldIcons = this.#icons;
		this.#icons = icons;
		if (this.#initResolve) {
			this.#initResolve();
			this.#initResolve = undefined;
		}
		// Go figure out which of the icons are new.
		const newIcons = this.#icons.filter((i) => !oldIcons.find((o) => o.name === i.name));
		newIcons.forEach((icon) => {
			// Do we already have a request for this one, then lets initiate the load for those:
			const unhandled = this.#unhandledProviders.get(icon.name);
			if (unhandled) {
				this.#loadIcon(icon.name, unhandled).then(() => {
					this.#unhandledProviders.delete(icon.name);
				});
			}
		});
	}
	appendIcons(icons: UmbIconDescriptor[]) {
		this.#icons = [...this.#icons, ...icons];
	}
	/**
	 * @param {string} iconName
	 * @return {*}  {boolean}
	 * @memberof UmbIconStore
	 */
	override acceptIcon(iconName: string): boolean {
		const iconProvider = this.provideIcon(iconName);
		this.#loadIcon(iconName, iconProvider);

		return true;
	}

	async #loadIcon(iconName: string, iconProvider: UUIIconHost): Promise<boolean> {
		await this.#init;
		const iconManifest = this.#icons.find((i: UmbIconDescriptor) => i.name === iconName);
		// Icon not found, so lets add it to a list of unhandled requests.
		if (!iconManifest) {
			this.#unhandledProviders.set(iconName, iconProvider);
			return false;
		}

		const iconPath = iconManifest.path;

		import(/* @vite-ignore */ iconPath)
			.then((iconModule) => {
				iconProvider.svg = iconModule.default;
			})
			.catch((err) => {
				console.error(`Failed to load icon ${iconName} on path ${iconPath}`, err.message);
			});
		return true;
	}
}
