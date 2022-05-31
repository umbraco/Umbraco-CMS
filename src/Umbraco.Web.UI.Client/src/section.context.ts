import { firstValueFrom, map, Observable, ReplaySubject } from 'rxjs';
import { UmbExtensionManifest, UmbExtensionRegistry } from './core/extension';

export class UmbSectionContext {
  private _extensionRegistry!: UmbExtensionRegistry;

  private _current: ReplaySubject<UmbExtensionManifest> = new ReplaySubject(1);
  public readonly current: Observable<UmbExtensionManifest> = this._current.asObservable();

  constructor(_extensionRegistry: UmbExtensionRegistry) {
    this._extensionRegistry = _extensionRegistry;
  }

  getSections () {
    return this._extensionRegistry.extensions
      .pipe(
        map((extensions: Array<UmbExtensionManifest>) => extensions
          .filter(extension => extension.type === 'section')
          .sort((a: any, b: any) => b.meta.weight - a.meta.weight))
      );
  }

  getCurrent () {
    return this.current;
  }

  async setCurrent (sectionAlias: string) {
    const sections = await firstValueFrom(this.getSections());
    const matchedSection = sections.find(section => section.alias === sectionAlias) as UmbExtensionManifest;
    this._current.next(matchedSection);
  }

}