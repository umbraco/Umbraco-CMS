import { UmbUfmElementBase } from '../ufm-element-base.js';
import { UMB_UFM_RENDER_CONTEXT } from '../ufm-render/ufm-render.context.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentItemRepository, UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import type { UmbLinkPickerLink } from '@umbraco-cms/backoffice/multi-url-picker';

const elementName = 'ufm-link';

@customElement(elementName)
export class UmbUfmLinkElement extends UmbUfmElementBase {
	@property()
	alias?: string;

	#documentRepository?: UmbDocumentItemRepository;
	#mediaRepository?: UmbMediaItemRepository;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
			this.observe(
				context.value,
				async (value) => {
					const temp =
						this.alias && typeof value === 'object'
							? (value as Record<string, unknown>)[this.alias]
							: (value as unknown);

					if (!temp) return;

					const items = Array.isArray(temp) ? temp : [temp];
					const names = await Promise.all(items.map(async (item) => await this.#getName(item)));
					this.value = names.filter((x) => x).join(', ');
				},
				'observeValue',
			);
		});
	}

	async #getName(item?: unknown) {
		const link = item as UmbLinkPickerLink;

		if (link.name) {
			return link.name;
		}

		const entityType = link.type;
		const unique = link.unique;

		if (unique) {
			const repository = this.#getRepository(entityType);
			if (repository) {
				const { data } = await repository.requestItems([unique]);
				if (Array.isArray(data) && data.length > 0) {
					return data.map((item) => item.name).join(', ');
				}
			}
		}

		return '';
	}

	#getRepository(entityType?: string | null) {
		switch (entityType) {
			case UMB_MEDIA_ENTITY_TYPE:
				if (!this.#mediaRepository) this.#mediaRepository = new UmbMediaItemRepository(this);
				return this.#mediaRepository;

			case UMB_DOCUMENT_ENTITY_TYPE:
			default:
				if (!this.#documentRepository) this.#documentRepository = new UmbDocumentItemRepository(this);
				return this.#documentRepository;
		}
	}
}

export { UmbUfmLinkElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUfmLinkElement;
	}
}
