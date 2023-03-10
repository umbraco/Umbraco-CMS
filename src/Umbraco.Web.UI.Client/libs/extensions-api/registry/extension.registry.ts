import { BehaviorSubject, map, Observable } from 'rxjs';
import type { ManifestTypes, ManifestTypeMap, ManifestBase, ManifestEntrypoint } from '../../models';
import { loadExtension } from '../load-extension.function';
import { hasInitExport } from "../has-init-export.function";
import type { UmbControllerHostInterface } from "@umbraco-cms/controller";
import { UmbContextToken } from "@umbraco-cms/context-api";

type SpecificManifestTypeOrManifestBase<T extends keyof ManifestTypeMap | string> = T extends keyof ManifestTypeMap
	? ManifestTypeMap[T]
	: ManifestBase;

export class UmbExtensionRegistry {

	// TODO: Use UniqueBehaviorSubject, as we don't want someone to edit data of extensions.
	private _extensions = new BehaviorSubject<Array<ManifestBase>>([]);
	public readonly extensions = this._extensions.asObservable();

	register(manifest: ManifestTypes, rootHost?: UmbControllerHostInterface): void {
		const extensionsValues = this._extensions.getValue();
		const extension = extensionsValues.find((extension) => extension.alias === manifest.alias);

		if (extension) {
			console.error(`Extension with alias ${manifest.alias} is already registered`);
			return;
		}

		this._extensions.next([...extensionsValues, manifest]);

		// If entrypoint extension, we should load and run it immediately
		if (manifest.type === 'entrypoint') {
			loadExtension(manifest as ManifestEntrypoint).then((js) => {
				// If the extension has an onInit export, be sure to run that or else let the module handle itself
				if (hasInitExport(js)) {
					js.onInit(rootHost!, this);
				}
			});
		}
	}

	unregister(alias: string): void {
		const oldExtensionsValues = this._extensions.getValue();
		const newExtensionsValues = oldExtensionsValues.filter((extension) => extension.alias !== alias);

		// TODO: Maybe its not needed to fire an console.error. as you might want to call this method without needing to check the existence first.
		if (oldExtensionsValues.length === newExtensionsValues.length) {
			console.error(`Unable to unregister extension with alias ${alias}`);
			return;
		}

		this._extensions.next(newExtensionsValues);
	}

	isRegistered(alias: string): boolean {
		const values = this._extensions.getValue();
		return values.some((ext) => ext.alias === alias);
	}

	getByAlias(alias: string) {
		// TODO: make pipes prettier/simpler/reuseable
		return this.extensions.pipe(map((dataTypes) => dataTypes.find((extension) => extension.alias === alias) || null));
	}

	getByTypeAndAlias<Key extends keyof ManifestTypeMap>(type: Key, alias: string) {
		return this.extensionsOfType(type).pipe(
			map((extensions) => extensions.find((extension) => extension.alias === alias) || null)
		);
	}

	extensionsOfType<Key extends keyof ManifestTypeMap | string, T = SpecificManifestTypeOrManifestBase<Key>>(type: Key) {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => ext.type === type).sort((a, b) => (b.weight || 0) - (a.weight || 0)))
		) as Observable<Array<T>>;
	}

	extensionsOfTypes<ExtensionType = ManifestBase>(types: string[]): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(
			map((exts) =>
				exts.filter((ext) => types.indexOf(ext.type) !== -1).sort((a, b) => (b.weight || 0) - (a.weight || 0))
			)
		) as Observable<Array<ExtensionType>>;
	}

	extensionsSortedByTypeAndWeight<ExtensionType = ManifestBase>(): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(
			map((exts) => exts
				.sort((a, b) => {
					// If type is the same, sort by weight
					if (a.type === b.type) {
						return (a.weight || 0) - (b.weight || 0);
					}

					// Otherwise sort by type
					return a.type.localeCompare(b.type);
				}))
		) as Observable<Array<ExtensionType>>;
	}
}

export const UMB_EXTENSION_REGISTRY_TOKEN = new UmbContextToken<UmbExtensionRegistry>('UmbExtensionRegistry');
