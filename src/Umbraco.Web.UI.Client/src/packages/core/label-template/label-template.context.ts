import { createDOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { Marked } from '@umbraco-cms/backoffice/external/marked';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import type { MarkedOptions } from '@umbraco-cms/backoffice/external/marked';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export class UmbLabelTemplateContext extends UmbContextBase<UmbLabelTemplateContext> {
	#hostElement?: UmbLitElement;
	#marked;
	#domPurify: DOMPurify.DOMPurifyI;
	#domPurifyConfig: DOMPurify.Config;

	constructor(host: UmbControllerHost) {
		super(host, UMB_LABEL_TEMPLATE_CONTEXT);

		this.#marked = new Marked({ gfm: true, breaks: true });

		this.#domPurifyConfig = { USE_PROFILES: { html: true } };

		this.#domPurify = createDOMPurify(window);

		this.#domPurify.addHook('afterSanitizeAttributes', function (node) {
			// set all elements owning target to target=_blank
			if ('target' in node) {
				node.setAttribute('target', '_blank');
			}
		});
	}

	public setHostElement(hostElement: UmbLitElement): void {
		this.#hostElement = hostElement;
	}

	public transform(input: string): string | null | undefined {
		return this.#transform(input, this.#marked.parse);
	}

	public transformInline(input: string): string | null | undefined {
		return this.#transform(input, this.#marked.parseInline);
	}

	#transform(
		input: string,
		markdownParse?: (src: string, options?: MarkedOptions | undefined | null) => string | Promise<string>,
	): string | null | undefined {
		if (!input) return undefined;

		const localized = this.#hostElement ? this.#hostElement.localize.string(input) : input;
		const markdowned = markdownParse ? (markdownParse(localized) as string) : localized;
		const sanitized = markdowned ? (this.#domPurify.sanitize(markdowned, this.#domPurifyConfig) as string) : localized;

		return sanitized ?? input;
	}
}

export const UMB_LABEL_TEMPLATE_CONTEXT = new UmbContextToken<UmbLabelTemplateContext>('UmbLabelTemplateContext');

// Default export to enable this as a globalContext extension js:
export default UmbLabelTemplateContext;
