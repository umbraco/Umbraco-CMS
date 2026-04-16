/* eslint-disable @typescript-eslint/no-explicit-any */
import type { ManifestBase } from '../types/index.js';

/**
 * Static property name used to store manifest metadata on decorated classes.
 */
const UMB_EXTENSION_MANIFEST = Symbol.for('umbExtensionManifest');

/**
 * The manifest metadata accepted by the `@umbExtension` decorator.
 *
 * Omits loader properties (element, api, js, elementName) and phantom types (ELEMENT_TYPE, API_TYPE)
 * since these are resolved from the module's exports:
 * - `default export` → `element`
 * - `export { X as api }` → `api`
 * - `export { X as element }` → `element` (explicit, overrides default)
 *
 * The generic parameter allows using specific manifest types (e.g. `ManifestDashboard`,
 * `ManifestEntityAction`) for full type-safety, or `ManifestBase` for a loose contract.
 */
export type UmbExtensionDecoratorManifest<T extends ManifestBase = ManifestBase> = Omit<
	T,
	'element' | 'elementName' | 'js' | 'api' | 'ELEMENT_TYPE' | 'API_TYPE'
>;

/**
 * A class decorator that stores extension manifest metadata on the class.
 *
 * The decorator itself has no runtime side-effects — it only tags the class
 * with manifest data. Registration happens separately:
 * - **Vite plugin (build time):** Extracts the metadata statically and generates
 * `umbraco-package.json`. The existing pipeline handles loading and registration.
 * - **Runtime (entry point / manual):** Call {@link registerExtensionModule} with
 * the module's exports and the extension registry to register dynamically.
 *
 * The module's named exports determine how classes map to manifest fields:
 * - `default export` → `element` field
 * - `export { X as api }` → `api` field
 * - `export { X as element }` → `element` field (explicit override)
 *
 * This decorator supports both modern "standard" decorators (Stage 3 TC39 proposal) and
 * legacy TypeScript experimental decorators for backward compatibility.
 * @param {UmbExtensionDecoratorManifest} manifest - The extension manifest metadata.
 * @returns {UmbExtensionClassDecorator} A class decorator function.
 * @example
 * ```ts
 * // Simple dashboard (default export = element)
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
 * // Action with both element and API (exported as named exports)
 * @umbExtension({
 *   type: 'entityAction',
 *   alias: 'My.Action',
 *   name: 'My Action',
 * })
 * class MyActionApi implements UmbApi {
 *   async execute() { ... }
 * }
 *
 * @customElement('my-action-button')
 * class MyActionElement extends UmbLitElement { ... }
 *
 * export { MyActionApi as api, MyActionElement as element };
 * ```
 */
export function umbExtension<T extends ManifestBase = ManifestBase>(manifest: UmbExtensionDecoratorManifest<T>) {
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
 * Scans the module for classes decorated with `@umbExtension` and resolves named exports
 * to manifest fields:
 * - `default` export → `element`
 * - `element` named export → `element` (overrides default)
 * - `api` named export → `api`
 *
 * The manifest metadata is read from the decorated class via {@link getExtensionManifest}.
 * Only classes with `@umbExtension` metadata are registered.
 * @param {object} moduleExports - The module's exports (e.g. from `import * as mod from './my-ext.js'`).
 * @param {object} registry - The UmbExtensionRegistry instance.
 * @param {(manifest: ManifestBase) => void} registry.register - The register method on the extension registry.
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
): void {
	// Collect all decorated classes and their manifest metadata
	const decoratedClasses = new Map<any, UmbExtensionDecoratorManifest>();
	for (const value of Object.values(moduleExports)) {
		const manifest = getExtensionManifest(value);
		if (manifest) {
			decoratedClasses.set(value, manifest);
		}
	}

	if (decoratedClasses.size === 0) {
		return;
	}

	// Resolve exports to manifest fields
	const elementExport = moduleExports.element ?? moduleExports.default;
	const apiExport = moduleExports.api;

	// Register each decorated class with the resolved exports
	for (const [targetClass, manifest] of decoratedClasses) {
		const fullManifest: any = { ...manifest };

		if (elementExport === targetClass || (!apiExport && elementExport)) {
			fullManifest.element = targetClass;
		}

		if (apiExport === targetClass) {
			fullManifest.api = targetClass;
		} else if (apiExport) {
			// The api export exists but is a different class — attach it to this manifest
			fullManifest.api = apiExport;
		}

		if (elementExport && elementExport !== targetClass) {
			fullManifest.element = elementExport;
		}

		registry.register(fullManifest);
	}
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
