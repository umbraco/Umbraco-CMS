import type { ManifestSection } from '../../extensions/index.js';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

// TODO: remove extension of ManifestSection
export interface UmbSectionItemModel extends UmbItemModel, ManifestSection {
	name: string;
}
