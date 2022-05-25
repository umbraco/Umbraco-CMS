import { BehaviorSubject, map, Observable } from 'rxjs';

// TODO: how do we want to type extensions?
export type UmbExtensionType = 'startUp' | 'section' | 'propertyEditor';

export interface UmbExtensionManifestBase {
  //type: string;
  alias: string;
  name: string;
  js?: string;
  elementName?: string;
  //meta: undefined;
}


// Core manifest types:

// Section:

export type UmbExtensionManifestSection = {
  type: 'section';
  meta: UmbManifestSectionMeta;
} & UmbExtensionManifestBase;

export type UmbManifestSectionMeta = {
  weight: number;
}

// propertyEditor:

export type UmbExtensionManifestPropertyEditor = {
  type: 'propertyEditor';
  meta: UmbManifestPropertyEditorMeta;
} & UmbExtensionManifestBase;

export type UmbManifestPropertyEditorMeta = {
  icon: string;
  groupAlias: string;
  description: string;
  configConfig: unknown;
}

export type UmbExtensionManifestCore = 
UmbExtensionManifestSection | 
UmbExtensionManifestPropertyEditor;

// Other manifest type:

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

  register<T extends UmbExtensionManifest = UmbExtensionManifestCore>(manifest: T) {
    const extensionsValues = this._extensions.getValue();
    const extension = extensionsValues.find(extension => extension.alias === manifest.alias);

    if (extension) {
      console.error(`Extension with alias ${manifest.alias} is already registered`);
      return;
    }

    this._extensions.next([...extensionsValues, manifest]);
  }

  // TODO: implement unregister of extension

  // Typings concept, need to evaluate...
  extensionsOfType(type: 'section'): Observable<Array<UmbExtensionManifestSection>>;
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