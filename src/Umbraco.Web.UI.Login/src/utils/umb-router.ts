import { LitElement, TemplateResult } from 'lit';

interface AnchorElement {
	nodeName: string;
	target: string;
	hasAttribute(name: string): boolean;
	pathname: string;
	search: string;
	hash: string;
	origin?: string;
	port: string;
	protocol: string;
	hostname: string;
	host: string;
}

type UmbRouterPath = {
	path: string;
	component: TemplateResult | (() => TemplateResult);
	search?: string;
	default?: boolean;
	action?(pathname: string, search: string, hash: string): string | null;
};

export default class UmbRouter {
	#host: LitElement;
	#paths: UmbRouterPath[] = [];
	#currentPath = '';
	#currentSearch = '';
	#currentHash = '';

	constructor(host: LitElement, paths: UmbRouterPath[]) {
		this.#host = host;
		this.#paths = paths;

		this.#updateUrl(window.location.pathname, window.location.search, window.location.hash, true);

		window.addEventListener('umbroute:change', this.#onUmbRouteChange.bind(this));
	}

	public subscribe() {
		this.#host.addEventListener('click', this.#onClick.bind(this));
		window.addEventListener('popstate', this.#onPopState.bind(this));
	}

	public unsubscribe() {
		this.#host.removeEventListener('click', this.#onClick.bind(this));
		window.removeEventListener('popstate', this.#onPopState.bind(this));
	}

	/**
	 * Push or replace a new state to the browser history and route immediately
	 * @param path The local path
	 * @param replace Replace the current state instead of pushing a new one
	 */
	static redirect(path: string, replace = false): void {
		const { pathname, search, hash } = new URL(path, document.baseURI);

		window.dispatchEvent(
			new CustomEvent('umbroute:change', {
				detail: {
					pathname,
					search,
					hash,
					replace,
				},
			})
		);
	}

	public render = () => {
		// Find the current path object in the paths array based on the currentPath
		const originalPath = this.#paths.find((p) => p.path === this.#currentPath);

		// Check if the current path object has an action function and call it
		// to see if a redirect is needed (newPath)
		const newPathName = originalPath?.action?.(this.#currentPath, this.#currentSearch, this.#currentHash);

		if (newPathName) {
			this.#updateUrl(newPathName, this.#currentSearch, this.#currentHash);
		}

		// If newPath is not null and not undefined, find the corresponding path object
		const newPath = newPathName && this.#paths.find((p) => p.path === newPathName);

		// Return the component based on the conditions using the ternary operator
		const cmp =
			(newPath && newPath.component) || // Return new path component if newPath is not null
			(originalPath && originalPath.component) || // Return original path component if newPath is not null
			this.#paths.find((p) => p.default)?.component; // Find and return default path component

		return typeof cmp === 'function' ? cmp() : cmp;
	};

	#onUmbRouteChange(event: any) {
		const { pathname, search, hash, replace } = event.detail;
		this.#updateUrl(pathname, search, hash, replace);
	}

	#updateUrl(path: string, search: string, hash: string, replace = false) {
		if (path.startsWith(new URL(document.baseURI).pathname)) {
			path = path.substring(new URL(document.baseURI).pathname.length);
		}

		// Check if temp exists in paths or find the default path
		const pathToUse = this.#paths.find((p) => p.path === path) || this.#paths.find((p) => p.default);

		path = pathToUse ? pathToUse.path : path;

		replace
			? history.replaceState({}, '', `${path}${search || ''}${hash || ''}`)
			: history.pushState({}, '', `${path}${search || ''}${hash || ''}`);

		this.#currentPath = path;
		this.#currentSearch = search;
		this.#currentHash = hash;

		this.#host.requestUpdate();
	}

	#onPopState(event: PopStateEvent) {
		event.preventDefault();

		const { pathname, search, hash } = window.location;
		this.#updateUrl(pathname, search, hash, true);
	}

	#onClick(event: any) {
		if (event.defaultPrevented) return;

		if (this.#isModifierKeyPressed(event)) return;

		const anchor = this.#findAnchorElement(event);

		if (!anchor) return;

		if (!this.#isDefaultTarget(anchor)) return;

		if (anchor.hasAttribute('download')) return;

		if (anchor.hasAttribute('umb-router-ignore')) return;

		if (this.#isSamePageFragment(anchor)) return;

		if (this.#isExternalURL(anchor)) return;

		event.preventDefault();

		const { pathname, search, hash } = anchor;

		this.#updateUrl(pathname, search, hash);
	}

	#isModifierKeyPressed(event: MouseEvent): boolean {
		return event.shiftKey || event.ctrlKey || event.altKey || event.metaKey;
	}

	#findAnchorElement(event: PointerEvent): AnchorElement | null {
		const path = event.composedPath ? event.composedPath() : (event as any).path;

		for (let i = 0; i < path.length; i++) {
			const target = path[i];
			if (target.nodeName && target.nodeName.toLowerCase() === 'a') {
				return target as HTMLAnchorElement;
			}
		}

		return null;
	}

	#isDefaultTarget(anchor: AnchorElement): boolean {
		return !anchor.target || anchor.target.toLowerCase() === '_self';
	}

	#isSamePageFragment(anchor: AnchorElement): boolean {
		return anchor.pathname === window.location.pathname && anchor.hash !== '';
	}

	#isExternalURL(anchor: AnchorElement): boolean {
		const origin = anchor.origin || this.#getAnchorOrigin(anchor);
		return origin !== window.location.origin;
	}

	#getAnchorOrigin(anchor: AnchorElement) {
		const { port, protocol } = anchor;
		const defaultHttp = protocol === 'http:' && port === '80';
		const defaultHttps = protocol === 'https:' && port === '443';
		const host = defaultHttp || defaultHttps ? anchor.hostname : anchor.host;
		return `${protocol}//${host}`;
	}
}
