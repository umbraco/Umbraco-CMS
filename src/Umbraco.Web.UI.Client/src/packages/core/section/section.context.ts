import type { ManifestSection } from '@umbraco-cms/backoffice/section';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbSectionContext {
	#manifestAlias = new UmbStringState<string | undefined>(undefined);
	#manifestPathname = new UmbStringState<string | undefined>(undefined);
	#manifestLabel = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#manifestAlias.asObservable();
	public readonly pathname = this.#manifestPathname.asObservable();
	public readonly label = this.#manifestLabel.asObservable();

	constructor(manifest: ManifestSection) {
		this.setManifest(manifest);
	}

	public setManifest(manifest?: ManifestSection) {
		this.#manifestAlias.setValue(manifest?.alias);
		this.#manifestPathname.setValue(manifest?.meta?.pathname);
		this.#manifestLabel.setValue(manifest ? manifest.meta?.label || manifest.name : undefined);
	}

	getPathname() {
		return this.#manifestPathname.getValue();
	}
}

export const UMB_SECTION_CONTEXT = new UmbContextToken<UmbSectionContext>('UmbSectionContext');
