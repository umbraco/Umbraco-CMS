import { BehaviorSubject, withLatestFrom, map, Observable } from 'rxjs';
import type {
	ManifestTypes,
	ManifestTypeMap,
	ManifestBase,
	ManifestEntrypoint,
	SpecificManifestTypeOrManifestBase,
	ManifestKind,
} from '../../models';
import { loadExtension } from '../load-extension.function';
import { hasInitExport } from '../has-init-export.function';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextToken } from '@umbraco-cms/context-api';

export class UmbExtensionRegistry {
	// TODO: Use UniqueBehaviorSubject, as we don't want someone to edit data of extensions.
	private _extensions = new BehaviorSubject<Array<ManifestBase>>([]);
	public readonly extensions = this._extensions.asObservable();

	private _kinds = new BehaviorSubject<Array<ManifestKind>>([]);
	public readonly kinds = this._kinds.asObservable();

	defineKind(kind: ManifestKind) {
		const nextData = this._kinds
			.getValue()
			.filter(
				(k) => k.matchType !== (kind as ManifestKind).matchType && k.matchKind !== (kind as ManifestKind).matchKind
			);
		nextData.push(kind as ManifestKind);
		this._kinds.next(nextData);
	}

	register(manifest: ManifestTypes | ManifestKind, rootHost?: UmbControllerHostInterface): void {
		if (manifest.type === 'kind') {
			this.defineKind(manifest as ManifestKind);
			return;
		}

		const extensionsValues = this._extensions.getValue();
		const extension = extensionsValues.find((extension) => extension.alias === (manifest as ManifestTypes).alias);

		if (extension) {
			console.error(`Extension with alias ${(manifest as ManifestTypes).alias} is already registered`);
			return;
		}

		this._extensions.next([...extensionsValues, manifest as ManifestTypes]);

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

	/*
	getByAlias(alias: string) {
		// TODO: make pipes prettier/simpler/reuseable
		return this.extensions.pipe(map((extensions) => extensions.find((extension) => extension.alias === alias) || null));
	}
	*/

	private _kindsOfType<Key extends keyof ManifestTypeMap | string>(type: Key) {
		return this.kinds.pipe(map((kinds) => kinds.filter((kind) => kind.matchType === type)));
		// TODO: DisctinctUntilChanged by using aliases?
	}
	private _extensionsOfType<Key extends keyof ManifestTypeMap | string>(type: Key) {
		return this.extensions.pipe(map((exts) => exts.filter((ext) => ext.type === type)));
		// TODO: DisctinctUntilChanged by using aliases?
	}
	private _kindsOfTypes(types: string[]) {
		return this.kinds.pipe(map((kinds) => kinds.filter((kind) => types.indexOf(kind.matchType) !== -1)));
		// TODO: DisctinctUntilChanged by using aliases?
	}
	private _extensionsOfTypes<ExtensionType = ManifestBase>(types: string[]): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(map((exts) => exts.filter((ext) => types.indexOf(ext.type) !== -1))) as Observable<
			Array<ExtensionType>
		>;
		// TODO: DisctinctUntilChanged by using aliases?
	}

	getByTypeAndAlias<Key extends keyof ManifestTypeMap | string, T = SpecificManifestTypeOrManifestBase<Key>>(
		type: Key,
		alias: string
	) {
		return this.extensions.pipe(
			map((exts) => exts.find((ext) => ext.type === type && ext.alias === alias)),
			withLatestFrom(this._kindsOfType(type)),
			map(([ext, kinds]) => {
				// TODO: Deep merge?
				return ext ? { ...kinds.find((kind) => kind.matchKind === ext.kind)?.manifest, ...ext } : undefined;
			})
		) as Observable<T | undefined>;
		// TODO: DisctinctUntilChanged by using aliases? and kind(if kind uses alias)
	}

	extensionsOfType<Key extends keyof ManifestTypeMap | string, T = SpecificManifestTypeOrManifestBase<Key>>(type: Key) {
		return this._extensionsOfType(type).pipe(
			withLatestFrom(this._kindsOfType(type)),
			map(([exts, kinds]) =>
				exts
					.map((ext) => {
						// TODO: Deep merge?
						return { ...kinds.find((kind) => kind.matchKind === ext.kind)?.manifest, ...ext };
					})
					.sort((a, b) => (b.weight || 0) - (a.weight || 0))
			)
		) as Observable<Array<T>>;
		//
		// TODO: DisctinctUntilChanged by using aliases?
	}

	extensionsOfTypes<ExtensionTypes = ManifestBase>(types: string[]): Observable<Array<ExtensionTypes>> {
		return this._extensionsOfTypes(types).pipe(
			withLatestFrom(this._kindsOfTypes(types)),
			map(([exts, kinds]) =>
				exts
					.map((ext) => {
						// TODO: Deep merge?
						return { ...kinds.find((kind) => kind.matchKind === ext.kind)?.manifest, ...ext };
					})
					.sort((a, b) => (b.weight || 0) - (a.weight || 0))
			)
		) as Observable<Array<ExtensionTypes>>;
		//
		// TODO: DisctinctUntilChanged by using aliases?
	}

	// TODO: consider ust having the a.type.localeCompare(b.type); in the extension view, to then be able to use other existing observables?
	/**
	 * Gets all the extensions registrations, but does not merge with kinds.
	 * @returns
	 */
	extensionRegistrationsSortedByTypeAndWeight<ExtensionType = ManifestBase>(): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(
			map((exts) =>
				exts.sort((a, b) => {
					// If type is the same, sort by weight
					if (a.type === b.type) {
						return (b.weight || 0) - (a.weight || 0);
					}

					// Otherwise sort by type
					return a.type.localeCompare(b.type);
				})
			)
		) as Observable<Array<ExtensionType>>;
	}
}

export const UMB_EXTENSION_REGISTRY_TOKEN = new UmbContextToken<UmbExtensionRegistry>('UmbExtensionRegistry');
