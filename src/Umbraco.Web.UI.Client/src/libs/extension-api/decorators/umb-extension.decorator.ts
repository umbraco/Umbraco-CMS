/* eslint-disable @typescript-eslint/no-explicit-any */
import type { ManifestBase } from '../types/index.js';

/**
 * Static property name used to store manifest metadata on decorated classes.
 */
const UMB_EXTENSION_MANIFEST = Symbol.for('umbraco:extension:manifest');

/**
 * The manifest metadata accepted by the `@umbExtension` decorator.
 *
 * Omits loader properties (element, js, elementName) and phantom types (ELEMENT_TYPE, API_TYPE)
 * since these are resolved automatically. The `api` field is preserved — it can be set to a
 * class reference to pair an API class with the decorated element class.
 *
 * The generic parameter allows using specific manifest types (e.g. `ManifestDashboard`,
 * `ManifestEntityAction`) for full type-safety.
 */
export type UmbExtensionDecoratorManifest<T extends UmbExtensionManifest = UmbExtensionManifest> = Omit<
	T,
	'element' | 'elementName' | 'js' | 'ELEMENT_TYPE' | 'API_TYPE'
>;

/**
 * A class decorator that stores extension manifest metadata on the class.
 *
 * The decorator itself has no runtime side-effects — it only tags the class
 * with manifest data. Registration happens separately via the bundle initializer
 * (automatic) or by calling {@link registerExtensionModule} manually.
 *
 * When registered, the decorated class is assigned to the manifest based on its type:
 * - `HTMLElement` subclass → `element`
 * - Anything else → `api`
 *
 * To pair an element with a separate API class, pass `api` in the manifest:
 * `@umbExtension({ ..., api: MyApiClass })`
 *
 * Supports both modern "standard" decorators (Stage 3 TC39 proposal) and
 * legacy TypeScript experimental decorators for backward compatibility.
 * @param {UmbExtensionDecoratorManifest} manifest - The extension manifest metadata.
 * @returns {UmbExtensionClassDecorator} A class decorator function.
 * @example
 * ```ts
 * // Dashboard — HTMLElement subclass, auto-assigned as element
 * @umbExtension({
 *   type: 'dashboard',
 *   alias: 'My.Dashboard',
 *   name: 'My Dashboard',
 *   meta: { label: 'My Dashboard', pathname: 'my-dashboard' },
 * })
 * @customElement('my-dashboard')
 * export default class MyDashboardElement extends UmbLitElement {
 *   render() { return html`<h1>Hello</h1>`; }
 * }
 * ```
 * @example
 * ```ts
 * // Entity action with kind — non-HTMLElement, auto-assigned as api
 * @umbExtension({
 *   type: 'entityAction',
 *   kind: 'default',
 *   alias: 'My.Action',
 *   name: 'My Action',
 *   forEntityTypes: ['document'],
 *   meta: { label: 'Do it', icon: 'icon-wand' },
 * })
 * export class MyActionApi implements UmbApi {
 *   async execute() { ... }
 * }
 * ```
 */
export function umbExtension<T extends UmbExtensionManifest = UmbExtensionManifest>(
	manifest: UmbExtensionDecoratorManifest<T>,
) {
	return ((targetOrValue: any, contextOrUndefined?: ClassDecoratorContext) => {
		// Standard decorator (Stage 3 TC39)
		if (contextOrUndefined && typeof contextOrUndefined === 'object' && contextOrUndefined.kind === 'class') {
			targetOrValue[UMB_EXTENSION_MANIFEST] = manifest;
			return targetOrValue;
		}

		// Legacy TypeScript experimental decorator
		targetOrValue[UMB_EXTENSION_MANIFEST] = manifest;
		return targetOrValue;
	}) as UmbExtensionClassDecorator;
}

/**
 * Retrieves the extension manifest metadata stored on a class by the `@umbExtension` decorator.
 * @param {object} target - The class to read metadata from.
 * @returns {UmbExtensionDecoratorManifest | undefined} The manifest metadata, or undefined if not decorated.
 */
