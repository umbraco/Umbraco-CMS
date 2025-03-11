import { UmbUfmElementBase } from '../ufm-element-base.js';
import { UMB_UFM_RENDER_CONTEXT } from '../ufm-render/ufm-render.context.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentItemRepository, UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbMediaItemRepository, UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import { UmbMemberItemRepository, UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';

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
							? (value as Record<string, unknown>)[this.alias]
							: (value as unknown);

					if (!temp) return;

					const entityType = this.#getEntityType(temp);
					const uniques = this.#getUniques(temp);

					this.value = await this.#getNames(entityType, uniques);
				},
				'observeValue',
			);
		});
	}

	#getEntityType(value: unknown) {
		if (Array.isArray(value) && value.length > 0) {
			const item = value[0];
			if (item.type) return item.type;
			if (item.mediaKey) return UMB_MEDIA_ENTITY_TYPE;
		}

		return null;
	}

	#getUniques(value: unknown) {
		if (Array.isArray(value)) {
			return value.map((x) => x.unique ?? x.mediaKey ?? x).filter((x) => UmbId.validate(x));
		}

		return typeof value === 'string' && UmbId.validate(value) ? [value] : [];
	}

	async #getNames(entityType: string, uniques?: Array<string>) {
		if (uniques?.length) {
			const repository = this.#getRepository(entityType);
			if (repository) {
				const { data } = await repository.requestItems(uniques);
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

			case UMB_MEMBER_ENTITY_TYPE:
				if (!this.#memberRepository) this.#memberRepository = new UmbMemberItemRepository(this);
				return this.#memberRepository;

			case UMB_DOCUMENT_ENTITY_TYPE:
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
