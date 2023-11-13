import type { UmbApi } from "../models/index.js";


// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type HTMLElementConstructor<T = HTMLElement> = new (...args: any[]) => T;

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type ClassConstructor<T = object> = new (...args: any[]) => T;



// Module Export Types:

export type ElementLoaderExports<
	ElementType extends HTMLElement = HTMLElement
> = ({default: ClassConstructor<ElementType>} | {element: ClassConstructor<ElementType>})// | Omit<Omit<object, 'element'>, 'default'>

export type ApiLoaderExports<
	ApiType extends UmbApi = UmbApi
> = ({default: ClassConstructor<ApiType>} | {api: ClassConstructor<ApiType>})//| Omit<Omit<object, 'api'>, 'default'>

export type ElementAndJsLoaderExports<
	ElementType extends HTMLElement = HTMLElement,
	ApiType extends UmbApi = UmbApi
> = ({api: ClassConstructor<ApiType>} | {element: ClassConstructor<ElementType>} | {api: ClassConstructor<ApiType>, element: ClassConstructor<ElementType>})// | Omit<Omit<Omit<object, 'element'>, 'api'>, 'default'>


// Promise Types:

export type ElementLoaderPromise<
	ElementType extends HTMLElement = HTMLElement
> = (() => Promise<ElementLoaderExports<ElementType>>)

export type ApiLoaderPromise<
	ApiType extends UmbApi = UmbApi
> = (() => Promise<ApiLoaderExports<ApiType>>)

export type ElementAndJsLoaderPromise<
	ElementType extends HTMLElement = HTMLElement,
	ApiType extends UmbApi = UmbApi
> = (() => Promise<ElementAndJsLoaderExports<ElementType, ApiType>>)


// Property Types:

export type JsLoaderProperty<JsType = unknown> = (
	string
	 | 
	(() => Promise<{default: JsType}>)// | Omit<object, 'default'>
	 | 
	JsType
);
export type ElementLoaderProperty<ElementType extends HTMLElement = HTMLElement> = (
	string
	 | 
	 ElementLoaderPromise<ElementType>
	 | 
	ClassConstructor<ElementType>
);
export type ApiLoaderProperty<ApiType extends UmbApi = UmbApi> = (
	string
	 | 
	 ApiLoaderPromise<ApiType>
	 | 
	ClassConstructor<ApiType>
);
export type ElementAndApiLoaderProperty<ElementType extends HTMLElement = HTMLElement, ApiType extends UmbApi = UmbApi> = (
	string
	 | 
	ElementAndJsLoaderPromise<ElementType, ApiType>
);