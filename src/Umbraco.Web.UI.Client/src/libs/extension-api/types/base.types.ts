import type { UmbApi } from '../models/index.js';
import type { ManifestBase } from './manifest-base.interface.js';
import type {
	ApiLoaderProperty,
	CssLoaderProperty,
	ElementAndApiLoaderProperty,
	ElementLoaderProperty,
	JsLoaderProperty,
} from './utils.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestWithView<ElementType extends HTMLElement = HTMLElement> extends ManifestElement<ElementType> {
	meta: MetaManifestWithView;
}

export interface MetaManifestWithView {
	pathname: string;
	label: string;
	icon: string;
}

export interface ManifestElementWithElementName extends ManifestElement {
	/**
	 * The HTML web component name to use such as 'my-dashboard'
	 * Note it is NOT <my-dashboard></my-dashboard> but just the name
	 */
	elementName: string;
}

export interface ManifestPlainCss extends ManifestBase {
	/**
	 * The file location of the stylesheet file to load
	 * @TJS-type string
	 */
	css?: CssLoaderProperty;
}

export interface ManifestPlainJs<JsType> extends ManifestBase {
	/**
	 * The file location of the javascript file to load
	 * @TJS-type string
	 */
	js?: JsLoaderProperty<JsType>;
}

/**
 * The type of extension such as dashboard etc...
 */
export interface ManifestApi<ApiType extends UmbApi = UmbApi> extends ManifestBase {
	/**
	 * @TJS-ignore
	 */
	readonly API_TYPE?: ApiType;

	/**
	 * The file location of the javascript file to load
	 * @TJS-type string
	 */
	js?: ApiLoaderProperty<ApiType>;

	/**
	 * @TJS-type string
	 */
	api?: ApiLoaderProperty<ApiType>;
}

export interface ManifestElement<ElementType extends HTMLElement = HTMLElement> extends ManifestBase {
	/**
	 * @TJS-ignore
	 */
	readonly ELEMENT_TYPE?: ElementType;

	/**
	 * The file location of the javascript file to load
	 * @TJS-type string
	 */
	js?: ElementLoaderProperty<ElementType>;

	/**
	 * The file location of the element javascript file to load
	 * @TJS-type string
	 */
	element?: ElementLoaderProperty<ElementType>;

	/**
	 * The HTML web component name to use such as 'my-dashboard'
	 * Note it is NOT <my-dashboard></my-dashboard>, just the element name.
	 */
	elementName?: string;

	/**
	 * This contains properties specific to the type of extension
	 */
	meta?: unknown;
}

export interface ManifestElementAndApi<
	ElementType extends UmbControllerHostElement = UmbControllerHostElement,
	ApiType extends UmbApi = UmbApi,
> extends ManifestBase {
	/**
	 * @TJS-ignore
	 */
	readonly API_TYPE?: ApiType;
	/**
	 * @TJS-ignore
	 */
	readonly ELEMENT_TYPE?: ElementType;

	/**
	 * The file location of the javascript file to load
	 * @TJS-type string
	 */
	js?: ElementAndApiLoaderProperty<ElementType, ApiType>;

	/**
	 * The file location of the api javascript file to load
	 * @TJS-type string
	 */
	api?: ApiLoaderProperty<ApiType>;

	/**
	 * The file location of the element javascript file to load
	 * @TJS-type string
	 */
	element?: ElementLoaderProperty<ElementType>;

	/**
	 * The HTML web component name to use such as 'my-dashboard'
	 * Note it is NOT <my-dashboard></my-dashboard>, just the element name.
	 */
	elementName?: string;
}
