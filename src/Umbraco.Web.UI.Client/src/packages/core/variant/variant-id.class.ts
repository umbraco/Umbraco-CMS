export type variantObject = { culture?: string | null; segment?: string | null };

export const INVARIANT_CULTURE = 'invariant';

export class UmbVariantId {
	public static Create(variantData: variantObject): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData));
	}

	public readonly culture: string | null = null;
	public readonly segment: string | null = null;

	constructor(variantData: variantObject) {
		this.culture = (variantData.culture === INVARIANT_CULTURE ? null : variantData.culture) ?? null;
		this.segment = variantData.segment ?? null;
	}

	public compare(obj: variantObject): boolean {
		return this.equal(new UmbVariantId(obj));
	}

	public equal(variantId: UmbVariantId): boolean {
		return this.culture === variantId.culture && this.segment === variantId.segment;
	}

	public toString(): string {
		return (this.culture || INVARIANT_CULTURE) + (this.segment ? `_${this.segment}` : '');
	}

	public toCultureString(): string {
		return (this.culture || INVARIANT_CULTURE);
	}

	public toSegmentString(): string {
		return (this.segment || '');
	}

	public isInvariant(): boolean {
		return this.culture === null && this.segment === null;
	}

	public toObject(): variantObject {
		return { culture: this.culture, segment: this.segment };
	}

	// TODO: needs localization option:
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
