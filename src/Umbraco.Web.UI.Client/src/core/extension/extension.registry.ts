import { BehaviorSubject, map, Observable } from 'rxjs';

import type {
	ManifestCore,
	ManifestDashboard,
	ManifestEditorView,
	ManifestEntrypoint,
	ManifestPropertyAction,
	ManifestPropertyEditorUI,
	ManifestSection,
} from '../models';
export class UmbExtensionRegistry {
	private _extensions = new BehaviorSubject<Array<ManifestCore>>([]);
	public readonly extensions = this._extensions.asObservable();

	register(manifest: ManifestCore): void {
		const extensionsValues = this._extensions.getValue();
		const extension = extensionsValues.find((extension) => extension.alias === manifest.alias);

		if (extension) {
			console.error(`Extension with alias ${manifest.alias} is already registered`);
			return;
		}

		this._extensions.next([...extensionsValues, manifest]);
	}

	getByAlias(alias: string): Observable<ManifestCore | null> {
		// TODO: make pipes prettier/simpler/reuseable
		return this.extensions.pipe(map((dataTypes) => dataTypes.find((extension) => extension.alias === alias) || null));
	}

	// TODO: implement unregister of extension

	// Typings concept, need to put all core types to get a good array return type for the provided type...
	extensionsOfType(type: 'section'): Observable<Array<ManifestSection>>;
	extensionsOfType(type: 'dashboard'): Observable<Array<ManifestDashboard>>;
	extensionsOfType(type: 'editorView'): Observable<Array<ManifestEditorView>>;
	extensionsOfType(type: 'propertyEditorUI'): Observable<Array<ManifestPropertyEditorUI>>;
	extensionsOfType(type: 'propertyAction'): Observable<Array<ManifestPropertyAction>>;
	extensionsOfType(type: 'entrypoint'): Observable<Array<ManifestEntrypoint>>;
	extensionsOfType(type: string): Observable<Array<ManifestCore>>;
	extensionsOfType<T extends ManifestCore>(type: string): Observable<Array<T>>;
	extensionsOfType(type: string) {
		return this.extensions.pipe(map((exts) => exts.filter((ext) => ext.type === type)));
	}
}
