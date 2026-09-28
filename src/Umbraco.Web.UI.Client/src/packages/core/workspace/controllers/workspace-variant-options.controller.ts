import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbVariantId,
	type UmbEntityVariantModel,
	type UmbEntityVariantOptionModel,
} from '@umbraco-cms/backoffice/variant';
import { mergeObservables, UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbWorkspaceVariantOptionsController<
	VariantModelType extends UmbEntityVariantModel,
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel<VariantModelType>,
> extends UmbControllerBase {
	/* Languages */
	#languages = new UmbArrayState<UmbLanguageDetailModel, string, undefined>([], (x) => x.unique);

	/**
	 * @private
	 * @description - Should not be used by external code.
	 * @internal
	 */
	public readonly languages = this.#languages.asObservable();
	getLanguages(): Array<UmbLanguageDetailModel> | undefined {
		return this.#languages.getValue();
	}
	setLanguages(languages: Array<UmbLanguageDetailModel> | undefined) {
		this.#languages.setValue(languages);
	}

	#variantOptions = new UmbArrayState<Omit<VariantOptionModelType, 'language'>>([], (x) => x.unique);

	public variantOptions = mergeObservables(
		[this.#variantOptions.asObservable(), this.languages],
		([option, languages]) => {
			return option.map((opt) => {
				const lang = languages?.find((x) => x.unique === opt.culture);
				return { ...opt, language: lang };
			});
		},
	) as Observable<Array<VariantOptionModelType>>;

	#currentCoreUniques?: Array<string>;

	constructor(
		host: UmbControllerHost,
		varyByCulture: Observable<boolean | undefined>,
		varyBySegment: Observable<boolean | undefined>,
		variants: Observable<Array<VariantModelType>>,
	) {
		super(host);

		this.observe(
			mergeObservables(
				[varyByCulture, varyBySegment, variants, this.languages],
				([varyByCulture, varyBySegment, variants, languages]) => {
					this.#variantOptions.mute();

					const newVariantOptions = this.#processLanguageVariantOptions(
						varyByCulture,
						varyBySegment,
						variants,
						languages,
					);

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
			),
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
			return [];
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
				variant: undefined, // We do not store variant-data for segments. [NL]
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
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_setOptionFilter(filter: (filter: Omit<VariantOptionModelType, 'language'>) => boolean) {
		if (this.#hasFilter) return;
		this.#hasFilter = true;
		this.variantOptions = this.variantOptions.pipe(map((options) => options.filter((option) => filter(option))));
	}

	public add(options: Array<Omit<VariantOptionModelType, 'language'>>) {
		this.#variantOptions.append(options);
	}

	public remove(uniques: Array<string>) {
		this.#variantOptions.remove(uniques);
	}
}
