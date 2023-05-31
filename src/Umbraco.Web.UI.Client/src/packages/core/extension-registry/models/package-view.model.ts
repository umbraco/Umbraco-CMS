import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPackageView extends ManifestElement {
	type: 'packageView';
	meta: MetaPackageView;
}

export interface MetaPackageView {
	packageName: string;
}
