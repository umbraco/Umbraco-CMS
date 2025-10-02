import type { ManifestSection } from './extensions/section.extension.js';
import { UMB_SECTION_CONTEXT } from './section.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbViewContext } from '@umbraco-cms/backoffice/view';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionContext extends UmbContextBase {
	#manifestAlias = new UmbStringState<string | undefined>(undefined);
	#manifestPathname = new UmbStringState<string | undefined>(undefined);
	#manifestLabel = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#manifestAlias.asObservable();
	public readonly pathname = this.#manifestPathname.asObservable();
	public readonly label = this.#manifestLabel.asObservable();

	#viewContext = new UmbViewContext(this, null);
	#sectionContextExtensionController?: UmbExtensionsApiInitializer<any>;

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_CONTEXT);

		this.#createSectionContextExtensions();
	}

	public setManifest(manifest?: ManifestSection) {
		this.#manifestAlias.setValue(manifest?.alias);
		this.#manifestPathname.setValue(manifest?.meta?.pathname);
		const sectionLabel = manifest ? manifest.meta?.label || manifest.name : undefined;
		this.#manifestLabel.setValue(sectionLabel);
		this.#viewContext.setTitle(sectionLabel);
	}

	getPathname() {
		return this.#manifestPathname.getValue();
	}

	#createSectionContextExtensions() {
		if (this.#sectionContextExtensionController) {
			this.#sectionContextExtensionController.destroy();
		}

		this.#sectionContextExtensionController = new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'sectionContext',
			[],
			undefined,
		);
	}
}
