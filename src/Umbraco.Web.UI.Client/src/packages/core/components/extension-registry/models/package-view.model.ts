import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestPackageView extends ManifestElement {
	type: 'packageView';
	meta: MetaPackageView;
}

export interface MetaPackageView {
	packageName: string;
}