export function getExtensionManifest(target: any): UmbExtensionDecoratorManifest | undefined {
	return target?.[UMB_EXTENSION_MANIFEST];
}

/**
 * Registers extensions from a module's exports with the provided extension registry.
 *
 * Scans the module for classes decorated with `@umbExtension` and registers each one.
 * The decorated class is automatically assigned to the manifest based on its type:
 * - `HTMLElement` subclass → `element`
 * - Anything else → `api`
 *
 * Explicit `element` or `api` references in the decorator manifest are preserved.
 * This allows pairing classes: `@umbExtension({ ..., api: MyApiClass })`
 *
 * The manifest metadata is read from the decorated class via {@link getExtensionManifest}.
 * Only classes with `@umbExtension` metadata are registered.
 * @param {object} moduleExports - The module's exports (e.g. from `import * as mod from './my-ext.js'`).
 * @param {object} registry - The UmbExtensionRegistry instance.
 * @param {(manifest: ManifestBase) => void} registry.register - The register method on the extension registry.
 * @returns {boolean} True if any decorated classes were found and registered.
 * @example
 * ```ts
 * // In a backofficeEntryPoint onInit:
 * import * as myDashboard from './my-dashboard.js';
 *
 * export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
 *   registerExtensionModule(myDashboard, extensionRegistry);
 * };
 * ```
 */
export function registerExtensionModule(
	moduleExports: Record<string, any>,
	registry: { register(manifest: ManifestBase): void },
): boolean {
	const decoratedClasses = collectDecoratedClasses(moduleExports);

	if (decoratedClasses.size === 0) {
		return false;
	}

	for (const [targetClass, manifest] of decoratedClasses) {
		const fullManifest: any = { ...manifest };

		if (!fullManifest.element && isHTMLElement(targetClass)) {
			fullManifest.element = targetClass;
		}
		if (!fullManifest.api && !isHTMLElement(targetClass)) {
			fullManifest.api = targetClass;
		}

		registry.register(fullManifest);
	}

	return true;
}

/**
 * Unregisters extensions from a module's exports that were previously registered
 * via {@link registerExtensionModule}.
 * @param {object} moduleExports - The module's exports.
 * @param {object} registry - The UmbExtensionRegistry instance.
 * @param {(alias: string) => void} registry.unregister - The unregister method on the extension registry.
 * @returns {boolean} True if any decorated classes were found and unregistered.
 */
export function unregisterExtensionModule(
	moduleExports: Record<string, any>,
	registry: { unregister(alias: string): void },
): boolean {
	const decoratedClasses = collectDecoratedClasses(moduleExports);

	if (decoratedClasses.size === 0) {
		return false;
	}

	for (const [, manifest] of decoratedClasses) {
		registry.unregister(manifest.alias);
	}

	return true;
}

/**
 * Collects all decorated classes and their manifest metadata from module exports.
 * @param {object} moduleExports - The module's exports.
 * @returns {Map<object, UmbExtensionDecoratorManifest>} Map of class → manifest metadata.
 */
function collectDecoratedClasses(moduleExports: Record<string, any>): Map<any, UmbExtensionDecoratorManifest> {
	const decoratedClasses = new Map<any, UmbExtensionDecoratorManifest>();
	for (const value of Object.values(moduleExports)) {
		const manifest = getExtensionManifest(value);
		if (manifest) {
			decoratedClasses.set(value, manifest);
		}
	}
	return decoratedClasses;
}

/**
 * Checks whether a class constructor extends HTMLElement.
 * @param {object} target - The class constructor to check.
 * @returns {boolean} True if the class extends HTMLElement.
 */
function isHTMLElement(target: any): boolean {
	return typeof HTMLElement !== 'undefined' && target.prototype instanceof HTMLElement;
}

/**
 * Type for the `@umbExtension` class decorator supporting both standard and legacy forms.
 */
type UmbExtensionClassDecorator = {
	// Standard decorator (Stage 3 TC39)
	<T extends abstract new (...args: any[]) => any>(value: T, context: ClassDecoratorContext<T>): T | void;

	// Legacy TypeScript experimental decorator
	<T extends abstract new (...args: any[]) => any>(target: T): T | void;
};
