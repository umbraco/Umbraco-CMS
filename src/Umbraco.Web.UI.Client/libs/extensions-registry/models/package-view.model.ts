import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestPackageView extends ManifestElement {
	type: 'packageView';
	meta: MetaPackageView;
}

export interface MetaPackageView {
	packageName: string;
}
