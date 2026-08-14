import type { UmbEntityMockDbBase } from './entity-base.js';

type VariantModel = { culture?: string | null; name: string };

type UrlInfo = { culture: string | null; url: string };

export class UmbMockEntityVariantUrlManager<
	T extends { id: string; parent?: { id: string } | null; variants: Array<VariantModel> },
> {
	#db: UmbEntityMockDbBase<T>;

	constructor(mockDb: UmbEntityMockDbBase<T>) {
		this.#db = mockDb;
	}

	/**
	 * Get the URLs for an entity by its id.
	 * Returns an array of URL info for each variant (culture).
	 * @param id
	 */
	getUrls(id: string): Array<UrlInfo> {
		const item = this.#db.read(id);
		if (!item) return [];

		const cultures = this.#getUniqueCultures(item.variants);

		return cultures.map((culture) => ({
			culture,
			url: this.#buildUrlForCulture(id, culture),
		}));
	}

	/**
	 * Get unique cultures from variants.
	 * @param variants
	 */
	#getUniqueCultures(variants: Array<VariantModel>): Array<string | null> {
		const cultures = new Set<string | null>();
		for (const variant of variants) {
			cultures.add(variant.culture ?? null);
		}
		return Array.from(cultures);
	}

	/**
	 * Build the URL for a specific culture by traversing ancestors.
	 * @param id
	 * @param culture
	 */
	#buildUrlForCulture(id: string, culture: string | null): string {
		const ancestors = this.#getAncestorsOf(id);
		const segments = ancestors.map((item) => {
			const name = this.#getNameForCulture(item.variants, culture);
			return this.#toSlug(name);
		});
		return '/' + segments.join('/');
	}

	/**
	 * Get the name for a specific culture from variants.
	 * Falls back to first variant if culture not found.
	 * @param variants
	 * @param culture
	 */
	#getNameForCulture(variants: Array<VariantModel>, culture: string | null): string {
		const variant = variants.find((v) => (v.culture ?? null) === culture);
		return variant?.name ?? variants[0]?.name ?? '';
	}

	/**
	 * Get all ancestors of an entity including the entity itself.
	 * Returns array ordered from root to the entity.
	 * @param id
	 */
	#getAncestorsOf(id: string): Array<T> {
		const items: Array<T> = [];
		let currentId: string | undefined = id;

		while (currentId) {
			const item = this.#db.read(currentId);
			if (!item) break;
			items.push(item);
			currentId = item.parent?.id;
		}

		return items.reverse();
	}

	/**
	 * Convert a name to a URL-friendly slug.
	 * @param name
	 */
	#toSlug(name: string): string {
		return name
			.toLowerCase()
			.replace(/\s+/g, '-') // Replace spaces with hyphens
			.replace(/[^a-z0-9-]/g, '') // Remove non-alphanumeric characters except hyphens
			.replace(/-+/g, '-') // Replace multiple hyphens with single hyphen
			.replace(/^-|-$/g, ''); // Remove leading/trailing hyphens
	}
}
