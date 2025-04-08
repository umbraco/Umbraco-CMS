export interface IRouterSlot<D = any, P = any> extends HTMLElement {
	readonly route: IRoute<D> | null;
	readonly isRoot: boolean;
	readonly fragments: IPathFragments | null;
	readonly params: Params | null;
	readonly match: IRouteMatch<D> | null;
	routes: IRoute<D>[];
	add: (routes: IRoute<D>[], navigate?: boolean) => void;
	clear: () => void;
	render: () => Promise<void>;
	constructAbsolutePath: (path: PathFragment) => string;
	parent: IRouterSlot<P> | null | undefined;
	queryParentRouterSlot: () => IRouterSlot<P> | null;
}

export type IRoutingInfo<D = any, P = any> = {
	slot: IRouterSlot<D, P>;
	match: IRouteMatch<D>;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export type CustomResolver<D = any, P = any> = (
	info: IRoutingInfo<D>,
) => boolean | void | Promise<boolean> | Promise<void>;
export type Guard<D = any, P = any> = (info: IRoutingInfo<D, P>) => boolean | Promise<boolean>;
export type Cancel = () => boolean;

export type PageComponent = HTMLElement | undefined;
export type ModuleResolver = Promise<{ default?: any /*PageComponent*/; element?: any /*PageComponent*/ }>;
export type Class<T extends PageComponent = PageComponent> = { new (...args: any[]): T };
export type Component =
	| Class
	| ModuleResolver
	| PageComponent
	| (() => Class)
	| (() => PromiseLike<Class>)
	| (() => PageComponent)
	| (() => PromiseLike<PageComponent>)
	| (() => ModuleResolver)
	| (() => PromiseLike<ModuleResolver>);
export type Setup<D = any> = (component: PageComponent, info: IRoutingInfo<D>) => void;

export type RouterTree<D = any, P = any> = ({ slot: IRouterSlot<D, P> } & { child?: RouterTree }) | null | undefined;
export type PathMatch = 'prefix' | 'suffix' | 'full' | 'fuzzy';

/**
 * The base route interface.
 * D = the data type of the data
 */
export interface IRouteBase<D = any> {
	// The path for the route fragment
	path: PathFragment;

	// Optional metadata
	data?: D;

	// If guard returns false, the navigation is not allowed
	guards?: Guard[];

	// The type of match.
	// - If "prefix" router-slot will try to match the first part of the path.
	// - If "suffix" router-slot will try to match the last part of the path.
	// - If "full" router-slot will try to match the entire path.
	// - If "fuzzy" router-slot will try to match an arbitrary part of the path.
	pathMatch?: PathMatch;
}

/**
 * Route type used for redirection.
 */
export interface IRedirectRoute<D = any> extends IRouteBase<D> {
	// The paths the route should redirect to. Can either be relative or absolute.
	redirectTo: string;

	// Whether the query should be preserved when redirecting.
	preserveQuery?: boolean;
}

/**
 * Route type used to resolve and stamp components.
 */
export interface IComponentRoute<D = any> extends IRouteBase<D> {
	// The component loader (should return a module with a default export)
	component: Component | PromiseLike<Component>;

	// A custom setup function for the instance of the component.
	setup?: Setup;
}

/**
 * Route type used to take control of how the route should resolve.
 */
export interface IResolverRoute<D = any> extends IRouteBase<D> {
	// A custom resolver that handles the route change
	resolve: CustomResolver;
}

export type IRoute<D = any> = IRedirectRoute<D> | IComponentRoute<D> | IResolverRoute<D>;
export type PathFragment = string;
export type IPathFragments = {
	consumed: PathFragment;
	rest: PathFragment;
};

export interface IRouteMatch<D = any> {
	route: IRoute<D>;
	params: Params;
	fragments: IPathFragments;
	match: RegExpMatchArray;
}

export type PushStateEvent = CustomEvent<null>;
export type ReplaceStateEvent = CustomEvent<null>;
export type ChangeStateEvent = CustomEvent<null>;
export type WillChangeStateEvent = CustomEvent<{ url?: string | null; eventName: GlobalRouterEvent }>;
export type NavigationStartEvent<D = any> = CustomEvent<IRoutingInfo<D>>;
export type NavigationSuccessEvent<D = any> = CustomEvent<IRoutingInfo<D>>;
export type NavigationCancelEvent<D = any> = CustomEvent<IRoutingInfo<D>>;
export type NavigationErrorEvent<D = any> = CustomEvent<IRoutingInfo<D>>;
export type NavigationEndEvent<D = any> = CustomEvent<IRoutingInfo<D>>;

export type Params = { [key: string]: string };
export type Query = { [key: string]: string };

export type EventListenerSubscription = () => void;

/**
 * RouterSlot related events.
 */
export type RouterSlotEvent = 'changestate';

/**
 * History related events.
 */
export type GlobalRouterEvent =
	// An event triggered when a new state is added to the history.
	| 'pushstate'

	// An event triggered when the current state is replaced in the history.
	| 'replacestate'

	// An event triggered when a state in the history is popped from the history.
	| 'popstate'

	// An event triggered when the state changes (eg. pop, push and replace)
	| 'changestate'

	// A cancellable event triggered before the history state changes.
	| 'willchangestate'

	// An event triggered when navigation starts.
	| 'navigationstart'

	// An event triggered when navigation is canceled. This is due to a route guard returning false during navigation.
	| 'navigationcancel'

	// An event triggered when navigation fails due to an unexpected error.
	| 'navigationerror'

	// An event triggered when navigation successfully completes.
	| 'navigationsuccess'

	// An event triggered when navigation ends.
	| 'navigationend';

export interface ISlashOptions {
	start: boolean;
	end: boolean;
}

/* Extend the global event handlers map with the router related events */
declare global {
	interface GlobalEventHandlersEventMap {
		pushstate: PushStateEvent;
		replacestate: ReplaceStateEvent;
		popstate: PopStateEvent;
		changestate: ChangeStateEvent;
		navigationstart: NavigationStartEvent;
		navigationend: NavigationEndEvent;
		navigationsuccess: NavigationSuccessEvent;
		navigationcancel: NavigationCancelEvent;
		navigationerror: NavigationErrorEvent;
		willchangestate: WillChangeStateEvent;
	}
}
