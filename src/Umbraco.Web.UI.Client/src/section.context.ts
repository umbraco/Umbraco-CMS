import { firstValueFrom, map, ReplaySubject } from 'rxjs';

import { UmbExtensionManifestSection, UmbExtensionRegistry } from './core/extension';

export class UmbSectionContext {
  private _extensionRegistry!: UmbExtensionRegistry;

  private _current = new ReplaySubject<UmbExtensionManifestSection>(1);
  public readonly current = this._current.asObservable();

  constructor(_extensionRegistry: UmbExtensionRegistry) {
    this._extensionRegistry = _extensionRegistry;
  }

  getSections() {
    return this._extensionRegistry.extensions.pipe(
      map((extensions) =>
        extensions
          .filter((extension) => extension.type === 'section')
          .map((extension) => extension as UmbExtensionManifestSection)
          .sort((a, b) => b.meta.weight - a.meta.weight)
      )
    );
  }

  getCurrent() {
    return this.current;
  }

  async setCurrent(sectionAlias: string) {
    const sections = await firstValueFrom(this.getSections());
    const matchedSection = sections.find((section) => section.alias === sectionAlias);
    if (matchedSection) {
      this._current.next(matchedSection);
    }
  }
}
