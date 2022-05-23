import { BehaviorSubject, Observable } from 'rxjs';

// TODO: how do we want to type extensions?
export type UmbExtensionType = 'startUp' | 'section' | 'propertyEditor';

export interface UmbExtensionManifest<Meta> {
  type: UmbExtensionType;
  alias: string;
  name: string;
  js?: string;
  elementName?: string;
  meta: Meta;
}

export interface UmbManifestSectionMeta {
  weight: number;
}

export class UmbExtensionRegistry {
  private _extensions: BehaviorSubject<Array<UmbExtensionManifest<unknown>>> = new BehaviorSubject(<Array<UmbExtensionManifest<unknown>>>[]);
  public readonly extensions: Observable<Array<UmbExtensionManifest<unknown>>> = this._extensions.asObservable();

  register (manifest: UmbExtensionManifest<unknown>) {
    const extensions = this._extensions.getValue();
    const extension = extensions.find(extension => extension.alias === manifest.alias);

    if (extension) {
      console.error(`Extension with alias ${manifest.alias} is already registered`);
      return;
    }

    this._extensions.next([...extensions, manifest]);
  }

  // TODO: implement unregister of extension
}