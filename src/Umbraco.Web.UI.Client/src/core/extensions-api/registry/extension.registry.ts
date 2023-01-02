import { BehaviorSubject, map, Observable } from 'rxjs';
import type {
	ManifestTypes,
	ManifestDashboard,
	ManifestWorkspaceView,
	ManifestEntrypoint,
	ManifestPropertyAction,
	ManifestPropertyEditorUI,
	ManifestPropertyEditorModel,
	ManifestSection,
	ManifestSectionView,
	ManifestTree,
	ManifestTreeItemAction,
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestCustom,
	ManifestPackageView,
	ManifestExternalLoginProvider,
	ManifestHeaderApp,
	ManifestCollectionView,
	ManifestCollectionBulkAction,
} from '../../models';
import { hasDefaultExport } from '../has-default-export.function';
import { loadExtension } from '../load-extension.function';

export class UmbExtensionRegistry {
	private _extensions = new BehaviorSubject<Array<ManifestTypes>>([]);
	public readonly extensions = this._extensions.asObservable();

	register(manifest: ManifestTypes & { loader?: () => Promise<object | HTMLElement> }): void {
		const extensionsValues = this._extensions.getValue();
		const extension = extensionsValues.find((extension) => extension.alias === manifest.alias);

		if (extension) {
			console.error(`Extension with alias ${manifest.alias} is already registered`);
			return;
		}

		this._extensions.next([...extensionsValues, manifest]);

		// If entrypoint extension, we should load and run it immediately
		if (manifest.type === 'entrypoint') {
			loadExtension(manifest).then((js) => {
				if (hasDefaultExport(js)) {
					new js.default();
				} else {
					console.error(`Extension with alias '${manifest.alias}' of type 'entrypoint' must have a default export of its JavaScript module.`)
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

	getByAlias<T extends ManifestTypes>(alias: string): Observable<T | null>;
	getByAlias(alias: string) {
		// TODO: make pipes prettier/simpler/reuseable
		return this.extensions.pipe(map((dataTypes) => dataTypes.find((extension) => extension.alias === alias) || null));
	}

	// TODO: implement unregister of extension

	// Typings concept, need to put all core types to get a good array return type for the provided type...
	extensionsOfType(type: 'headerApp'): Observable<Array<ManifestHeaderApp>>;
	extensionsOfType(type: 'section'): Observable<Array<ManifestSection>>;
	extensionsOfType(type: 'sectionView'): Observable<Array<ManifestSectionView>>;
	extensionsOfType(type: 'tree'): Observable<Array<ManifestTree>>;
	extensionsOfType(type: 'workspace'): Observable<Array<ManifestWorkspace>>;
	extensionsOfType(type: 'treeItemAction'): Observable<Array<ManifestTreeItemAction>>;
	extensionsOfType(type: 'dashboard'): Observable<Array<ManifestDashboard>>;
	extensionsOfType(type: 'dashboardCollection'): Observable<Array<ManifestDashboard>>;
	extensionsOfType(type: 'workspaceView'): Observable<Array<ManifestWorkspaceView>>;
	extensionsOfType(type: 'workspaceAction'): Observable<Array<ManifestWorkspaceAction>>;
	extensionsOfType(type: 'propertyEditorUI'): Observable<Array<ManifestPropertyEditorUI>>;
	extensionsOfType(type: 'propertyEditorModel'): Observable<Array<ManifestPropertyEditorModel>>;
	extensionsOfType(type: 'propertyAction'): Observable<Array<ManifestPropertyAction>>;
	extensionsOfType(type: 'packageView'): Observable<Array<ManifestPackageView>>;
	extensionsOfType(type: 'entrypoint'): Observable<Array<ManifestEntrypoint>>;
	extensionsOfType(type: 'custom'): Observable<Array<ManifestCustom>>;
	extensionsOfType(type: 'externalLoginProvider'): Observable<Array<ManifestExternalLoginProvider>>;
	extensionsOfType(type: 'collectionView'): Observable<Array<ManifestCollectionView>>;
	extensionsOfType(type: 'collectionBulkAction'): Observable<Array<ManifestCollectionBulkAction>>;
	extensionsOfType<T extends ManifestTypes>(type: string): Observable<Array<T>>;
	extensionsOfType(type: string): Observable<Array<ManifestTypes>> {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => ext.type === type).sort((a, b) => (b.weight || 0) - (a.weight || 0)))
		);
	}

	extensionsOfTypes(types: string[]): Observable<Array<ManifestTypes>> {
		return this.extensions.pipe(
			map((exts) => exts.filter((ext) => (types.indexOf(ext.type) !== -1)).sort((a, b) => (b.weight || 0) - (a.weight || 0)))
		);
	}
}
