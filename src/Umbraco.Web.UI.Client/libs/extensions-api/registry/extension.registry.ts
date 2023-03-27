import { BehaviorSubject, map, Observable, distinctUntilChanged, combineLatest } from 'rxjs';
import type {
	ManifestTypes,
	ManifestTypeMap,
	ManifestBase,
	SpecificManifestTypeOrManifestBase,
	ManifestKind,
} from '@umbraco-cms/backoffice/extensions-registry';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

function extensionArrayMemoization<T extends { alias: string }>(
	previousValue: Array<T>,
	currentValue: Array<T>
): boolean {
	// If length is different, data is different:
	if (previousValue.length !== currentValue.length) {
		return false;
	}
	// previousValue has an alias that is not present in currentValue:
	if (previousValue.find((p) => !currentValue.find((c) => c.alias === p.alias))) {
		return false;
	}
	return true;
}

function extensionSingleMemoization<T extends { alias: string }>(
	previousValue: T | undefined,
	currentValue: T | undefined
): boolean {
	if (previousValue && currentValue) {
		return previousValue.alias === currentValue.alias;
	}
	return previousValue === currentValue;
}

const sortExtensions = (a: ManifestBase, b: ManifestBase) => (b.weight || 0) - (a.weight || 0);

export class UmbExtensionRegistry {
	// TODO: Use UniqueBehaviorSubject, as we don't want someone to edit data of extensions.
	private _extensions = new BehaviorSubject<Array<ManifestTypes>>([]);
	public readonly extensions = this._extensions.asObservable();

	private _kinds = new BehaviorSubject<Array<ManifestKind>>([]);
	public readonly kinds = this._kinds.asObservable();

	defineKind(kind: ManifestKind) {
		const nextData = this._kinds
			.getValue()
			.filter(
				(k) => !(k.matchType === (kind as ManifestKind).matchType && k.matchKind === (kind as ManifestKind).matchKind)
			);
		nextData.push(kind as ManifestKind);
		this._kinds.next(nextData);
	}

	register(manifest: ManifestTypes | ManifestKind): void {
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
		return this.kinds.pipe(
			map((kinds) => kinds.filter((kind) => kind.matchType === type)),
			distinctUntilChanged(extensionArrayMemoization)
		);
	}
	private _extensionsOfType<Key extends keyof ManifestTypeMap | string>(type: Key) {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => ext.type === type)),
			distinctUntilChanged(extensionArrayMemoization)
		);
	}
	private _kindsOfTypes(types: string[]) {
		return this.kinds.pipe(
			map((kinds) => kinds.filter((kind) => types.indexOf(kind.matchType) !== -1)),
			distinctUntilChanged(extensionArrayMemoization)
		);
	}
	private _extensionsOfTypes<ExtensionType = ManifestBase>(types: string[]): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => types.indexOf(ext.type) !== -1)),
			distinctUntilChanged(extensionArrayMemoization)
		) as Observable<Array<ExtensionType>>;
	}

	getByTypeAndAlias<
		Key extends keyof ManifestTypeMap | string,
		T extends ManifestBase = SpecificManifestTypeOrManifestBase<Key>
	>(type: Key, alias: string) {
		return combineLatest([
			this.extensions.pipe(
				map((exts) => exts.find((ext) => ext.type === type && ext.alias === alias)),
				distinctUntilChanged(extensionSingleMemoization)
			),
			this._kindsOfType(type),
		]).pipe(
			map(([ext, kinds]) => {
				// TODO: share one merge function between the different methods of this class:
				// Specific Extension Meta merge (does not merge conditions)
				if (ext) {
					const baseManifest = kinds.find((kind) => kind.matchKind === ext.kind)?.manifest;
					if (baseManifest) {
						const merged = { ...baseManifest, ...ext } as any;
						if ((baseManifest as any).meta) {
							merged.meta = { ...(baseManifest as any).meta, ...(ext as any).meta };
						}
						return merged;
					}
				}
				return ext;
			}),
			distinctUntilChanged(extensionSingleMemoization)
		) as Observable<T | undefined>;
	}

	extensionsOfType<
		Key extends keyof ManifestTypeMap | string,
		T extends ManifestBase = SpecificManifestTypeOrManifestBase<Key>
	>(type: Key) {
		return combineLatest([this._extensionsOfType(type), this._kindsOfType(type)]).pipe(
			map(([exts, kinds]) =>
				exts
					.map((ext) => {
						// Specific Extension Meta merge (does not merge conditions)
						const baseManifest = kinds.find((kind) => kind.matchKind === ext.kind)?.manifest;
						if (baseManifest) {
							const merged = { ...baseManifest, ...ext } as any;
							if ((baseManifest as any).meta) {
								merged.meta = { ...(baseManifest as any).meta, ...(ext as any).meta };
							}
							return merged;
						}
						return ext;
					})
					.sort(sortExtensions)
			),
			distinctUntilChanged(extensionArrayMemoization)
		) as Observable<Array<T>>;
	}

	extensionsOfTypes<ExtensionTypes extends ManifestBase = ManifestBase>(
		types: string[]
	): Observable<Array<ExtensionTypes>> {
		return combineLatest([this._extensionsOfTypes(types), this._kindsOfTypes(types)]).pipe(
			map(([exts, kinds]) =>
				exts
					.map((ext) => {
						// Specific Extension Meta merge (does not merge conditions)
						if (ext) {
							const baseManifest = kinds.find((kind) => kind.matchKind === ext.kind)?.manifest;
							if (baseManifest) {
								const merged = { ...baseManifest, ...ext } as any;
								if ((baseManifest as any).meta) {
									merged.meta = { ...(baseManifest as any).meta, ...(ext as any).meta };
								}
								return merged;
							}
						}
						return ext;
					})
					.sort(sortExtensions)
			),
			distinctUntilChanged(extensionArrayMemoization)
		) as Observable<Array<ExtensionTypes>>;
	}
}

export const UMB_EXTENSION_REGISTRY_TOKEN = new UmbContextToken<UmbExtensionRegistry>('UmbExtensionRegistry');
