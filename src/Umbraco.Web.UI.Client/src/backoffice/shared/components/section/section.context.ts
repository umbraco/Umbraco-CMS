import type { ManifestSection } from '@umbraco-cms/backoffice/extensions-registry';
import type { Entity } from '@umbraco-cms/backoffice/models';
import { ObjectState, StringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export type ActiveTreeItemType = Entity | undefined;

export class UmbSectionContext {
	#manifestAlias = new StringState<string | undefined>(undefined);
	#manifestPathname = new StringState<string | undefined>(undefined);
	#manifestLabel = new StringState<string | undefined>(undefined);
	public readonly alias = this.#manifestAlias.asObservable();
	public readonly pathname = this.#manifestPathname.asObservable();
	public readonly label = this.#manifestLabel.asObservable();

	constructor(manifest: ManifestSection) {
		this.setManifest(manifest);
	}

	public setManifest(manifest?: ManifestSection) {
		this.#manifestAlias.next(manifest?.alias);
		this.#manifestPathname.next(manifest?.meta?.pathname);
		this.#manifestLabel.next(manifest ? manifest.meta?.label || manifest.name : undefined);
	}
}

export const UMB_SECTION_CONTEXT_TOKEN = new UmbContextToken<UmbSectionContext>('UmbSectionContext');
