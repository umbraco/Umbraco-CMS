import type { ManifestSection } from './section.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbSectionElement extends UmbControllerHostElement {
	manifest?: ManifestSection;
}
