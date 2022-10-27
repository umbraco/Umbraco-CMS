import type { ManifestElement } from './models';

export interface ManifestPackageView extends ManifestElement {
	type: 'packageView';
	meta: MetaPackageView;
}

export interface MetaPackageView {
	packageAlias: string;
}
