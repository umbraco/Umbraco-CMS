export class UmbVariantId {
	public static Create(variantData: { culture?: string | null; segment?: string | null }): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData));
	}

	public readonly culture: string | null = null;
	public readonly segment: string | null = null;

	constructor(variantData: { culture?: string | null; segment?: string | null }) {
		this.culture = variantData.culture || null;
		this.segment = variantData.segment || null;
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

	public toDifferencesString(variantId: UmbVariantId): string {
		let r = '';

		if (variantId.culture !== this.culture) {
			r = 'Invariant';
		}

		if (variantId.segment !== this.segment) {
			r = (r !== '' ? ' ' : '') + 'Unsegmented';
		}

		return r;
	}
}
