import type {
	ManifestBase,
	ManifestKind,
	ManifestWithDynamicConditions,
	UmbConditionConfigBase,
} from '../types/index.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { map, distinctUntilChanged, combineLatest, of, switchMap } from '@umbraco-cms/backoffice/external/rxjs';

/**
 *
 * @param previousValue {Array<ManifestBase>} - previous value
 * @param currentValue {Array<ManifestBase>} - current value
 * @returns {boolean} - true if value is assumed to be the same as previous value.
 */
function extensionArrayMemoization<T extends Pick<ManifestBase, 'alias'>>(
	previousValue: Array<T>,
	currentValue: Array<T>,
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

/**
 *
 * @param previousValue {Array<ManifestBase>} - previous value
 * @param currentValue {Array<ManifestBase>} - current value
 * @returns {boolean} - true if value is assumed to be the same as previous value.
 */
function extensionAndKindMatchArrayMemoization<
	T extends Pick<ManifestBase, 'alias'> & { __isMatchedWithKind?: boolean },
>(previousValue: Array<T>, currentValue: Array<T>): boolean {
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
			// Then checking __isMatchedWithKind, as this is much more performant than checking the whole object. (I assume the only change happening to an extension is the match with a kind, we do not want to watch for other changes)
			return oldValue === undefined || newValue.__isMatchedWithKind !== oldValue.__isMatchedWithKind;
		})
	) {
		return false;
	}
	return true;
}

/**
 *
 * @param previousValue {Array<ManifestBase>} - previous value
 * @param currentValue {Array<ManifestBase>} - current value
 * @returns {boolean} - true if value is assumed to be the same as previous value.
 */
function extensionSingleMemoization<T extends Pick<ManifestBase, 'alias'>>(
	previousValue: T | undefined,
	currentValue: T | undefined,
): boolean {
	if (previousValue && currentValue) {
		return previousValue.alias === currentValue.alias;
	}
	return previousValue === currentValue;
}

/**
 *
 * @param previousValue {Array<ManifestBase>} - previous value
 * @param currentValue {Array<ManifestBase>} - current value
 * @returns {boolean} - true if value is assumed to be the same as previous value.
 */
function extensionAndKindMatchSingleMemoization<
	T extends Pick<ManifestBase, 'alias'> & { __isMatchedWithKind?: boolean },
>(previousValue: T | undefined, currentValue: T | undefined): boolean {
	if (previousValue && currentValue) {
		return (
			previousValue.alias === currentValue.alias &&
			previousValue.__isMatchedWithKind === currentValue.__isMatchedWithKind
		);
	}
	return previousValue === currentValue;
}

const sortExtensions = (a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0);

export class UmbExtensionRegistry<
	IncomingManifestTypes extends ManifestBase,
	IncomingConditionConfigTypes extends UmbConditionConfigBase = UmbConditionConfigBase,
	ManifestTypes extends ManifestBase = IncomingManifestTypes | ManifestBase,
