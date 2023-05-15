import { UUIIconRegistry } from '@umbraco-ui/uui-icon-registry';
import icons from '../../../../public-assets/icons/icons.json';

interface UmbIconDescriptor {
	name: string;
	path: string;
}

/**
 * @export
 * @class UmbIconRegistry
 * @extends {UUIIconRegistry}
 * @description - Icon Registry. Provides icons from the icon manifest. Icons are loaded on demand. All icons are prefixed with 'umb:'
 */
export class UmbIconRegistry extends UUIIconRegistry {
	/**
	 * @param {string} iconName
	 * @return {*}  {boolean}
	 * @memberof UmbIconStore
	 */
	acceptIcon(iconName: string): boolean {
		const iconManifest = icons.find((i: UmbIconDescriptor) => i.name === iconName);
		if (!iconManifest) return false;

		const icon = this.provideIcon(iconName);

		const iconPath = `${import.meta.env.BASE_URL}${iconManifest.path}`;

		import(/* @vite-ignore */ iconPath)
			.then((iconModule) => {
				icon.svg = iconModule.default;
			})
			.catch((err) => {
				console.error(`Failed to load icon ${iconName} on path ${iconPath}`, err.message);
			});

		return true;
	}
}
