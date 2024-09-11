import { UmbUfmElementBase } from '../ufm-element-base.js';
import { UMB_UFM_RENDER_CONTEXT } from '../ufm-render/ufm-render.context.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentItemRepository } from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository } from '@umbraco-cms/backoffice/media';
import { UmbMemberItemRepository } from '@umbraco-cms/backoffice/member';

const elementName = 'ufm-content-name';

@customElement(elementName)
export class UmbUfmContentNameElement extends UmbUfmElementBase {
	@property()
	alias?: string;

	#documentRepository?: UmbDocumentItemRepository;
	#mediaRepository?: UmbMediaItemRepository;
	#memberRepository?: UmbMemberItemRepository;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
			this.observe(
				context.value,
				async (value) => {
					const temp =
						this.alias && typeof value === 'object'
							? ((value as Record<string, unknown>)[this.alias] as string)
							: (value as string);

					const entityType = Array.isArray(temp) && temp.length > 0 ? temp[0].type : null;
					const uniques = Array.isArray(temp) ? temp.map((x) => x.unique) : temp ? [temp] : [];

					if (uniques?.length) {
						const repository = this.#getRepository(entityType);
						if (repository) {
							const { data } = await repository.requestItems(uniques);
							this.value = data ? data.map((item) => item.name).join(', ') : '';
							return;
						}
					}

					this.value = '';
				},
				'observeValue',
			);
		});
	}

	#getRepository(entityType?: string | null) {
		switch (entityType) {
			case 'media':
				if (!this.#mediaRepository) this.#mediaRepository = new UmbMediaItemRepository(this);
				return this.#mediaRepository;

			case 'member':
				if (!this.#memberRepository) this.#memberRepository = new UmbMemberItemRepository(this);
				return this.#memberRepository;

			case 'document':
			default:
				if (!this.#documentRepository) this.#documentRepository = new UmbDocumentItemRepository(this);
				return this.#documentRepository;
		}
	}
}

export { UmbUfmContentNameElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUfmContentNameElement;
	}
}