> {
	readonly MANIFEST_TYPES: ManifestTypes = undefined as never;

	private _extensions = new UmbBasicState<Array<ManifestTypes>>([]);
	public readonly extensions = this._extensions.asObservable();

	private _kinds = new UmbBasicState<Array<ManifestKind<ManifestTypes>>>([]);
	public readonly kinds = this._kinds.asObservable();

	#exclusions: Array<string> = [];

	#additionalConditions: Map<string, Array<UmbConditionConfigBase>> = new Map();
	#appendAdditionalConditions(manifest: ManifestTypes) {
		const newConditions = this.#additionalConditions.get(manifest.alias);
		if (newConditions) {
			// Append the condition to the extensions conditions array
			if ((manifest as ManifestWithDynamicConditions).conditions) {
				for (const condition of newConditions) {
					(manifest as ManifestWithDynamicConditions).conditions!.push(condition);
				}
			} else {
				(manifest as ManifestWithDynamicConditions).conditions = newConditions;
			}
			this.#additionalConditions.delete(manifest.alias);
		}
		return manifest;
	}

	defineKind(kind: ManifestKind<ManifestTypes>): void {
		const extensionsValues = this._extensions.getValue();
		const extension = extensionsValues.find(
			(extension) => extension.alias === (kind as ManifestKind<ManifestTypes>).alias,
		);

		if (extension) {
			console.error(`Extension Kind with alias ${(kind as ManifestKind<ManifestTypes>).alias} is already registered`);
			return;
		}

		const nextData = this._kinds
			.getValue()
			.filter(
				(k) =>
					!(
						k.matchType === (kind as ManifestKind<ManifestTypes>).matchType &&
						k.matchKind === (kind as ManifestKind<ManifestTypes>).matchKind
					),
			);
		nextData.push(kind as ManifestKind<ManifestTypes>);
		this._kinds.setValue(nextData);
	}

	exclude(alias: string): void {
		this.#exclusions.push(alias);
		this._extensions.setValue(this._extensions.getValue().filter(this.#acceptExtension));
	}

	#acceptExtension = (ext: ManifestTypes): boolean => {
		return !this.#exclusions.includes(ext.alias);
	};

	register(manifest: ManifestTypes | ManifestKind<ManifestTypes>): void {
		const isValid = this.#validateExtension(manifest);
		if (!isValid) {
			return;
		}

		if (manifest.type === 'kind') {
			this.defineKind(manifest as ManifestKind<ManifestTypes>);
			return;
		}

		const isApproved = this.#isExtensionApproved(manifest);
		if (!isApproved) {
			return;
		}

		this._extensions.setValue([
			...this._extensions.getValue(),
			this.#appendAdditionalConditions(manifest as ManifestTypes),
		]);
	}

	getAllExtensions(): Array<ManifestTypes> {
		return this._extensions.getValue();
	}

	registerMany(manifests: Array<ManifestTypes | ManifestKind<ManifestTypes>>): void {
		// we have to register extensions individually, so we ensure a manifest is valid before continuing to the next one
		manifests.forEach((manifest) => this.register(manifest));
	}

	unregisterMany(aliases: Array<string>): void {
		aliases.forEach((alias) => this.unregister(alias));
	}

	unregister(alias: string): void {
		const newKindsValues = this._kinds.getValue().filter((kind) => kind.alias !== alias);
		const newExtensionsValues = this._extensions.getValue().filter((extension) => extension.alias !== alias);

		this._kinds.setValue(newKindsValues);
		this._extensions.setValue(newExtensionsValues);
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

	#validateExtension(manifest: ManifestTypes | ManifestKind<ManifestTypes>): boolean {
		if (!manifest.type) {
			console.error(`Extension is missing type`, manifest);
			return false;
		}

		if (!manifest.alias) {
			console.error(`Extension is missing alias`, manifest);
			return false;
		}

		return true;
	}
	#isExtensionApproved(manifest: ManifestTypes | ManifestKind<ManifestTypes>): boolean {
		if (!this.#acceptExtension(manifest as ManifestTypes)) {
			return false;
		}

		const extensionsValues = this._extensions.getValue();
		const extension = extensionsValues.find((extension) => extension.alias === (manifest as ManifestTypes).alias);

		if (extension) {
			console.error(`Extension with alias ${(manifest as ManifestTypes).alias} is already registered`);
			return false;
		}

		return true;
	}

	#kindsOfType<Key extends string>(type: Key): Observable<ManifestKind<ManifestTypes>[]> {
		return this.kinds.pipe(
			map((kinds) => kinds.filter((kind) => kind.matchType === type)),
			distinctUntilChanged(extensionArrayMemoization),
		);
	}

	#extensionsOfType<
		Key extends string,
		T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>,
	>(type: Key): Observable<Array<T>> {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => ext.type === type) as unknown as T[]),
			distinctUntilChanged(extensionArrayMemoization),
		);
	}
	#kindsOfTypes(types: string[]): Observable<ManifestKind<ManifestTypes>[]> {
		return this.kinds.pipe(
			map((kinds) => kinds.filter((kind) => types.indexOf(kind.matchType) !== -1)),
			distinctUntilChanged(extensionArrayMemoization),
		);
	}

	// TODO: can we get rid of as unknown here
	#extensionsOfTypes<ExtensionType extends ManifestBase = ManifestBase>(
		types: Array<ExtensionType['type']>,
	): Observable<Array<ExtensionType>> {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => types.indexOf(ext.type) !== -1) as unknown as Array<ExtensionType>),
			distinctUntilChanged(extensionArrayMemoization),
		);
	}

	#mergeExtensionWithKinds<ExtensionType extends ManifestBase, KindType extends ManifestKind<ManifestTypes>>([
		ext,
		kinds,
	]: [ExtensionType | undefined, Array<KindType>]): ExtensionType | undefined {
		// Specific Extension Meta merge (does not merge conditions)
		if (ext) {
			// Since we don't have the type up front in this request, we will just get all kinds here and find the matching one:
			const baseManifest = kinds.find((kind) => kind.matchKind === ext.kind)?.manifest;
			// TODO: This check can go away when making a find kind based on type and kind.
			if (baseManifest) {
				const merged = { __isMatchedWithKind: true, ...baseManifest, ...ext };
				if ((baseManifest as any).meta) {
					(merged as any).meta = { ...(baseManifest as any).meta, ...(ext as any).meta };
				}
				return merged as ExtensionType;
			}
		}
		return ext;
	}

	#mergeExtensionsWithKinds<ExtensionType extends ManifestBase, KindType extends ManifestKind<ManifestTypes>>([
		exts,
		kinds,
	]: [Array<ExtensionType>, Array<KindType>]): ExtensionType[] {
		return exts
			.map((ext) => {
				// Specific Extension Meta merge (does not merge conditions)
				const baseManifest = kinds.find((kind) => kind.matchKind === ext.kind)?.manifest;
				if (baseManifest) {
					const merged = { __isMatchedWithKind: true, ...baseManifest, ...ext } as any;
					if ((baseManifest as any).meta) {
						merged.meta = { ...(baseManifest as any).meta, ...(ext as any).meta };
					}
					return merged;
				}
				return ext;
			})
			.sort(sortExtensions);
	}

	/**
	 * Get an observable that provides an extension matching the given alias.
	 * @param alias {string} - The alias of the extension to get.
	 * @returns {Observable<T | undefined>} - An observable of the extension that matches the alias.
	 */
	byAlias<T extends ManifestBase = ManifestBase>(alias: string): Observable<T | undefined> {
		return this.extensions.pipe(
			map((exts) => exts.find((ext) => ext.alias === alias)),
			distinctUntilChanged(extensionSingleMemoization),
			switchMap((ext) => {
				if (ext?.kind) {
					return this.#kindsOfType(ext.type).pipe(map((kinds) => this.#mergeExtensionWithKinds([ext, kinds])));
				}
				return of(ext);
			}),

			distinctUntilChanged(extensionAndKindMatchSingleMemoization),
		) as Observable<T | undefined>;
	}

	/**
	 * Get an extension that matches the given alias, this will not return an observable, it is a one of retrieval if it exists at the given point in time.
	 * @param alias {string} - The alias of the extension to get.
	 * @returns {<T | undefined>} - The extension manifest that matches the alias.
	 */
	getByAlias<T extends ManifestBase = ManifestBase>(alias: string): T | undefined {
		const ext = this._extensions.getValue().find((ext) => ext.alias === alias) as T | undefined;
		if (ext?.kind) {
			return this.#mergeExtensionWithKinds([ext, this._kinds.getValue()]);
		}
		return ext;
	}

	/**
	 * Get an observable that provides extensions matching the given type and alias.
	 * @param type {string} - The type of the extensions to get.
	 * @param alias {string} - The alias of the extensions to get.
	 * @returns {Observable<T | undefined>} - An observable of the extensions that matches the type and alias.
	 */
	byTypeAndAlias<Key extends string, T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>>(
		type: Key,
		alias: string,
	): Observable<T | undefined> {
		return combineLatest([
			this.extensions.pipe(
				map((exts) => exts.find((ext) => ext.type === type && ext.alias === alias)),
				distinctUntilChanged(extensionSingleMemoization),
			),
			this.#kindsOfType(type),
		]).pipe(
			map(this.#mergeExtensionWithKinds),
			distinctUntilChanged(extensionAndKindMatchSingleMemoization),
		) as Observable<T | undefined>;
	}

	byTypeAndAliases<Key extends string, T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>>(
		type: Key,
		aliases: Array<string>,
	): Observable<Array<T>> {
		return combineLatest([
			this.extensions.pipe(
				map((exts) => exts.filter((ext) => ext.type === type && aliases.indexOf(ext.alias) !== -1) as unknown as T[]),
				distinctUntilChanged(extensionArrayMemoization),
			),
			this.#kindsOfType(type),
		]).pipe(
			map(this.#mergeExtensionsWithKinds),
			distinctUntilChanged(extensionAndKindMatchArrayMemoization),
		) as Observable<Array<T>>;
	}

	/**
	 * Get an observable of extensions by type and a given filter method.
	 * This will return the all extensions that matches the type and which filter method returns true.
	 * The filter method will be called for each extension manifest of the given type, and the first argument to it is the extension manifest.
	 * @param type {string} - The type of the extension to get
	 * @param filter {(ext: T): void} - The filter method to use to filter the extensions
	 * @returns {Observable<Array<T>>} - An observable of the extensions that matches the type and filter method
	 */
	byTypeAndFilter<Key extends string, T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>>(
		type: Key,
		filter: (ext: T) => boolean,
	): Observable<Array<T>> {
		return combineLatest([
			this.extensions.pipe(
				map((exts) => exts.filter((ext) => ext.type === type && filter(ext as unknown as T)) as unknown as T[]),
				distinctUntilChanged(extensionArrayMemoization),
			),
			this.#kindsOfType(type),
		]).pipe(
			map(this.#mergeExtensionsWithKinds),
			distinctUntilChanged(extensionAndKindMatchArrayMemoization),
		) as Observable<Array<T>>;
	}

	// TODO: Write test for this method:
	getByTypeAndFilter<
		Key extends string,
		T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>,
	>(type: Key, filter: (ext: T) => boolean): Array<T> {
		const exts = this._extensions
			.getValue()
			.filter((ext) => ext.type === type && filter(ext as unknown as T)) as unknown as T[];
		if (exts.length === 0) {
			return [];
		}
		const kinds = this._kinds.getValue();
		return exts.map((ext) => (ext?.kind ? (this.#mergeExtensionWithKinds([ext, kinds]) ?? ext) : ext));
	}

	/**
	 * Get an observable of extensions by types and a given filter method.
	 * This will return the all extensions that matches the types and which filter method returns true.
	 * The filter method will be called for each extension manifest of the given types, and the first argument to it is the extension manifest.
	 * @param types {Array<string>} - The types of the extensions to get.
	 * @param filter {(ext: T): void} - The filter method to use to filter the extensions
	 * @returns {Observable<Array<T>>} - An observable of the extensions that matches the type and filter method
	 */
	byTypesAndFilter<ExtensionTypes extends ManifestBase = ManifestBase>(
		types: string[],
		filter: (ext: ExtensionTypes) => boolean,
	): Observable<Array<ExtensionTypes>> {
		return combineLatest([
			this.extensions.pipe(
				map(
					(exts) =>
						exts.filter(
							(ext) => types.indexOf(ext.type) !== -1 && filter(ext as unknown as ExtensionTypes),
						) as unknown as Array<ExtensionTypes>,
				),
				distinctUntilChanged(extensionArrayMemoization),
			),
			this.#kindsOfTypes(types),
		]).pipe(
			map(this.#mergeExtensionsWithKinds),
			distinctUntilChanged(extensionAndKindMatchArrayMemoization),
		) as Observable<Array<ExtensionTypes>>;
	}

	/**
	 * Get an observable that provides extensions matching the given type.
	 * @param type {string} - The type of the extensions to get.
	 * @returns {Observable<T | undefined>} - An observable of the extensions that matches the type.
	 */
	byType<Key extends string, T extends ManifestBase = SpecificManifestTypeOrManifestBase<ManifestTypes, Key>>(
		type: Key,
	): Observable<Array<T>> {
		return combineLatest([this.#extensionsOfType(type), this.#kindsOfType(type)]).pipe(
			map(this.#mergeExtensionsWithKinds),
			distinctUntilChanged(extensionAndKindMatchArrayMemoization),
		) as Observable<Array<T>>;
	}

	/**
	 * Get an observable that provides extensions matching given types.
	 * @param types {Array<string>} - The types of the extensions to get.
	 * @returns {Observable<T | undefined>} - An observable of the extensions that matches the types.
	 */
	byTypes<ExtensionTypes extends ManifestBase = ManifestBase>(types: string[]): Observable<Array<ExtensionTypes>> {
		return combineLatest([this.#extensionsOfTypes<ExtensionTypes>(types), this.#kindsOfTypes(types)]).pipe(
			map(this.#mergeExtensionsWithKinds),
			distinctUntilChanged(extensionAndKindMatchArrayMemoization),
		) as Observable<Array<ExtensionTypes>>;
	}

	/**
	 * Append a new condition to an existing extension
	 * Useful to add a condition for example the Save And Publish workspace action shipped by core.
	 * @param {string} alias - The alias of the extension to append the condition to.
	 * @param  {UmbConditionConfigBase} newCondition - The condition to append to the extension.
	 */
	appendCondition(alias: string, newCondition: IncomingConditionConfigTypes) {
		this.appendConditions(alias, [newCondition]);
	}

	/**
	 * Appends an array of conditions to an existing extension
	 * @param {string} alias  - The alias of the extension to append the condition to
	 * @param {Array<UmbConditionConfigBase>} newConditions  - An array of conditions to be appended to an extension manifest.
	 */
	appendConditions(alias: string, newConditions: Array<IncomingConditionConfigTypes>) {
		const existingConditionsToBeAdded = this.#additionalConditions.get(alias);
		this.#additionalConditions.set(
			alias,
			existingConditionsToBeAdded ? [...existingConditionsToBeAdded, ...newConditions] : newConditions,
		);

		const allExtensions = this._extensions.getValue();
		for (const extension of allExtensions) {
			if (extension.alias === alias) {
				// Replace the existing extension with the updated one
				allExtensions[allExtensions.indexOf(extension)] = this.#appendAdditionalConditions(extension as ManifestTypes);

				// Update the main extensions collection/observable
				this._extensions.setValue(allExtensions);

				//Stop the search:
				break;
			}
		}
	}
}
