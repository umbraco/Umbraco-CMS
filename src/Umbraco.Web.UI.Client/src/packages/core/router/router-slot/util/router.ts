/* eslint-disable no-useless-escape */
import { CATCH_ALL_WILDCARD, DEFAULT_PATH_MATCH, PARAM_IDENTIFIER, TRAVERSE_FLAG } from '../config.js';
import type {
	IComponentRoute,
	IRedirectRoute,
	IResolverRoute,
	IRoute,
	IRouteMatch,
	IRouterSlot,
	ModuleResolver,
	PageComponent,
	Params,
	PathFragment,
	RouterTree,
	IRoutingInfo,
} from '../model.js';
import { constructPathWithBasePath, path as getPath, queryString, stripSlash } from './url.js';

/**
 * Determines whether the path is active.
 * If the full path starts with the path and is followed by the end of the string or a "/" the path is considered active.
 * @param path
 * @param fullPath
 */
export function isPathActive(path: string | PathFragment, fullPath: string = getPath()): boolean {
	return new RegExp(`^${stripSlash(path)}(\/|$)`, 'gm').test(stripSlash(fullPath));
}

/**
 * Matches a route.
 * @param route
 * @param path
 */
export function matchRoute<D = any>(route: IRoute<D>, path: string | PathFragment): IRouteMatch<D> | null {
	// We start by preparing the route path by replacing the param names with a regex that matches everything
	// until either the end of the path or the next "/". While replacing the param placeholders we make sure
	// to store the names of the param placeholders.
	const paramNames: string[] = [];
	const routePath = stripSlash(
		route.path.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
			paramNames.push(args[0]);
			return `([^\/]+)`;
		}),
	);

	// Construct the regex to match with the path or fragment
	// If path is wildcard:
	// - We match with /^/ to not consume any characters.
	// If path is empty and pathmatch is not full
	// - We match with /^/ to not consume any characters.
	// If pathmatch is prefix
	// - We start the match with [/]? to allow a slash in front of the path.
	// - We end the match with (?:/|$) to make sure the match ends at either the end of the fragment or end of the path.
	// If pathmatch is suffix:
	// - We start the match with .*? to allow anything to be in front of what we are trying to match.
	// - We end the match with $ to make sure the match ends at the end of the path.
	// If pathmatch is full:
	// - We end the match with $ to make sure the match ends at the end of the path.
	// If pathmatch is fuzzy
	// - We start the match with .*? to allow anything to be in front of what we are trying to match.
	// - We end the match with .*? to allow anything to be after what we are trying to match.
	// All matches starts with ^ to make sure the match is done from the beginning of the path.
	const regex =
		route.path === CATCH_ALL_WILDCARD || (route.path.length === 0 && route.pathMatch != 'full')
			? /^/
			: (() => {
					switch (route.pathMatch || DEFAULT_PATH_MATCH) {
						case 'full':
							return new RegExp(`^${routePath}\/?$`);
						case 'suffix':
							return new RegExp(`^.*?${routePath}\/?$`);
						case 'fuzzy':
							return new RegExp(`^.*?${routePath}.*?$`);
						case 'prefix':
						default:
							return new RegExp(`^[\/]?${routePath}(?:\/|$)`);
					}
				})();

	// Check if there's a match
	const match = path.match(regex);
	if (match != null) {
		// Match the param names with the matches. The matches starts from index 1 which is the
		// reason why we add 1. match[0] is the entire string.
		const params = paramNames.reduce((acc: Params, name: string, i: number) => {
			acc[name] = match[i + 1];
			return acc;
		}, {});

		// Split up the path into two fragments: the one consumed and the rest.
		const consumed = stripSlash(path.slice(0, match[0].length));
		const rest = stripSlash(path.slice(match[0].length, path.length));

		return {
			route,
			match,
			params,
			fragments: {
				consumed,
				rest,
			},
		};
	}

	return null;
}

/**
 * Matches the first route that matches the given path.
 * @param routes
 * @param path
 */
export function matchRoutes<D = any>(routes: IRoute<D>[], path: string | PathFragment): IRouteMatch<D> | null {
	for (const route of routes) {
		const match = matchRoute(route, path);
		if (match != null) {
			return match;
		}
	}

	return null;
}

/**
 * Returns the page from the route.
 * If the component provided is a function (and not a class) call the function to get the promise.
 * @param route
 * @param info
 */
