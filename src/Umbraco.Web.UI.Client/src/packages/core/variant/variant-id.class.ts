import type { UmbObjectWithVariantProperties } from './types.js';

export const UMB_INVARIANT_CULTURE = 'invariant';

/**
 * An identifier representing a Variant. This is at current state a culture and a segment.
 * The identifier is not specific for ContentType Variants, but is used for many type of identification of a culture and a segment. One case is any property of a ContentType can be resolved into a VariantId depending on their structural settings such as Vary by Culture and Vary by Segmentation.
 *
 * Instances are immutable. Methods that derive a variant return a new instance rather than modifying this one.
 */
export class UmbVariantId {
	/**
	 * A shared instance representing the invariant variant, meaning it has no culture and no segment.
	 */
	public static readonly INVARIANT = Object.freeze(new UmbVariantId(null, null));

	/**
	 * Creates a Variant Id from an object holding a culture and a segment.
	 * @param {UmbObjectWithVariantProperties} variantData - The object to read the culture and segment from.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public static Create(variantData: UmbObjectWithVariantProperties): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData.culture, variantData.segment));
	}

	/**
	 * Creates a Variant Id from an object where the culture and/or segment may be omitted.
	 * Omitted values are treated as invariant.
	 * @param {Partial<UmbObjectWithVariantProperties>} variantData - The object to read the culture and segment from.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public static CreateFromPartial(variantData: Partial<UmbObjectWithVariantProperties>): UmbVariantId {
		return Object.freeze(new UmbVariantId(variantData.culture, variantData.segment));
	}

	/**
	 * Gets the Variant Id representing the invariant variant.
	 * @returns {UmbVariantId} The shared {@link UmbVariantId.INVARIANT} instance.
	 */
	public static CreateInvariant(): UmbVariantId {
		return UmbVariantId.INVARIANT;
	}

	/**
	 * Parses a string in the format produced by {@link UmbVariantId.toString} into a Variant Id.
	 * The format is `culture` or `culture_segment`, where a culture of `invariant` means no culture.
	 * @param {string} str - The string to parse.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public static FromString(str: string): UmbVariantId {
		const firstUnderscoreIndex = str.indexOf('_');
		let culture: string | null = firstUnderscoreIndex === -1 ? str : str.substring(0, firstUnderscoreIndex);
		culture = culture === UMB_INVARIANT_CULTURE ? null : culture;

		const segment = firstUnderscoreIndex === -1 ? null : str.substring(firstUnderscoreIndex + 1) || null;

		return Object.freeze(new UmbVariantId(culture, segment));
	}

	/**
	 * The culture of this variant, or `null` when it is culture invariant.
	 */
	public readonly culture: string | null = null;

	/**
	 * The segment of this variant, or `null` when it is segment invariant (unsegmented).
	 */
	public readonly segment: string | null = null;

	/**
	 * @param {string | null} [culture] - The culture. `null`, `undefined` and `'invariant'` all mean culture invariant.
	 * @param {string | null} [segment] - The segment. `null` and `undefined` both mean segment invariant.
	 */
	constructor(culture?: string | null, segment?: string | null) {
		this.culture = (culture === UMB_INVARIANT_CULTURE ? null : culture) ?? null;
		this.segment = segment ?? null;
	}

	/**
	 * Checks whether this Variant Id has the same culture and segment as a given object.
	 * @param {UmbObjectWithVariantProperties} obj - The object to compare with.
	 * @returns {boolean} True if both culture and segment match.
	 */
	public compare(obj: UmbObjectWithVariantProperties): boolean {
		return this.equal(new UmbVariantId(obj.culture, obj.segment));
	}

	/**
	 * Checks whether this Variant Id has the same culture and segment as another Variant Id.
	 * @param {UmbVariantId} variantId - The Variant Id to compare with.
	 * @returns {boolean} True if both culture and segment match.
	 */
	public equal(variantId: UmbVariantId): boolean {
		return this.culture === variantId.culture && this.segment === variantId.segment;
	}

	/**
	 * Checks whether this Variant Id is included in a given array of Variant Ids.
	 * @param {Array<UmbVariantId>} variantIds - The variant ids to check against.
	 * @returns {boolean} True if a matching variant id is found.
	 */
	public oneOf(variantIds: Array<UmbVariantId>): boolean {
		return variantIds.some((v) => v.equal(this));
	}

