import type { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';

export interface UmbSectionItemModel extends ManifestSection {
	unique: string;
}
