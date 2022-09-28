import { UUIIconRegistry } from '@umbraco-ui/uui';
import icons from '../../../../public-assets/icons/icons.json';

interface UmbIconDescriptor {
	name: string;
	path: string;
}

/**
 * @export
 * @class UmbIconStore
 * @extends {UUIIconRegistry}
 * @description - Icon Store. Provides icons from the icon manifest. Icons are loaded on demand. All icons are prefixed with 'umb:'
 */
export class UmbIconStore extends UUIIconRegistry {
	/**
	 * @param {string} iconName
	 * @return {*}  {boolean}
	 * @memberof UmbIconStore
	 */
	acceptIcon(iconName: string): boolean {
		const iconManifest = icons.find((i: UmbIconDescriptor) => i.name === iconName);
		if (!iconManifest) return false;

		const icon = this.provideIcon(iconName);

		import(/* @vite-ignore */ `${iconManifest.path}`).then((iconModule) => {
			icon.svg = iconModule.default;
		});

		return true;
	}
}