	/**
	 * Serializes this Variant Id as `culture` or `culture_segment`, using `invariant` when there is no culture.
	 * The result can be parsed back with {@link UmbVariantId.FromString}.
	 * @returns {string} The string representation.
	 */
	public toString(): string {
		// Currently a direct copy of the VariantPropertiesObjectToString method const.
		return (this.culture || UMB_INVARIANT_CULTURE) + (this.segment ? `_${this.segment}` : '');
	}

	/**
	 * Gets the culture part as a string.
	 * @returns {string} The culture, or `invariant` when there is no culture.
	 */
	public toCultureString(): string {
		return this.culture || UMB_INVARIANT_CULTURE;
	}

	/**
	 * Gets the segment part as a string.
	 * @returns {string} The segment, or an empty string when there is no segment.
	 */
	public toSegmentString(): string {
		return this.segment || '';
	}

	/**
	 * Checks whether this Variant Id has no culture.
	 * @returns {boolean} True if the culture is invariant.
	 */
	public isCultureInvariant(): boolean {
		return this.culture === null;
	}

	/**
	 * Checks whether this Variant Id has no segment.
	 * @returns {boolean} True if the segment is invariant.
	 */
	public isSegmentInvariant(): boolean {
		return this.segment === null;
	}

	/**
	 * Checks whether this Variant Id has neither a culture nor a segment.
	 * @returns {boolean} True if both culture and segment are invariant.
	 */
	public isInvariant(): boolean {
		return this.culture === null && this.segment === null;
	}

	/**
	 * Creates a copy of this Variant Id.
	 * @returns {UmbVariantId} A new Variant Id with the same culture and segment.
	 */
	public clone(): UmbVariantId {
		// TODO: Let's make the clone method return a frozen instance like the other methods in v.20.0 [NL]
		return new UmbVariantId(this.culture, this.segment);
	}

	/**
	 * Converts this Variant Id to a plain object.
	 * @returns {UmbObjectWithVariantProperties} An object holding the culture and the segment.
	 */
	public toObject(): UmbObjectWithVariantProperties {
		return { culture: this.culture, segment: this.segment };
	}

	/**
	 * Creates a Variant Id with the same culture as this one and no segment.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public toSegmentInvariant(): UmbVariantId {
		return Object.freeze(new UmbVariantId(this.culture, null));
	}

	/**
	 * Creates a Variant Id with the same segment as this one and no culture.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public toCultureInvariant(): UmbVariantId {
		return Object.freeze(new UmbVariantId(null, this.segment));
	}

	/**
	 * Creates a Variant Id with the given culture and the segment of this one.
	 * @param {string | null} culture - The culture to use.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public toCulture(culture: string | null): UmbVariantId {
		return Object.freeze(new UmbVariantId(culture, this.segment));
	}

	/**
	 * Creates a Variant Id with the given segment and the culture of this one.
	 * @param {string | null} segment - The segment to use.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public toSegment(segment: string | null): UmbVariantId {
		return Object.freeze(new UmbVariantId(this.culture, segment));
	}

	/**
	 * Creates a Variant Id reduced to the dimensions that are allowed to vary.
	 * A dimension that does not vary becomes invariant.
	 * @param {boolean} [varyByCulture] - Whether to keep the culture.
	 * @param {boolean} [varyBySegment] - Whether to keep the segment.
	 * @returns {UmbVariantId} A new Variant Id.
	 */
	public toVariant(varyByCulture?: boolean, varyBySegment?: boolean): UmbVariantId {
		return Object.freeze(new UmbVariantId(varyByCulture ? this.culture : null, varyBySegment ? this.segment : null));
	}

	/**
	 * Describes how a given Variant Id differs from this one, as a display string.
	 * @param {UmbVariantId} variantId - The Variant Id to compare with.
	 * @param {string} [invariantMessage] - The text used when the cultures differ.
	 * @param {string} [unsegmentedMessage] - The text used when the segments differ.
	 * @returns {string} The description, or an empty string when there are no differences.
	 */
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
