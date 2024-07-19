import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

export interface UmbElement extends UmbClassInterface, UmbControllerHostElement {
	/**
	 * Use the UmbLocalizeController to localize your element.
	 * @see UmbLocalizationController
	 */
	localize: UmbLocalizationController;
}
