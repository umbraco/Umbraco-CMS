import { BehaviorSubject, map, Observable } from 'rxjs';

// TODO: how do we want to type extensions?
export type UmbExtensionType = 'startUp' | 'section' | 'propertyEditorUI' | 'dashboard';


export interface UmbExtensionManifestBase {
  type: string;
  alias: string;
  name: string;
  js?: string | (() => Promise<unknown>);
  elementName?: string;
  meta: any;
}

export type UmbExtensionManifestSection = {
  type: 'section';
  meta: UmbManifestSectionMeta;
} & UmbExtensionManifestBase;

export type UmbExtensionManifestPropertyEditor = {
  type: 'propertyEditorUI';
  meta: UmbManifestPropertyEditorMeta;
} & UmbExtensionManifestBase;

export type UmbExtensionManifestDashboard = {
  type: 'dashboard';
  meta: UmbManifestDashboardMeta;
} & UmbExtensionManifestBase;

export type UmbExtensionManifest = UmbExtensionManifestBase | UmbExtensionManifestSection | UmbExtensionManifestPropertyEditor;

export interface UmbManifestSectionMeta {
  pathname: string, // TODO: how to we want to support pretty urls?
  weight: number;
}

export interface UmbManifestPropertyEditorMeta {
  icon: string;
  groupAlias: string;
  description: string;
  configConfig: unknown;
}

export interface UmbManifestDashboardMeta {
  sections: Array<string>;
  pathname: string; // TODO: how to we want to support pretty urls?
  weight: number;
}

// TODO: consider making a UmbStore base class for...
export class UmbExtensionRegistry {

  private _extensions: BehaviorSubject<Array<UmbExtensionManifest>> = new BehaviorSubject(<Array<UmbExtensionManifest>>[]);
  public readonly extensions: Observable<Array<UmbExtensionManifest>> = this._extensions.asObservable();

  register (manifest: UmbExtensionManifest) {
    const extensionsValues = this._extensions.getValue();
    const extension = extensionsValues.find(extension => extension.alias === manifest.alias);

    if (extension) {
      console.error(`Extension with alias ${manifest.alias} is already registered`);
      return;
    }

    this._extensions.next([...extensionsValues, manifest]);
  }

  getByAlias (alias: string): Observable<UmbExtensionManifest | null> {
    // TODO: make pipes prettier/simpler/reuseable
    return this.extensions.pipe(map(((dataTypes: Array<UmbExtensionManifest>) => dataTypes.find((extension: UmbExtensionManifest) => extension.alias === alias) || null)));
  }



  // TODO: implement unregister of extension
}