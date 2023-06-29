import type { ManifestTypeMap, ManifestBase, SpecificManifestTypeOrManifestBase, ManifestKind } from '../types.js';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { map, Observable, distinctUntilChanged, combineLatest } from '@umbraco-cms/backoffice/external/rxjs';

function extensionArrayMemoization<T extends Pick<ManifestBase, 'alias'>>(
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

// Note: Keeping the memoization in two separate function, for performance concern.
function extensionAndKindMatchArrayMemoization<T extends Pick<ManifestBase, 'alias'> & { isMatchedWithKind?: boolean }>(
	previousValue: Array<T>,
	currentValue: Array<T>
): boolean {
	// If length is different, data is different:
	if (previousValue.length !== currentValue.length) {
		return false;
	}
	// previousValue has an alias that is not present in currentValue:
	/* !! This is covered by the test below:
	if (previousValue.find((p) => !currentValue.find((c) => c.alias === p.alias))) {
		return false;
	}*/
	// if previousValue has different meta values:
	if (
		currentValue.find((newValue: T) => {
			const oldValue = previousValue.find((c) => c.alias === newValue.alias);
			// First check if we found a previous value, matching this alias.
			// Then checking isMatchedWithKind, as this is much more performant than checking the whole object. (I assume the only change happening to an extension is the match with a kind, we do not want to watch for other changes)
			return oldValue === undefined || newValue.isMatchedWithKind !== oldValue.isMatchedWithKind;
		})
	) {
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

export class UmbExtensionRegistry<
	IncomingManifestTypes extends ManifestBase,
	ManifestTypes extends ManifestBase = IncomingManifestTypes | ManifestBase
> {
	readonly MANIFEST_TYPES: ManifestTypes = undefined as never;

	private _extensions = new UmbBasicState<Array<ManifestTypes>>([]);
	public readonly extensions = this._extensions.asObservable();

	private _kinds = new UmbBasicState<Array<ManifestKind<ManifestTypes>>>([]);
	public readonly kinds = this._kinds.asObservable();

	defineKind(kind: ManifestKind<ManifestTypes>) {
		const nextData = this._kinds
			.getValue()
			.filter(
				(k) =>
					!(
						k.matchType === (kind as ManifestKind<ManifestTypes>).matchType &&
						k.matchKind === (kind as ManifestKind<ManifestTypes>).matchKind
					)
			);
		nextData.push(kind as ManifestKind<ManifestTypes>);
		this._kinds.next(nextData);
	}

	register(manifest: ManifestTypes | ManifestKind<ManifestTypes>): void {
		// TODO: Consider if we need to implement some safety features here, like checking if the object has a 'type' and/or 'alias'?

		if (manifest.type === 'kind') {
			this.defineKind(manifest as ManifestKind<ManifestTypes>);
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

	registerMany(manifests: Array<ManifestTypes | ManifestKind<ManifestTypes>>): void {
		manifests.forEach((manifest) => this.register(manifest));
	}

	unregisterMany(aliases: Array<string>): void {
		aliases.forEach((alias) => this.unregister(alias));
	}

	unregister(alias: string): void {
		const newKindsValues = this._kinds.getValue().filter((kind) => kind.alias !== alias);
		const newExtensionsValues = this._extensions.getValue().filter((extension) => extension.alias !== alias);

		this._kinds.next(newKindsValues);
		this._extensions.next(newExtensionsValues);
	}

	isRegistered(alias: string): boolean {
		if (this._extensions.getValue().find((ext) => ext.alias === alias)) {
			return true;
		}

		if (this._kinds.getValue().find((ext) => ext.alias === alias)) {
			return true;
		}

		return false;
	}

	/*
	getByAlias(alias: string) {
		// TODO: make pipes prettier/simpler/reuseable
		return this.extensions.pipe(map((extensions) => extensions.find((extension) => extension.alias === alias) || null));
	}
	*/

	private _kindsOfType<Key extends keyof ManifestTypeMap<ManifestTypes> | string>(type: Key) {
		return this.kinds.pipe(
			map((kinds) => kinds.filter((kind) => kind.matchType === type)),
			distinctUntilChanged(extensionArrayMemoization)
		);
	}
	private _extensionsOfType<Key extends keyof ManifestTypeMap<ManifestTypes> | string>(type: Key) {
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

	// TODO: can we get rid of as unknown here
	private _extensionsOfTypes<ExtensionType extends ManifestBase = ManifestBase>(
		types: Array<ExtensionType['type']>
	): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => types.indexOf(ext.type) !== -1)),
			distinctUntilChanged(extensionArrayMemoization)
		) as unknown as Observable<Array<ExtensionType>>;
	}

	getByTypeAndAlias<
		Key extends keyof ManifestTypeMap<ManifestTypes> | string,
		T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>
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
						const merged = { isMatchedWithKind: true, ...baseManifest, ...ext } as any;
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
		Key extends keyof ManifestTypeMap<ManifestTypes> | string,
		T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>
	>(type: Key) {
		return combineLatest([this._extensionsOfType(type), this._kindsOfType(type)]).pipe(
			map(([exts, kinds]) =>
				exts
					.map((ext) => {
						// Specific Extension Meta merge (does not merge conditions)
						const baseManifest = kinds.find((kind) => kind.matchKind === ext.kind)?.manifest;
						if (baseManifest) {
							const merged = { isMatchedWithKind: true, ...baseManifest, ...ext } as any;
							if ((baseManifest as any).meta) {
								merged.meta = { ...(baseManifest as any).meta, ...(ext as any).meta };
							}
							return merged;
						}
						return ext;
					})
					.sort(sortExtensions)
			),
			distinctUntilChanged(extensionAndKindMatchArrayMemoization)
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
								const merged = { isMatchedWithKind: true, ...baseManifest, ...ext } as any;
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
			distinctUntilChanged(extensionAndKindMatchArrayMemoization)
		) as Observable<Array<ExtensionTypes>>;
	}
}
