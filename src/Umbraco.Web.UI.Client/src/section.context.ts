import { firstValueFrom, map, Observable, ReplaySubject } from 'rxjs';
import { UmbExtensionManifest, UmbExtensionRegistry, UmbManifestSectionMeta } from './core/extension';

export class UmbSectionContext {
  private _extensionRegistry!: UmbExtensionRegistry;

  private _current: ReplaySubject<UmbExtensionManifest<UmbManifestSectionMeta>> = new ReplaySubject(1);
  public readonly current: Observable<UmbExtensionManifest<UmbManifestSectionMeta>> = this._current.asObservable();

  constructor(_extensionRegistry: UmbExtensionRegistry) {
    this._extensionRegistry = _extensionRegistry;
  }

  getSections () {
    return this._extensionRegistry.extensions
      .pipe(
        map((extensions: Array<UmbExtensionManifest<unknown>>) => extensions.filter(extension => extension.type === 'section'))
      );
  }

  getCurrent () {
    return this.current;
  }

  async setCurrent (sectionAlias: string) {
    const sections = await firstValueFrom(this.getSections());
    const matchedSection = sections.find(section => section.alias === sectionAlias) as UmbExtensionManifest<UmbManifestSectionMeta>;
    this._current.next(matchedSection);
  }

}