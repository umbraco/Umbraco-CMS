import { ufm } from '../plugins/marked-ufm.plugin.js';
import type { UfmPlugin } from '../plugins/marked-ufm.plugin.js';
import type { ManifestUfmComponent } from '../ufm-component.extension.js';
import type { ManifestUfmFilter } from '../ufm-filter.extension.js';
import { DOMPurify, type Config } from '@umbraco-cms/backoffice/external/dompurify';
import { Marked } from '@umbraco-cms/backoffice/external/marked';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

const UmbDomPurify = DOMPurify(window);
const UmbDomPurifyConfig: Config = {
	USE_PROFILES: { html: true },
	CUSTOM_ELEMENT_HANDLING: {
		tagNameCheck: /^(?:ufm|umb|uui)-.*$/,
		attributeNameCheck: /.+/,
		allowCustomizedBuiltInElements: false,
	},
};

UmbDomPurify.addHook('afterSanitizeAttributes', function (node) {
	// set all elements owning target to target=_blank
	if ('target' in node && node instanceof HTMLElement) {
		node.setAttribute('target', '_blank');
	}
});

export const UmbMarked = new Marked({
	async: true,
	gfm: true,
	breaks: true,
	hooks: {
		postprocess: (markup) => {
			return UmbDomPurify.sanitize(markup, UmbDomPurifyConfig) as string;
		},
	},
});

type UmbUfmFilterType = {
	alias: string;
	filter: ((...args: Array<unknown>) => string | undefined | null) | undefined;
};

export class UmbUfmContext extends UmbContextBase {
	#filters = new UmbArrayState<UmbUfmFilterType>([], (x) => x.alias);
	public readonly filters = this.#filters.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_UFM_CONTEXT);

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'ufmComponent', [], undefined, (controllers) => {
			UmbMarked.use(
				ufm(
					controllers
						.map((controller) => {
							const ctrl = controller as unknown as UmbExtensionApiInitializer<ManifestUfmComponent>;
							if (!ctrl.manifest || !ctrl.api) return;
							return {
								alias: ctrl.manifest.meta.alias || ctrl.manifest.alias,
								marker: ctrl.manifest.meta.marker,
								render: ctrl.api.render,
							};
						})
						.filter((x) => x) as Array<UfmPlugin>,
				),
			);
		});

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'ufmFilter', [], undefined, (controllers) => {
			const filters = controllers
				.map((controller) => {
					const ctrl = controller as unknown as UmbExtensionApiInitializer<ManifestUfmFilter>;
					if (!ctrl.manifest || !ctrl.api) return null;
					return { alias: ctrl.manifest.meta.alias, filter: ctrl.api.filter };
				})
				.filter((x) => x) as Array<UmbUfmFilterType>;

			this.#filters.setValue(filters);
		});
	}

	public getFilterByAlias(alias: string) {
		return this.#filters.getValue().find((x) => x.alias === alias)?.filter;
	}

	public async parse(markdown: string, inline: boolean) {
		return !inline ? await UmbMarked.parse(markdown) : await UmbMarked.parseInline(markdown);
	}
}

export const UMB_UFM_CONTEXT = new UmbContextToken<UmbUfmContext>('UmbUfmContext');

export { UmbUfmContext as api };
