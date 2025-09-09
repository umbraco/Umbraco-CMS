import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestEntitySign } from './entity-sign.extension';

export interface UmbEntitySignElement extends UmbControllerHostElement {
	manifest?: ManifestEntitySign;
}
