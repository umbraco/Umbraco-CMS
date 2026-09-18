/* eslint-disable @typescript-eslint/naming-convention */
import '@umbraco-cms/backoffice/extension-registry';

/**
 * The Umbraco CMS package manifest extension types.
 *
 * This is the union of every extension type known to the backoffice. It is generated as a separate
 * schema so it can be composed into the `extensions` array items of the aggregate
 * `umbraco-package-schema.json`, alongside the extension types contributed by other packages,
 * while the base manifest shape lives in `umbraco-package-schema.ts`.
 */
export type UmbracoPackageExtensions = UmbExtensionManifest;
