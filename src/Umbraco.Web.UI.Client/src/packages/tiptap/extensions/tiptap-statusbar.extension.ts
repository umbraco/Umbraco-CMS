import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapStatusbarExtension<
	MetaType extends MetaTiptapStatusbarExtension = MetaTiptapStatusbarExtension,
> extends ManifestElement<UmbControllerHostElement> {
	type: 'tiptapStatusbarExtension';
	forExtensions?: Array<string>;
	meta: MetaType;
}

export interface MetaTiptapStatusbarExtension {
	alias: string;
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTiptapStatusbarExtension: ManifestTiptapStatusbarExtension;
	}
}