export async function resolvePageComponent(route: IComponentRoute, info: IRoutingInfo): Promise<PageComponent> {
	// Figure out if the component were given as an import or class.
	let cmp = route.component;
	if (cmp instanceof Function) {
		try {
			// eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
			cmp = (cmp as Function)();
		} catch (err) {
			// The invocation most likely failed because the function is a class.
			// If it failed due to the "new" keyword not being used, the error will be of type "TypeError".
			// This is the most reliable way to check whether the provided function is a class or a function.
			if (!(err instanceof TypeError)) {
				throw err;
			}
		}
	}

	// Load the module or component.
	const moduleClassOrPage = await Promise.resolve(<ModuleResolver>cmp);

	// Instantiate the component
	let component!: PageComponent;

	if (!(moduleClassOrPage instanceof HTMLElement)) {
		component = new (moduleClassOrPage.default
			? moduleClassOrPage.default
			: moduleClassOrPage.element
				? moduleClassOrPage.element
				: moduleClassOrPage)() as PageComponent;
	} else {
		component = moduleClassOrPage as PageComponent;
	}

	// Setup the component using the callback.
	if (route.setup != null) {
		await route.setup(component, info);
	}

	return component;
}

/**
 * Determines if a route is a redirect route.
 * @param route
 */
export function isRedirectRoute(route: IRoute): route is IRedirectRoute {
	return 'redirectTo' in route;
}

/**
 * Determines if a route is a resolver route.
 * @param route
 */
export function isResolverRoute(route: IRoute): route is IResolverRoute {
	return 'resolve' in route;
}

/**
 * Traverses the router tree up to the root route.
 * @param slot
 */
export function traverseRouterTree(slot: IRouterSlot): { tree: RouterTree; depth: number } {
	// Find the nodes from the route up to the root route
	const routes: IRouterSlot[] = [slot];
	while (slot.parent != null) {
		slot = slot.parent;
		routes.push(slot);
	}

	// Create the tree
	const tree: RouterTree = routes.reduce((acc: RouterTree, slot: IRouterSlot) => {
		return { slot, child: acc };
	}, undefined);

	const depth = routes.length;

	return { tree, depth };
}

/**
 * Generates a path based on the router tree.
 * @param tree
 * @param depth
 */
export function getFragments(tree: RouterTree, depth: number): PathFragment[] {
	let child = tree;
	const fragments: PathFragment[] = [];

	// Look through all of the path fragments
	while (child != null && child.slot.match != null && depth > 0) {
		fragments.push(child.slot.match.fragments.consumed);
		child = child.child;
		depth--;
	}

	return fragments;
}

/**
 * Constructs the correct absolute path based on a router.
 * - Handles relative paths: "mypath"
 * - Handles absolute paths: "/mypath"
 * - Handles traversing paths: "../../mypath"
 * @param slot
 * @param path
 */
export function constructAbsolutePath<D = any, P = any>(
	slot: IRouterSlot<D, P>,
	path: string | PathFragment = '',
): string {
	// Grab the router tree
	const { tree, depth } = traverseRouterTree(slot);

	// If the path starts with "/" we treat it as an absolute path
	// and therefore don't continue because it is already absolute.
	if (!path.startsWith('/')) {
		let traverseDepth = 0;

		// If the path starts with "./" we can remove that part
		// because we know the path is relative to its route.
		if (path.startsWith('./')) {
			path = path.slice(2);
		}

		// Match with the traverse flag.
		const match = path.match(new RegExp(TRAVERSE_FLAG, 'g'));
		if (match != null) {
			// If the path matched with the traverse flag we know that we have to construct
			// a route until a certain depth. The traverse depth is the amount of "../" in the path
			// and the depth is the part of the path we a slicing away.
			traverseDepth = match.length;

			// Count the amount of characters that the matches add up to and remove it from the path.
			const length = match.reduce((acc: number, m: string) => acc + m.length, 0);
			path = path.slice(length);
		}

		// Grab the fragments and construct the new path, taking the traverse depth into account.
		// Always subtract at least 1 because we the path is relative to its parent.
		// Filter away the empty fragments from the path.
		const fragments = getFragments(tree, depth - 1 - traverseDepth).filter((fragment) => fragment.length > 0);
		path = `${fragments.join('/')}${fragments.length > 0 ? '/' : ''}${path}`;
	}

	// Add the base path in front of the path. If the path is already absolute, the path wont get the base path added.
	return constructPathWithBasePath(path, { end: false });
}

/**
 * Handles a redirect.
 * @param slot
 * @param route
 */
export function handleRedirect(slot: IRouterSlot, route: IRedirectRoute) {
	history.replaceState(
		history.state,
		'',
		`${constructAbsolutePath(slot, route.redirectTo)}${route.preserveQuery ? queryString() : ''}`,
	);
}

/**
 * Determines whether the navigation should start based on the current match and the new match.
 * @param currentMatch
 * @param newMatch
 */
export function shouldNavigate<D>(currentMatch: IRouteMatch<D> | null, newMatch: IRouteMatch<D>) {
	// If the current match is not defined we should always route.
	if (currentMatch == null) {
		return true;
	}

	// Extract information about the matches
	const { route: currentRoute, fragments: currentFragments } = currentMatch;
	const { route: newRoute, fragments: newFragments } = newMatch;

	const isSameRoute = currentRoute == newRoute;
	const isSameFragments = currentFragments.consumed == newFragments.consumed;

	// Only navigate if the URL consumption is new or if the two routes are no longer the same.
	return !isSameFragments || !isSameRoute;
}
