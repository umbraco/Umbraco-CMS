import { BehaviorSubject, map, Observable } from 'rxjs';

// TODO: how do we want to type extensions?
export type UmbExtensionType = 'startUp' | 'section' | 'propertyEditorUI' | 'dashboard';

export type UmbExtensionManifestBase = {
  //type: string;
  alias: string;
  name: string;
  js?: string | (() => Promise<unknown>);
  elementName?: string;
  //meta: undefined;
}


// Core manifest types:

// Section:
export type UmbManifestSectionMeta = {
  pathname: string, // TODO: how to we want to support pretty urls?
  weight: number;
}
export type UmbExtensionManifestSection = {
  type: 'section';
  meta: UmbManifestSectionMeta;
} & UmbExtensionManifestBase;


// propertyEditor:
export type UmbManifestPropertyEditorMeta = {
  icon: string;
  group: string; // TODO: use group alias or other name to indicate that it could be used to look up translation.
  //groupAlias: string;
  //description: string;
  //configConfig: unknown; // we need a name and concept for how to setup editor-UI for 
}
export type UmbExtensionManifestPropertyEditor = {
  type: 'propertyEditorUI';
  meta: UmbManifestPropertyEditorMeta;
} & UmbExtensionManifestBase;



// Dashboard:
export type UmbManifestDashboardMeta = {
  sections: Array<string>;
  pathname: string; // TODO: how to we want to support pretty urls?
  weight: number;
}
export type UmbExtensionManifestDashboard = {
  type: 'dashboard';
  meta: UmbManifestDashboardMeta;
} & UmbExtensionManifestBase;



export type UmbExtensionManifestCore = 
UmbExtensionManifestSection | 
UmbExtensionManifestDashboard |
UmbExtensionManifestPropertyEditor;



// the 'Other' manifest type:

type UmbExtensionManifestOther = 
{
  type: string;
  meta: unknown;
} & UmbExtensionManifestBase;

export type UmbExtensionManifest = 
UmbExtensionManifestCore |
UmbExtensionManifestOther;

type UmbExtensionManifestCoreTypes = Pick<UmbExtensionManifestCore, 'type'>['type'];




export class UmbExtensionRegistry {

  private _extensions: BehaviorSubject<Array<UmbExtensionManifest>> = new BehaviorSubject(<Array<UmbExtensionManifest>>[]);
  public readonly extensions: Observable<Array<UmbExtensionManifest>> = this._extensions.asObservable();

  register<T extends UmbExtensionManifest = UmbExtensionManifestCore>(manifest: T):void {
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

  // Typings concept, need to put all core types to get a good array return type for the provided type...
  extensionsOfType(type: 'section'): Observable<Array<UmbExtensionManifestSection>>;
  extensionsOfType(type: 'dashboard'): Observable<Array<UmbExtensionManifestDashboard>>;
  extensionsOfType(type: 'propertyEditor'): Observable<Array<UmbExtensionManifestPropertyEditor>>;
  extensionsOfType(type: UmbExtensionManifestCoreTypes): Observable<Array<UmbExtensionManifestCore>>;
  extensionsOfType(type: string): Observable<Array<UmbExtensionManifestOther>>;
  extensionsOfType(type: string) {
    return this.extensions.pipe(
      map((exts: Array<UmbExtensionManifest>) => exts
        .filter(ext => ext.type === type)
    ))
  }
}