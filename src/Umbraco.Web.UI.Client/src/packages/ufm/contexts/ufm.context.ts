import type { ManifestUfmFilter } from '../extensions/ufm-filter.extension.js';
import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { Marked } from '@umbraco-cms/backoffice/external/marked';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { Config as DOMPurifyConfig } from '@umbraco-cms/backoffice/external/dompurify';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

const UmbDomPurify = DOMPurify(window);
const UmbDomPurifyConfig: DOMPurifyConfig = {
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
		postprocess: (markup) => UmbDomPurify.sanitize(markup, UmbDomPurifyConfig),
	},
});

export type UmbUfmFilterFunction = ((...args: Array<unknown>) => string | undefined | null) | undefined;

export type UmbUfmFilterType = {
	alias: string;
	filter: UmbUfmFilterFunction;
};

export class UmbUfmContext extends UmbContextBase {
	#filters = new UmbArrayState<UmbUfmFilterType>([], (x) => x.alias);
	public readonly filters = this.#filters.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_UFM_CONTEXT);

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'markedExtension', [UmbMarked]);

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

	/**
	 * Get the filters registered in the UFM context.
	 * @returns {Array<UmbUfmFilterType>} An array of filters with their aliases and filter functions.
	 */
	public getFilters(): Array<UmbUfmFilterType> {
		return this.#filters.getValue();
	}

	/**
	 * Get a filter by its alias.
	 * @param alias The alias of the filter to retrieve.
	 * @returns {UmbUfmFilterFunction} The filter function associated with the alias, or undefined if not found.
	 */
	public getFilterByAlias(alias: string): UmbUfmFilterFunction {
		return this.#filters.getValue().find((x) => x.alias === alias)?.filter;
	}

	/**
	 * Parse markdown content, optionally inline.
	 * @param markdown The markdown string to parse.
	 * @param inline If true, parse inline markdown; otherwise, parse block markdown.
	 * @returns {Promise<string>} A promise that resolves to the parsed HTML string.
	 */
	public async parse(markdown: string, inline: boolean): Promise<string> {
		return !inline ? await UmbMarked.parse(markdown) : await UmbMarked.parseInline(markdown);
	}
}

export const UMB_UFM_CONTEXT = new UmbContextToken<UmbUfmContext>('UmbUfmContext');

export { UmbUfmContext as api };
