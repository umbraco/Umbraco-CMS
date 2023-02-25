export class UmbVariantId {
	public static Create(culture: string | null, segment: string | null) {
		return Object.freeze(new UmbVariantId(culture, segment));
	}

	public readonly culture: string | null = null;
	public readonly segment: string | null = null;

	// prettier-ignore
	constructor(culture: string | null, segment: string | null) {
		this.culture = culture || null;
		this.segment = segment || null;
	}

	public compare(obj: { culture?: string | null; segment?: string | null }): boolean {
		return this.culture === (obj.culture || null) && this.segment === (obj.segment || null);
	}

	public equal(variantId: UmbVariantId): boolean {
		return this.culture === variantId.culture && this.segment === variantId.segment;
	}

	public toString(): string {
		return (this.culture || 'invariant') + (this.segment ? `_${this.segment}` : '');
	}
}
