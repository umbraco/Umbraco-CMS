export class UmbVariantId {
	// prettier-ignore
	constructor(
		public culture: string | null = null,
		public segment: string | null = null
	) {}

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
