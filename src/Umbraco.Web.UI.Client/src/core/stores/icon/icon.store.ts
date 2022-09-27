import { UUIIconRegistry } from '@umbraco-ui/uui';
import iconManifests from '../../../../public-assets/icons/icon.manifests.json';

export class UmbIconStore extends UUIIconRegistry {
	acceptIcon(iconName: string) {
		const iconManifest = iconManifests.find((i: any) => i.name === iconName);
		if (!iconManifest) return false;

		const icon = this.provideIcon(iconName);

		import(/* @vite-ignore */ `${iconManifest.path}`).then((iconModule) => {
			icon.svg = iconModule.default;
		});

		return true;
	}
}
