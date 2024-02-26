import { LitElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

// TODO: Currently we don't check if the `lang` is registered in the backoffice. We should do that. We can do that by checking if the `lang` is in the `languages` array of the `language` resource and potentially make sure that UmbLocalizationRegistry only loads the localizations and some other mechanism reloads to another language (currently it does both)

/**
 * The base class for all Umbraco LitElement elements.
 *
 * @abstract
 * @remarks This class is a wrapper around the LitElement class.
 * @remarks The `dir` and `lang` properties are defined here as reactive properties so they react to language changes.
 */
export class UmbLitElement extends UmbElementMixin(LitElement) {
	/**
	 * The direction of the element.
	 *
	 * @attr
	 * @remarks This is the direction of the element, not the direction of the backoffice.
	 * @example 'ltr'
	 * @example 'rtl'
	 */
	@property() dir: 'rtl' | 'ltr' | '' = '';

	/**
	 * The language of the element.
	 *
	 * @attr
	 * @remarks This is the language of the element, not the language of the backoffice.
	 * @example 'en-us'
	 * @example 'en'
	 */
	@property() lang = '';
}
