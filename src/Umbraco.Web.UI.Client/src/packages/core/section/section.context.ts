import type { ManifestSection } from './extensions/section.extension.js';
import { UMB_SECTION_CONTEXT } from './section.context.token.js';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbSectionContext extends UmbContextBase {
	#manifestAlias = new UmbStringState<string | undefined>(undefined);
	#manifestPathname = new UmbStringState<string | undefined>(undefined);
	#manifestLabel = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#manifestAlias.asObservable();
	public readonly pathname = this.#manifestPathname.asObservable();
	public readonly label = this.#manifestLabel.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_CONTEXT);
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
