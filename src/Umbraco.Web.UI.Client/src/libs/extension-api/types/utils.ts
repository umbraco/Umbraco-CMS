import type { UmbApi } from '../models/index.js';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type ClassConstructor<T = object> = new (...args: any[]) => T;

// Module Export Types:

export type CssLoaderExports<CssType extends string = string> = { default: CssType } | { css: CssType };

export type ElementLoaderExports<ElementType extends HTMLElement = HTMLElement> =
	| { default: ClassConstructor<ElementType> }
	| { element: ClassConstructor<ElementType> }; // | Omit<Omit<object, 'element'>, 'default'>

export type ApiLoaderExports<ApiType extends UmbApi = UmbApi> =
	| { default: ClassConstructor<ApiType> }
	| { api: ClassConstructor<ApiType> }; //| Omit<Omit<object, 'api'>, 'default'>

export type ElementAndApiLoaderExports<
	ElementType extends HTMLElement = HTMLElement,
	ApiType extends UmbApi = UmbApi,
> =
	| { api: ClassConstructor<ApiType> }
	| { element: ClassConstructor<ElementType> }
	| { api: ClassConstructor<ApiType>; element: ClassConstructor<ElementType> }; // | Omit<Omit<Omit<object, 'element'>, 'api'>, 'default'>

// Promise Types:

export type CssLoaderPromise<CssType extends string = string> = () => Promise<CssLoaderExports<CssType>>;

export type JsLoaderPromise<JsExportType> = () => Promise<JsExportType>;

export type ElementLoaderPromise<ElementType extends HTMLElement = HTMLElement> = () => Promise<
	ElementLoaderExports<ElementType>
>;

export type ApiLoaderPromise<ApiType extends UmbApi = UmbApi> = () => Promise<ApiLoaderExports<ApiType>>;

export type ElementAndApiLoaderPromise<
	ElementType extends HTMLElement = HTMLElement,
	ApiType extends UmbApi = UmbApi,
> = () => Promise<ElementAndApiLoaderExports<ElementType, ApiType>>;

// Property Types:

export type CssLoaderProperty<CssType extends string = string> = string | CssLoaderPromise<CssType>;
export type JsLoaderProperty<JsExportType> = string | JsLoaderPromise<JsExportType>;
export type ElementLoaderProperty<ElementType extends HTMLElement = HTMLElement> =
	| string
	| ElementLoaderPromise<ElementType>
	| ClassConstructor<ElementType>;
export type ApiLoaderProperty<ApiType extends UmbApi = UmbApi> =
	| string
	| ApiLoaderPromise<ApiType>
	| ClassConstructor<ApiType>;
export type ElementAndApiLoaderProperty<
	ElementType extends HTMLElement = HTMLElement,
	ApiType extends UmbApi = UmbApi,
> =
	| string
	| ElementAndApiLoaderPromise<ElementType, ApiType>
	| ElementLoaderPromise<ElementType>
	| ApiLoaderPromise<ApiType>;
