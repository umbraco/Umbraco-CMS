import { UUIIconRegistry } from '@umbraco-ui/uui';
import { UmbAuthMainContext } from '../context/auth-main.context.ts';

/**
 * This is a custom icon registry that will load icons from the Umbraco assets folder.
 * Optimistically, we will provide the icon, and then try and load it.
 */
export class UmbIconRegistry extends UUIIconRegistry {
	protected acceptIcon(iconName: string): boolean {
		// Inform that we will be providing this.
		const icon = this.provideIcon(iconName);

		UmbAuthMainContext.Instance.getIcons().subscribe((icons) => {
			if (icons[iconName]) {
				icon.svg = icons[iconName];
			} else {
				// If we can't load the icon, we will not provide it.
				console.warn(`Icon ${iconName} not found`);
			}
		});

		return true;
	}
}
