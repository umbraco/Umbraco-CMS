import type { UfmPlugin } from '../../plugins/marked-ufm.plugin.js';
import { ufm } from '../../plugins/marked-ufm.plugin.js';
import { UmbUfmRenderContext } from './ufm-render.context.js';
import { css, customElement, nothing, property, unsafeHTML, until } from '@umbraco-cms/backoffice/external/lit';
import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { Marked } from '@umbraco-cms/backoffice/external/marked';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestUfmComponent } from '@umbraco-cms/backoffice/extension-registry';

const UmbDomPurify = DOMPurify(window);
const UmbDomPurifyConfig: DOMPurify.Config = {
	USE_PROFILES: { html: true },
	CUSTOM_ELEMENT_HANDLING: {
		tagNameCheck: /^(?:ufm|umb|uui)-.*$/,
		attributeNameCheck: /.+/,
		allowCustomizedBuiltInElements: false,
	},
};

UmbDomPurify.addHook('afterSanitizeAttributes', function (node) {
	// set all elements owning target to target=_blank
	if ('target' in node) {
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

const elementName = 'umb-ufm-render';

@customElement(elementName)
export class UmbUfmRenderElement extends UmbLitElement {
	#context = new UmbUfmRenderContext(this);

	@property({ type: Boolean })
	inline = false;

	@property()
	markdown?: string;

	// No reactive property declaration cause its causing a re-render that is not needed. This just works as a shortcut to set the values on the context. [NL]
	public set value(value: string | unknown | undefined) {
		this.#context.setValue(value);
	}
	public get value(): string | unknown | undefined {
		return this.#context.getValue();
	}

	constructor() {
		super();

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'ufmComponent', [], undefined, (controllers) => {
			UmbMarked.use(
				ufm(
					controllers
						.map((controller) => {
							const ctrl = controller as unknown as UmbExtensionApiInitializer<ManifestUfmComponent>;
							if (!ctrl.manifest || !ctrl.api) return;
							return {
								alias: ctrl.manifest.alias,
								marker: ctrl.manifest.meta.marker,
								render: ctrl.api.render,
							};
						})
						.filter((x) => x) as Array<UfmPlugin>,
				),
			);
			this.requestUpdate('markdown');
		});
	}

	override render() {
		return until(this.#renderMarkdown());
	}

	async #renderMarkdown() {
		if (!this.markdown) return null;
		const markup = !this.inline ? await UmbMarked.parse(this.markdown) : await UmbMarked.parseInline(this.markdown);
		return markup ? unsafeHTML(markup) : nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			* {
				max-width: 100%;
				word-wrap: break-word;
			}

			pre {
				overflow: auto;
			}
		`,
	];
}

export { UmbUfmRenderElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUfmRenderElement;
	}
}
