import { UmbUfmElementBase } from '../ufm-element-base.js';
import { UMB_UFM_RENDER_CONTEXT } from '../ufm-render/ufm-render.context.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { getGuidFromUdi } from '@umbraco-cms/backoffice/utils';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbMemberItemRepository } from '@umbraco-cms/backoffice/member';

@customElement('ufm-member-name')
export class UmbUfmMemberNameElement extends UmbUfmElementBase {
	@property()
	alias?: string;

	#memberRepository?: UmbMemberItemRepository;

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

					if (!temp) {
						this.value = '';
						return;
					}

					const uniques = this.#getUniques(temp);

					this.value = await this.#getNames(uniques);
				},
				'observeValue',
			);
		});
	}

	#getUniques(value: unknown): Array<string> {
		const entries = Array.isArray(value) ? value : [value];
		return entries.flatMap((entry) => this.#normalizeEntry(entry)).filter((x) => x.length > 0);
	}

	// Returns an array because a single string entry may be comma-separated.
	#normalizeEntry(entry: unknown): Array<string> {
		if (entry && typeof entry === 'object') {
			const unique = (entry as Record<string, unknown>).unique;
			return typeof unique === 'string' ? this.#normalizeEntry(unique) : [];
		}
		if (typeof entry === 'string') {
			return entry
				.split(',')
				.map((token) => this.#extractGuid(token.trim()))
				.filter((x): x is string => !!x);
		}
		return [];
	}

	#extractGuid(token: string): string | undefined {
		if (token.startsWith('umb://')) {
			try {
				const guid = getGuidFromUdi(token);
				return UmbId.validate(guid) ? guid : undefined;
			} catch {
				return undefined;
			}
		}
		return UmbId.validate(token) ? token : undefined;
	}

	async #getNames(uniques?: Array<string>) {
		if (uniques?.length) {
			if (!this.#memberRepository) {
				this.#memberRepository = new UmbMemberItemRepository(this);
			}

			const { data } = await this.#memberRepository.requestItems(uniques);
			if (Array.isArray(data) && data.length > 0) {
				// TODO: Review usage of `item.variants[0].name` as this needs to be implemented properly for member items [LK]
				return data.map((item) => item.variants[0]?.name ?? item.name).join(', ');
			}
		}

		return '';
	}
}

export { UmbUfmMemberNameElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'ufm-member-name': UmbUfmMemberNameElement;
	}
}
