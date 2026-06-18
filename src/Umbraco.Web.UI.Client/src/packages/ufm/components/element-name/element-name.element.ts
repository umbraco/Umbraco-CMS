import { UmbUfmElementBase } from '../ufm-element-base.js';
import { UMB_UFM_RENDER_CONTEXT } from '../ufm-render/ufm-render.context.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementItemDataResolver, UmbElementItemRepository } from '@umbraco-cms/backoffice/element';
import { UmbId } from '@umbraco-cms/backoffice/id';

@customElement('ufm-element-name')
export class UmbUfmElementNameElement extends UmbUfmElementBase {
	#repository?: UmbElementItemRepository;

	@property()
	alias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
			this.observe(
				context?.value,
				async (value) => {
					const temp =
						this.alias && typeof value === 'object'
							? (value as Record<string, unknown>)[this.alias]
							: (value as unknown);

					if (!temp) return;

					const uniques = this.#getUniques(temp);
					this.value = await this.#getNames(uniques);
				},
				'observeValue',
			);
		});
	}

	#getUniques(value: unknown) {
		if (Array.isArray(value)) {
			return value.map((x) => x.unique ?? x).filter((x) => UmbId.validate(x));
		}
		return typeof value === 'string' && UmbId.validate(value) ? [value] : [];
	}

	async #getNames(uniques?: Array<string>) {
		if (uniques?.length) {
			if (!this.#repository) this.#repository = new UmbElementItemRepository(this);
			const { data } = await this.#repository.requestItems(uniques);

			if (Array.isArray(data) && data.length > 0) {
				const namePromises = data.map(async (item) => {
					const resolver = new UmbElementItemDataResolver(this);
					resolver.setData(item);
					return await resolver.getName();
				});
				const names = await Promise.all(namePromises);
				return names.join(', ');
			}
		}
		return '';
	}
}

export { UmbUfmElementNameElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'ufm-element-name': UmbUfmElementNameElement;
	}
}
