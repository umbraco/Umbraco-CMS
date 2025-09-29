import type { ManifestEntitySign } from './entity-sign.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntitySignElement extends UmbControllerHostElement {
	manifest?: ManifestEntitySign;
}
