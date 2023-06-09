export type variantObject = { culture?: string | null; segment?: string | null };

export class UmbVariantId {
	public static Create(variantData: variantObject): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData));
	}

	public readonly culture: string | null = null;
	public readonly segment: string | null = null;

	constructor(variantData: variantObject) {
		this.culture = (variantData.culture === 'invariant' ? null : variantData.culture) ?? null;
		this.segment = variantData.segment ?? null;
	}

	public compare(obj: variantObject): boolean {
		return this.equal(new UmbVariantId(obj));
	}

	public equal(variantId: UmbVariantId): boolean {
		return this.culture === variantId.culture && this.segment === variantId.segment;
	}

	public toString(): string {
		return (this.culture || 'invariant') + (this.segment ? `_${this.segment}` : '');
	}

	public toObject(): variantObject {
		return { culture: this.culture, segment: this.segment };
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
