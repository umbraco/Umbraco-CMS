import type { ManifestElement } from '.';

export interface ManifestPackageView extends ManifestElement {
	type: 'packageView';
	meta: MetaPackageView;
}

export interface MetaPackageView {
	packageName: string;
}
