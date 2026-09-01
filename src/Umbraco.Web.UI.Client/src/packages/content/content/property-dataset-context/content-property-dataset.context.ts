import type { UmbContentWorkspaceContext } from '../workspace/index.js';
import type { UmbContentDetailModel } from '../types.js';
import { UmbElementPropertyDatasetContext } from './element-property-dataset.context.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityVariantModel, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';
import { of, switchMap } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbContentPropertyDatasetContext<
	ContentModel extends UmbContentDetailModel = UmbContentDetailModel,
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
	VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbElementPropertyDatasetContext<
	ContentModel,
	ContentTypeModel,
	UmbContentWorkspaceContext<ContentModel, ContentTypeModel, VariantModelType>
> {
	//
	#pathAddendum = new UmbRoutePathAddendumContext(this);

	#currentVariantId = new UmbObjectState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#currentVariantId.asObservable();
	public readonly culture = this.#currentVariantId.asObservablePart((x) => x?.culture);
	public readonly segment = this.#currentVariantId.asObservablePart((x) => x?.segment);

	#currentVariant = new UmbObjectState<VariantModelType | undefined>(undefined);
	public readonly currentVariant = this.#currentVariant.asObservable();
	public readonly name = this.#currentVariant.asObservablePart((x) => x?.name);

	public readonly IS_CONTENT = true;

	getName(): string | undefined {
		return this._dataOwner.getName(this.getVariantId());
	}
	setName(name: string) {
		this._dataOwner.setName(name, this.getVariantId());
	}

	constructor(
		host: UmbControllerHost,
		dataOwner: UmbContentWorkspaceContext<ContentModel, ContentTypeModel, VariantModelType>,
		variantId: UmbVariantId,
	) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, dataOwner, variantId);

		this.#pathAddendum.setAddendum(variantId ? variantId.toString() : '');

		this.#currentVariantId.setValue(variantId);

		this.observe(
			this.variantId.pipe(
				switchMap((variantId) => (variantId ? this._dataOwner.variantById(variantId) : of(undefined))),
			),
			async (variantInfo) => {
				this.#currentVariant.setValue(variantInfo);
			},
			null,
		);
	}
}
