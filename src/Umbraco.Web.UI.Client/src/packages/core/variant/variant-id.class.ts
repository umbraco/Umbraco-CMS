import { UmbDeprecation } from '../utils/index.js';
import type { UmbObjectWithVariantProperties } from './types.js';

/**
 *
 * @param {UmbObjectWithVariantProperties} variant - An object with variant specifying properties to convert to a string.
 * @returns {string} A string representation of the variant properties.
 * @deprecated Use UmbVariantId to make this conversion. Will ve removed in v.17
 */
export function variantPropertiesObjectToString(variant: UmbObjectWithVariantProperties): string {
	new UmbDeprecation({
		deprecated: 'Method `variantPropertiesObjectToString` is deprecated',
		removeInVersion: '17',
		solution: 'Use UmbVariantId to make this conversion',
	}).warn();
	// Currently a direct copy of the toString method of variantId.
	return (variant.culture || UMB_INVARIANT_CULTURE) + (variant.segment ? `_${variant.segment}` : '');
}

export const UMB_INVARIANT_CULTURE = 'invariant';

/**
 * An identifier representing a Variant. This is at current state a culture and a segment.
 * The identifier is not specific for ContentType Variants, but is used for many type of identification of a culture and a segment. One case is any property of a ContentType can be resolved into a VariantId depending on their structural settings such as Vary by Culture and Vary by Segmentation.
 */
export class UmbVariantId {
	public static Create(variantData: UmbObjectWithVariantProperties): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData.culture, variantData.segment));
	}

	public static CreateFromPartial(variantData: Partial<UmbObjectWithVariantProperties>): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData.culture, variantData.segment));
	}

	public static CreateInvariant(): UmbVariantId {
		return Object.freeze(new UmbVariantId(null, null));
	}

	public static FromString(str: string): UmbVariantId {
		const split = str.split('_');
		const culture = split[0] === UMB_INVARIANT_CULTURE ? null : split[0];
		const segment = split[1] ?? null;
		return Object.freeze(new UmbVariantId(culture, segment));
	}

	public readonly culture: string | null = null;
	public readonly segment: string | null = null;

	constructor(culture?: string | null, segment?: string | null) {
		this.culture = (culture === UMB_INVARIANT_CULTURE ? null : culture) ?? null;
		this.segment = segment ?? null;
	}

	public compare(obj: UmbObjectWithVariantProperties): boolean {
		return this.equal(new UmbVariantId(obj.culture, obj.segment));
	}

	public equal(variantId: UmbVariantId): boolean {
		return this.culture === variantId.culture && this.segment === variantId.segment;
	}

	public toString(): string {
		// Currently a direct copy of the VariantPropertiesObjectToString method const.
		return (this.culture || UMB_INVARIANT_CULTURE) + (this.segment ? `_${this.segment}` : '');
	}

	public toCultureString(): string {
		return this.culture || UMB_INVARIANT_CULTURE;
	}

	public toSegmentString(): string {
		return this.segment || '';
	}

	public isCultureInvariant(): boolean {
		return this.culture === null;
	}

	public isSegmentInvariant(): boolean {
		return this.segment === null;
	}

	public isInvariant(): boolean {
		return this.culture === null && this.segment === null;
	}

	public clone(): UmbVariantId {
		return new UmbVariantId(null, this.segment);
	}

	public toObject(): UmbObjectWithVariantProperties {
		return { culture: this.culture, segment: this.segment };
	}

	public toSegmentInvariant(): UmbVariantId {
		return Object.freeze(new UmbVariantId(this.culture, null));
	}
	public toCultureInvariant(): UmbVariantId {
		return Object.freeze(new UmbVariantId(null, this.segment));
	}

	public toCulture(culture: string | null): UmbVariantId {
		return Object.freeze(new UmbVariantId(culture, this.segment));
	}
	public toSegment(segment: string | null): UmbVariantId {
		return Object.freeze(new UmbVariantId(this.culture, segment));
	}

	public toVariant(varyByCulture?: boolean, varyBySegment?: boolean): UmbVariantId {
		return Object.freeze(new UmbVariantId(varyByCulture ? this.culture : null, varyBySegment ? this.segment : null));
	}

	// TODO: needs localization option:
	// TODO: Consider if this should be handled else where, it does not seem like the responsibility of this class, since it contains wordings:
	public toDifferencesString(
		variantId: UmbVariantId,
		invariantMessage: string = 'Invariant',
		unsegmentedMessage: string = 'Unsegmented',
	): string {
		let r = '';

		if (variantId.culture !== this.culture) {
			r = invariantMessage;
		}

		if (variantId.segment !== this.segment) {
			r = (r !== '' ? ' ' : '') + unsegmentedMessage;
		}

		return r;
	}
}
