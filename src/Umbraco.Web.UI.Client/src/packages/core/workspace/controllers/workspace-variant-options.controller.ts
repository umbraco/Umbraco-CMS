import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbVariantId,
	type UmbEntityVariantModel,
	type UmbEntityVariantOptionModel,
	type UmbEntityVariantOptionSourceModel,
} from '@umbraco-cms/backoffice/variant';
import { mergeObservables, UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Manages the variant options of a workspace: the options derived from the variation of the entity and the
 * available languages, plus any options added by other extensions, like segments.
 * @template VariantModelType - The variant model of the entity.
 * @template VariantOptionModelType - The variant option model exposed to consumers.
 */
export class UmbWorkspaceVariantOptionsController<
	VariantModelType extends UmbEntityVariantModel,
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel<VariantModelType>,
> extends UmbControllerBase {
	/* Languages */
	#languages = new UmbArrayState<UmbLanguageDetailModel, string, undefined>(undefined, (x) => x.unique);

	/**
	 * @private
	 * @description - Should not be used by external code.
	 * @internal
	 */
	public readonly languages = this.#languages.asObservable();
	getLanguages(): Array<UmbLanguageDetailModel> | undefined {
		return this.#languages.getValue();
	}

	#variantOptions = new UmbArrayState<UmbEntityVariantOptionSourceModel<VariantOptionModelType>, string, undefined>(
		undefined,
		(x) => x.unique,
	);

	/**
	 * The variant options with their language resolved. Options whose language cannot be found are left out. Emits `undefined` until the variation of the entity and the languages are known.
	 */
	public variantOptions = mergeObservables(
		[this.#variantOptions.asObservable(), this.languages],
		([option, languages]) => {
			if (option === undefined || languages === undefined) return undefined;
			const defaultLang = languages.find((x) => x.isDefault);
			return option.flatMap((opt) => {
				const language = opt.culture ? languages.find((x) => x.unique === opt.culture) : defaultLang;
				if (!language) {
					this.#warnUnresolvedLanguage(opt.unique);
					return [];
				}
				return [{ ...opt, language }];
			});
		},
	) as Observable<undefined | Array<VariantOptionModelType>>;

	#currentCoreUniques?: Array<string>;
	#warnedUniques = new Set<string>();

	#warnUnresolvedLanguage(unique: string) {
		if (this.#warnedUniques.has(unique)) return;
		this.#warnedUniques.add(unique);
		console.warn(`Variant option "${unique}" is left out, as its language could not be found.`);
	}

	constructor(
		host: UmbControllerHost,
		varyByCulture: Observable<boolean | undefined>,
		varyBySegment: Observable<boolean | undefined>,
		variants: Observable<Array<VariantModelType>>,
		languages: Observable<Array<UmbLanguageDetailModel> | undefined>,
	) {
		super(host);

		this.observe(
			languages,
			(languages) => {
				this.#languages.setValue(languages);
			},
			null,
		);

		this.observe(
			mergeObservables([varyByCulture, varyBySegment, variants, this.languages], (sources) => sources),
			([varyByCulture, varyBySegment, variants, languages]) => {
				const newVariantOptions = this.#processLanguageVariantOptions(
					varyByCulture,
					varyBySegment,
					variants,
					languages,
				);

				// Still loading, keep what we have until the core options can be computed.
				if (newVariantOptions === undefined) return;

				this.#variantOptions.mute();

				if (newVariantOptions) {
					// Figure out which to be removed:
					const removedVariantOptions = this.#currentCoreUniques?.filter(
						(unique) => !newVariantOptions.some((newOption) => newOption.unique === unique),
					);

					if (removedVariantOptions) {
						this.#variantOptions.remove(removedVariantOptions);
					}

					this.#variantOptions.append(newVariantOptions);
				} else if (this.#currentCoreUniques) {
					// Since no options, then remove all previous ones:
					this.#variantOptions.remove(this.#currentCoreUniques);
				}

				this.#currentCoreUniques = newVariantOptions?.map((x) => x.unique) ?? [];

				this.#variantOptions.unmute();
			},
			null,
		);
	}

	#processLanguageVariantOptions(
		varyByCulture: boolean | undefined,
		varyBySegment: boolean | undefined,
		variants: Array<VariantModelType>,
		languages: Array<UmbLanguageDetailModel> | undefined,
	) {
		// Are we in a loading phase?
		if (languages === undefined || varyByCulture === undefined || varyBySegment === undefined) {
			return undefined;
		}

		const varies = varyByCulture || varyBySegment;

		// No variation
		if (!varies) {
			return [
				{
					variant: variants.find((x) => x.culture === null),
					culture: null,
					segment: null,
					unique: new UmbVariantId().toString(),
				} as VariantOptionModelType,
			];
		}

		// Only culture variation
		if (varyByCulture && !varyBySegment) {
			return languages.map((language) => {
				return {
					variant: variants.find((x) => x.culture === language.unique),
					culture: language.unique,
					segment: null,
					unique: new UmbVariantId(language.unique).toString(),
				} as VariantOptionModelType;
			});
		}

		// Only segment variation
		if (!varyByCulture && varyBySegment) {
			const invariantCulture = {
				variant: variants.find((x) => x.culture === null),
				culture: null,
				segment: null,
				unique: new UmbVariantId().toString(),
			} as VariantOptionModelType;
			return [invariantCulture] as Array<VariantOptionModelType>;
		}

		// Culture and segment variation
		if (varyByCulture && varyBySegment) {
			return languages.map((language) => {
				return {
					variant: variants.find((x) => x.culture === language.unique),
					culture: language.unique,
					segment: null,
					unique: new UmbVariantId(language.unique).toString(),
				} as VariantOptionModelType;
			});
		}

		return [];
	}

	#hasFilter?: boolean;

	// TODO: Remove this feature in v.21
	/**
	 * @deprecated - it is not possible to filter variant options from v.21.
	 * @param {(option: UmbEntityVariantOptionSourceModel<VariantOptionModelType>) => boolean} filter - The filter function to apply to variant options.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_setOptionFilter(
		filter: (filter: UmbEntityVariantOptionSourceModel<VariantOptionModelType>) => boolean,
	) {
		if (this.#hasFilter) return;
		this.#hasFilter = true;
		this.variantOptions = this.variantOptions.pipe(map((options) => options?.filter((option) => filter(option))));
	}

	/**
	 * Adds options next to the ones derived from the entity. An option with a unique that already exists replaces it.
	 * @param {Array<UmbEntityVariantOptionSourceModel<VariantOptionModelType>>} options - The options to add. The language is resolved from the culture, and an option whose language cannot be found is left out.
	 */
	public add(options: Array<UmbEntityVariantOptionSourceModel<VariantOptionModelType>>) {
		this.#variantOptions.append(options);
	}

	/**
	 * Removes options by their unique.
	 * @param {Array<string>} uniques - The uniques of the options to remove.
	 */
	public remove(uniques: Array<string>) {
		this.#variantOptions.remove(uniques);
	}

	/**
	 * Removes and adds options in one step, so consumers only see the end result.
	 * @param {Array<string>} uniques - The uniques of the options to remove.
	 * @param {Array<UmbEntityVariantOptionSourceModel<VariantOptionModelType>>} options - The options to add.
	 */
	public replace(uniques: Array<string>, options: Array<UmbEntityVariantOptionSourceModel<VariantOptionModelType>>) {
		this.#variantOptions.mute();
		this.remove(uniques);
		this.add(options);
		this.#variantOptions.unmute();
	}
}
