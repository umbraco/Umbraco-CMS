import { BehaviorSubject, map, Observable } from 'rxjs';

import { createExtensionElement } from './create-extension-element.function';

export type UmbExtensionManifestJSModel = {
	elementName?: string;
};

import type {
	ManifestTypes,
	ManifestDashboard,
	ManifestEditorView,
	ManifestEntrypoint,
	ManifestPropertyAction,
	ManifestPropertyEditorUI,
	ManifestSection,
	ManifestTree,
	ManifestTreeItemAction,
	ManifestEditor,
	ManifestCustom,
	ManifestPackageView,
} from '../models';

// TODO: add to schema
export type ManifestBase = {
	alias: string;
	name: string;
	js?: string | (() => Promise<unknown>);
	elementName?: string;
};

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

		// If entrypoint extension, we should load it immediately
		if (manifest.type === 'entrypoint') {
			createExtensionElement(manifest);
		}
	}

	getByAlias<T extends ManifestTypes>(alias: string): Observable<T | null>;
	getByAlias(alias: string) {
		// TODO: make pipes prettier/simpler/reuseable
		return this.extensions.pipe(map((dataTypes) => dataTypes.find((extension) => extension.alias === alias) || null));
	}

	// TODO: implement unregister of extension

	// Typings concept, need to put all core types to get a good array return type for the provided type...
	extensionsOfType(type: 'section'): Observable<Array<ManifestSection>>;
	extensionsOfType(type: 'tree'): Observable<Array<ManifestTree>>;
	extensionsOfType(type: 'editor'): Observable<Array<ManifestEditor>>;
	extensionsOfType(type: 'treeItemAction'): Observable<Array<ManifestTreeItemAction>>;
	extensionsOfType(type: 'dashboard'): Observable<Array<ManifestDashboard>>;
	extensionsOfType(type: 'editorView'): Observable<Array<ManifestEditorView>>;
	extensionsOfType(type: 'propertyEditorUI'): Observable<Array<ManifestPropertyEditorUI>>;
	extensionsOfType(type: 'propertyAction'): Observable<Array<ManifestPropertyAction>>;
	extensionsOfType(type: 'packageView'): Observable<Array<ManifestPackageView>>;
	extensionsOfType(type: 'entrypoint'): Observable<Array<ManifestEntrypoint>>;
	extensionsOfType(type: 'custom'): Observable<Array<ManifestCustom>>;
	extensionsOfType<T extends ManifestTypes>(type: string): Observable<Array<T>>;
	extensionsOfType(type: string): Observable<Array<ManifestTypes>> {
		return this.extensions.pipe(map((exts) => exts.filter((ext) => ext.type === type)));
	}
}
