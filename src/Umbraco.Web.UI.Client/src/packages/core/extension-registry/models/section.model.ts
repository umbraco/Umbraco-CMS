import type { ConditionTypes } from '../conditions/types.js';
import type { UmbSectionElement } from '../interfaces/index.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSection
	extends ManifestElement<UmbSectionElement>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
